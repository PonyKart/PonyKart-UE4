#include "pch.h"
#include <BulletDynamics/Dynamics/btDiscreteDynamicsWorld.h>
#include <BulletDynamics/ConstraintSolver/btSequentialImpulseConstraintSolver.h>
#include <BulletCollision/CollisionDispatch/btDefaultCollisionConfiguration.h>
#include "Core/Pauser.h"
#include "Kernel/LKernelOgre.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"
#include "Physics/PhysicsMain.h"

using namespace Ogre;
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
