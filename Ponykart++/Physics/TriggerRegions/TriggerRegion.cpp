#include "pch.h"
#include <BulletCollision/BroadphaseCollision/btBroadphaseProxy.h>
#include <BulletCollision/CollisionDispatch/btCollisionObject.h>
#include <BulletCollision/CollisionDispatch/btGhostObject.h>
#include <BulletCollision/CollisionShapes/btCollisionShape.h>
#include <BulletCollision/CollisionShapes/btBoxShape.h>
#include <BulletCollision/CollisionShapes/btCapsuleShape.h>
#include <BulletCollision/CollisionShapes/btCylinderShape.h>
#include <BulletCollision/CollisionShapes/btSphereShape.h>
#include <BulletDynamics/Dynamics/btDiscreteDynamicsWorld.h>
#include <LinearMath/btDefaultMotionState.h>
#include <OgreEntity.h>
#include <OgreQuaternion.h>
#include <OgreSceneManager.h>
#include "Core/Settings.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Misc/bulletExtensions.h"
#include "Misc/ogreExtensions.h"
#include "Physics/CollisionObjectDataHolder.h"
#include "Physics/PhysicsMain.h"
#include "Physics/TriggerRegions/TriggerRegion.h"
#include "Physics/TriggerRegions/TriggerReporter.h"

using namespace std;
using namespace Ogre;
using namespace Extensions;
using namespace Ponykart;
using namespace Ponykart::Physics;

/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
TriggerRegion::TriggerRegion(const string& name, const Vector3& position, btCollisionShape* shape)
: TriggerRegion(name, position, Ogre::Quaternion::IDENTITY, shape) 
{ 
}

/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
/// @param orientation a degree vector
TriggerRegion::TriggerRegion(const std::string& Name, const Ogre::Vector3& position, const Ogre::Quaternion& orientation, btCollisionShape* shape)
: name(Name)
{
	// ogre
	SceneManager* sceneMgr = LKernel::gSceneManager;

	node = sceneMgr->getRootSceneNode()->createChildSceneNode(name);
	if (Settings::EnableGlowyRegions) 
	{
		// Temporary varibales for the switch
		Vector3 vec;
		float dim;
		// make a mesh for the region depending on what its type is
		switch (shape->getShapeType()) 
		{
		case BroadphaseNativeTypes::BOX_SHAPE_PROXYTYPE:
			entity = sceneMgr->createEntity(name, "primitives/box.mesh");
			node->setScale(toOgreVector3(((btBoxShape*)shape)->getHalfExtentsWithoutMargin() * 2));
			break;
		case BroadphaseNativeTypes::CAPSULE_SHAPE_PROXYTYPE:
			entity = sceneMgr->createEntity(name, "primitives/cylinder.mesh");
			vec.y = ((btCapsuleShape*)shape)->getHalfHeight() * 2;
			vec.x = vec.z = ((btCapsuleShape*)shape)->getRadius() * 2;
			node->setScale(vec);
			break;
		case BroadphaseNativeTypes::CYLINDER_SHAPE_PROXYTYPE:
			entity = sceneMgr->createEntity(name, "primitives/cylinder.mesh");
			vec.y = ((btCylinderShape*)shape)->getHalfExtentsWithoutMargin().y();
			vec.x = vec.z = ((btCylinderShape*)shape)->getRadius() * 2;
			node->setScale(vec);
			break;
		case BroadphaseNativeTypes::SPHERE_SHAPE_PROXYTYPE:
			entity = sceneMgr->createEntity(name, "primitives/sphere.mesh");
			dim = ((btSphereShape*)shape)->getRadius() * 2;
			node->setScale(dim, dim, dim);
			break;
		default:
			// for things like meshes, convex hulls, etc
			entity = sceneMgr->createEntity(name, "primitives/box.mesh");
			break;
		}
		setGlowColor(BalloonGlowColour::red);
		entity->setCastShadows(false);

		node->attachObject(entity);
	}
	node->setPosition(position);
	node->setOrientation(orientation);

	// physics
	btTransform transform = btTransform(toBtQuaternion(orientation), toBtVector3(position));
	//Matrix4 transform;
	//transform.makeTransform(position, Vector3::UNIT_SCALE, orientation);

	auto motionState = new btDefaultMotionState();
	motionState->setWorldTransform(transform);

	// thanks to kloplop321 in #ogre3d for his help with this
	ghost = new btGhostObject();
	ghost->setCollisionShape(shape);
	ghost->setWorldTransform(transform);
	ghost->setUserPointer(new CollisionObjectDataHolder(ghost, PonykartCollisionGroups::Triggers, name));

	int flags = ghost->getCollisionFlags();
	ghost->setCollisionFlags(flags | (btCollisionObject::CF_NO_CONTACT_RESPONSE 
											| btCollisionObject::CF_CUSTOM_MATERIAL_CALLBACK));
	LKernel::getG<PhysicsMain>()->getWorld()->addCollisionObject(ghost, (short)PonykartCollisionGroups::Triggers, 
																(short)PonykartCollidesWithGroups::Triggers);


	// then add this to the trigger reporter
	LKernel::getG<TriggerReporter>()->getRegions().emplace(name, this);
}

BalloonGlowColour TriggerRegion::getGlowColor()
{
	return _balloonColor;
}

void TriggerRegion::setGlowColor(BalloonGlowColour color)
{
	_balloonColor = color;
	if (Settings::EnableGlowyRegions)
		entity->setMaterialName(string("BalloonGlow_") + balloonGlowColorNames[(unsigned)color]);
}

void TriggerRegion::invokeTrigger(btRigidBody* otherBody, TriggerReportFlags flags, CollisionReportInfo* info)
{
	// at the moment this only triggers when the "main" shape of an actor enters. Do we want to change this?
	if (onTrigger.size()) 
	{
#if DEBUG
		try {
#endif
			for (auto& fun : onTrigger)
				fun(this, otherBody, flags, info);
#if DEBUG
		}
		catch (std::exception& e) {
			LKernel::log("Exception at TriggerRegion.InvokeTrigger: " + *e.what());
		}
#endif
	}
}