#include "pch.h"
#include <fstream>
#include <BulletDynamics/Dynamics/btDiscreteDynamicsWorld.h>
#include <BulletDynamics/Dynamics/btRigidBody.h>
#include <BulletDynamics/ConstraintSolver/btSequentialImpulseConstraintSolver.h>
#include <BulletCollision/CollisionDispatch/btDefaultCollisionConfiguration.h>
#include "Core/Pauser.h"
#include "Kernel/LKernelOgre.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"
#include "Misc/bulletExtensions.h"
#include "Physics/CollisionObjectDataHolder.h"
#include "Physics/CollisionShapeManager.h"
#include "Physics/PhysicsMain.h"
#include "Physics/DotSceneLoader/DotSceneLoader.h"

using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Core;
using namespace Ponykart::Physics;
using namespace Ponykart::LKernel;
using namespace Ponykart::Levels;

// Define static members
bool PhysicsMain::drawLines = false;
bool PhysicsMain::slowMo = false;
const int PhysicsMain::_maxSubsteps = 60; //45;
const float PhysicsMain::_fixedTimestep = 1.0f / 60.0f; //1.0/45.0;
ContactAdded PhysicsMain::contactAdded;
PhysicsWorldEvent PhysicsMain::postCreateWorld;
PhysicsSimulateEvent PhysicsMain::preSimulate;
PhysicsSimulateEvent PhysicsMain::postSimulate;
PhysicsSimulateEvent PhysicsMain::finaliseBeforeSimulation;


PhysicsMain::PhysicsMain()
{
	log("[Loading] Creating PhysicsMain...");
	LevelManager::onLevelUnload.push_back(std::bind(&PhysicsMain::onLevelUnload, this, std::placeholders::_1));
}

btDiscreteDynamicsWorld* PhysicsMain::getWorld()
{
    return world;
}

void PhysicsMain::onLevelUnload(LevelChangedEventArgs* eventArgs)
{
	LKernel::gRoot->removeFrameListener(this);

	if (world)
	{
		for (int a = 0; a < world->getCollisionObjectArray().size(); a++)
		{
			auto obj = world->getCollisionObjectArray()[a];
			if (obj)
			{
				world->removeCollisionObject(obj);
				delete obj;
			}
		}

		delete broadphase; broadphase=nullptr;
		delete solver; solver=nullptr;
		delete dcc; dcc=nullptr;
		delete dispatcher; dispatcher=nullptr;

		delete world; world=nullptr;
	}
}

bool PhysicsMain::frameEnded(const Ogre::FrameEvent& evt)
{
	if (Pauser::isPaused || world==nullptr)
		return true;

	// run the events that go just before we simulate
	for (auto& fun : preSimulate)
		fun(world, evt);

	for (auto& fun : finaliseBeforeSimulation)
		fun(world, evt);

	world->stepSimulation(PhysicsMain::slowMo ? evt.timeSinceLastFrame / 10.f : evt.timeSinceLastFrame, _maxSubsteps, _fixedTimestep);

	// run the events that go just after we simulate
	for (auto& fun : postSimulate)
		fun(world, evt);

	if (PhysicsMain::drawLines)
		world->debugDrawWorld();

	//foreach (var item in LKernel.GetG<LevelManager>().CurrentLevel.Things) {
	//	if (item.Value.Name.Contains("Apple") && !item.Value.Name.Contains("Tree")) {
	//		System.Console.WriteLine(item.Value.Body.IsActive);
	//	}
	//}

	return true;
}

void PhysicsMain::loadPhysicsLevel(const std::string& levelName) 
{
	log("[Loading] Setting up Physics world and loading shapes from .scene file");

	createWorld(levelName);

	// creates collision meshes out of static objects
	// get the scene manager
	SceneManager* sceneMgr = LKernel::getG<SceneManager>();
	// parse our .scene file

	// No standard way to check if a file exists without opening it.
	std::ifstream fileExistsCheck(Settings::WorldFileLocation + levelName + ".scene");
	if (fileExistsCheck)
	{
		fileExistsCheck.close();
		DotSceneLoader dsl;
		dsl.parseDotScene(levelName + ".scene", ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME, 
							sceneMgr, sceneMgr->getRootSceneNode());

		// then go through each of the static objects and turn them into trimeshes.
		for (const std::string& s : dsl.staticObjects) 
		{
			// apparently triangle meshes only screw up if you turn on debug drawing for them. No I don't know why the fuck that should matter.
			Entity* dslEnt = sceneMgr->getEntity(s);
			SceneNode* dslNode = sceneMgr->getSceneNode(s);

			btCollisionShape* shape;

			std::string bulletFilePath = dslNode->getName() + ".bullet";

			shape = LKernel::getG<CollisionShapeManager>()->getShapeFromFile(bulletFilePath, dslEnt, dslNode);

			// then do the rest as usual
			btRigidBody::btRigidBodyConstructionInfo info{ 0, new btDefaultMotionState(), shape };
			auto body = new btRigidBody(info);
			body->setCollisionFlags(btCollisionObject::CF_STATIC_OBJECT | btCollisionObject::CF_DISABLE_VISUALIZE_OBJECT);
			body->setUserPointer(new CollisionObjectDataHolder(body, PonykartCollisionGroups::Road, dslNode->getName()));
			world->addRigidBody(body, (short)PonykartCollisionGroups::Road, (short)PonykartCollidesWithGroups::Road);
		}
	}

	// make a ground plane for us
	createGroundPlane(-15);

	// run some events
	if (postCreateWorld.size())
		for (auto& fun : postCreateWorld)
			fun(world);
}

void PhysicsMain::createWorld(const std::string& levelName)
{
	log("[PhysicsMain] Creating new world...");
	// have to make more of these every level because disposing the world apparently disposes of them too.
	broadphase = new btDbvtBroadphase();
	solver = new btSequentialImpulseConstraintSolver();
	dcc = new btDefaultCollisionConfiguration();
	dispatcher = new btCollisionDispatcher(dcc);
	// set up this stuff... not quite sure what it's for, but you need it if you want the CCD to work for the karts
	dispatcher->registerCollisionCreateFunc(CONVEX_HULL_SHAPE_PROXYTYPE, CONVEX_HULL_SHAPE_PROXYTYPE,
		dcc->getCollisionAlgorithmCreateFunc(TRIANGLE_MESH_SHAPE_PROXYTYPE, TRIANGLE_MESH_SHAPE_PROXYTYPE));
	dispatcher->registerCollisionCreateFunc(CONVEX_HULL_SHAPE_PROXYTYPE, CONVEX_HULL_SHAPE_PROXYTYPE,
		dcc->getCollisionAlgorithmCreateFunc(CONVEX_HULL_SHAPE_PROXYTYPE, CONVEX_HULL_SHAPE_PROXYTYPE));

	world = new btDiscreteDynamicsWorld(dispatcher, broadphase, solver, dcc);
	// and then turn on CCD
	world->getDispatchInfo().m_useContinuous = true;

	world->setGravity(btVector3(0, Settings::Gravity, 0));

	if (!contactAdded.target<ContactAddedCallback>())
		throw std::string("PhysicsMain::contactAdded's target is null");
	else
		gContactAddedCallback = *contactAdded.target<ContactAddedCallback>();
}

void PhysicsMain::createGroundPlane(float yposition) 
{
	// make an infinite plane so we don't fall forever. TODO: hook up an event so when we collide with this, we respawn back on the track
	btTransform matrix(toBtQuaternion(Quaternion(0, 0, 0, 1)), toBtVector3(Vector3(0, yposition, 0)));

	btCollisionShape* groundShape = new btStaticPlaneShape(toBtVector3(Vector3::NEGATIVE_UNIT_Y), 1);
	btRigidBody::btRigidBodyConstructionInfo groundInfo{ 0, new btDefaultMotionState(matrix), groundShape };
	auto groundBody = new btRigidBody(groundInfo);
	groundBody->setUserPointer(new CollisionObjectDataHolder(groundBody, PonykartCollisionGroups::Environment, "ground"));
	groundBody->setCollisionFlags(btCollisionObject::CF_STATIC_OBJECT | btCollisionObject::CF_DISABLE_VISUALIZE_OBJECT);
	world->addRigidBody(groundBody, (short)PonykartCollisionGroups::Environment, (short)PonykartCollidesWithGroups::Environment);
}
