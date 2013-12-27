#include <BulletCollision/CollisionShapes/btCollisionShape.h>
#include <BulletDynamics/Dynamics/btRigidBody.h>
#include <BulletDynamics/Dynamics/btDiscreteDynamicsWorld.h>
#include <LinearMath/btDefaultMotionState.h>
#include "Actors/LThing.h"
#include "Actors/Components/BillboardSetComponent.h"
#include "Actors/Components/ModelComponent.h"
#include "Actors/Components/RibbonComponent.h"
#include "Actors/Components/ShapeComponent.h"
#include "Actors/Components/SoundComponent.h"
#include "Core/IDs.h"
#include "Core/Animation/AnimationBlender.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"
#include "Lua/LuaMain.h"
#include "Misc/bulletExtensions.h"
#include "Misc/ogreExtensions.h"
#include "Muffin/ThingBlock.h"
#include "Physics/CollisionGroups.h"
#include "Physics/CollisionObjectDataHolder.h"
#include "Physics/CollisionShapeManager.h"
#include "Physics/PhysicsMain.h"
#include "Physics/Materials/PhysicsMaterial.h"
#include "Physics/Materials/PhysicsMaterialFactory.h"
#include "Physics/MotionStates/OgreMotionState.h"
#include "Thing/ThingDefinition.h"
#include "TokenHolder.h"

using namespace std;;
using namespace Ogre;
using namespace PonykartParsers;
using namespace Ponykart::Actors;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Lua;
using namespace Ponykart::Physics;

LThing::LThing(ThingBlock* thingTemplate, ThingDefinition* def)
{
	soundsNeedUpdate = false;
	id = IDs::incremental();
	name = thingTemplate->getThingName();

	// get our three basic transforms
	spawnPosition = thingTemplate->getVectorProperty("position");

	spawnOrientation = thingTemplate->getQuatProperty("orientation", Quaternion::IDENTITY);
	if (spawnOrientation == Quaternion::IDENTITY)
		spawnOrientation = degreeVectorToGlobalQuaternion(thingTemplate->getVectorProperty("rotation", Vector3::ZERO));

	spawnScale = thingTemplate->getVectorProperty("scale", Vector3::UNIT_SCALE);

	// and start setting up this thing!
	preSetup(thingTemplate, def);
	setupOgre(thingTemplate, def);

	initialiseComponents(thingTemplate, def);

	rootNode->setPosition(spawnPosition);
	rootNode->setOrientation(spawnOrientation);
	// Only scale up the root node if it doesn't have any physics things attached - bullet really does not like scaling.
	// Need a few variations of identical objects with different scales? Gonna have to make different .things for them.
	// Though it might be easier to just have one general .thing for them, and all it does is run a script that randomly gets one of the others.
	if (!shapeComponents.size())
		rootNode->scale(spawnScale);
	rootNode->setInitialState();

	postInitialiseComponents(thingTemplate, def);

	setupPhysics(thingTemplate, def);

	// get our script token and run it, if it has one and if this thing was created on the fly instead of through a .muffin file
	auto newscriptIt=def->getStringTokens().find("script");
	if (newscriptIt != def->getStringTokens().end())
	{
		script = newscriptIt->second;
		runScript();
	}

	deleteIfStaticOrInstanced(def);
}

LThing::~LThing() // TODO: The C# version takes "bool disposing" as an argument. Check if this is correct.
{
	auto sceneMgr = LKernel::gSceneManager;
	auto world = LKernel::getG<PhysicsMain>()->getWorld();
	bool valid = LKernel::getG<LevelManager>()->getIsValidLevel();

	// delete all of our components
	for (auto x : modelComponents) delete x;
	for (auto x : shapeComponents) delete x;
	for (auto x : ribbonComponents) delete x;
	for (auto x : billboardSetComponents) delete x;
	for (auto x : soundComponents) delete x;

	// these are conditional in case we want to delete stuff in the middle of a level
	if (rootNode != nullptr)
	{
		if (valid) sceneMgr->destroySceneNode(rootNode);
		delete rootNode;
	}

	if (body != nullptr)
	{
		if (valid) world->removeRigidBody(body);
		delete body;
	}
}

void LThing::changeAnimation(std::string animationName)
{
	for (auto mcomp : modelComponents)
	{
		if (mcomp->animationBlender != nullptr && mcomp->getEntity()->getAllAnimationStates()->hasAnimationState(animationName))
		{
			mcomp->animationBlender->blend(animationName, AnimationBlendingTransition::BlendSwitch, 0, true);
			mcomp->animationBlender->addTime((int) id);
		}
	}
}


bool LThing::getSoundsNeedUpdate() const
{
    return soundsNeedUpdate;
}

void LThing::setSoundsNeedUpdate(bool value)
{
	// Only change the sound components if this property is changing from false to true.
	// They will change back to false by themselves and we don't need to do the foreach loop if they're already true
	if (value && soundsNeedUpdate != value)
		for (auto soundComponent : soundComponents)
			soundComponent->needUpdate = value;
	soundsNeedUpdate = value;
}

SceneNode* LThing::getRootNode()
{
	return rootNode;
}

btRigidBody* LThing::getBody()
{
	return body;
}

Vector3 LThing::getBodyLinearVelocity() const
{
	btVector3 v = body->getLinearVelocity();
	return Vector3(v.getX(), v.getY(), v.getZ());
}

const std::vector<ModelComponent*>& LThing::getModelComponents() const
{
	return modelComponents;
}

void LThing::initialiseComponents(ThingBlock* thingTemplate, ThingDefinition* def)
{
	int vsize;

	// Ogre
	vsize=def->getModelBlocks().size();
	if (vsize>0)
	{
		modelComponents.reserve(vsize);
		for (auto mblock : def->getModelBlocks())
			modelComponents.push_back(new ModelComponent(this, thingTemplate, mblock, def));
	}
	// Bullet
	vsize=def->getShapeBlocks().size();
	if (vsize>0)
	{
		shapeComponents.reserve(vsize);
		for (auto sblock : def->getShapeBlocks())
			shapeComponents.push_back(new ShapeComponent(this, sblock));
	}
	// Ribbons
	vsize=def->getRibbonBlocks().size();
	if (vsize>0)
	{
		ribbonComponents.reserve(vsize);
		for (auto rblock : def->getRibbonBlocks())
			ribbonComponents.push_back(new RibbonComponent(this, thingTemplate, rblock));
	}
	// Billboard sets
	vsize=def->getBillboardSetBlocks().size();
	if (vsize>0)
	{
		billboardSetComponents.reserve(vsize);
		for (auto bblock : def->getBillboardSetBlocks())
			billboardSetComponents.push_back(new BillboardSetComponent(this, thingTemplate, bblock));
	}
	// Sounds
	vsize=def->getSoundBlocks().size();
	if (vsize>0)
	{
		soundComponents.reserve(vsize);
		for (auto sblock : def->getSoundBlocks())
			soundComponents.push_back(new SoundComponent(this, thingTemplate, sblock));
	}
}

void LThing::setupOgre(ThingBlock* thingTemplate, ThingDefinition* def)
{
	auto sceneMgr = LKernel::gSceneManager;
	ostringstream ssId; ssId<<id;
	string nameId = name+ssId.str();

	// Create our root node. Need to check for map regions
	string mapRegion = thingTemplate->getStringProperty("MapRegion", string());
	if (mapRegion.empty())
		rootNode = sceneMgr->getRootSceneNode()->createChildSceneNode(nameId); // no map region, continue on as normal
	else
	{
		string mapRegionNodeName = mapRegion + "Node";
		// There is a map region, make our root node a child of a node with the region's name.
		// First check to see if that node exists already.
		if (sceneMgr->hasSceneNode(mapRegionNodeName)) // If it does, just attach our node to it
			rootNode = sceneMgr->getSceneNode(mapRegionNodeName)->createChildSceneNode(nameId);
		else // If it doesn't, create it first, then attach our node to it
		{
			SceneNode* newSceneNode = sceneMgr->getRootSceneNode()->createChildSceneNode(mapRegionNodeName);
			rootNode = newSceneNode->createChildSceneNode(nameId);
		}
	}
}

void LThing::setupPhysics(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def)
{
	if (shapeComponents.size()<=0) // If we have no shape components then we don't set up physics
		return;

	preSetUpBodyInfo(def);
	setUpBodyInfo(def);
	postSetUpBodyInfo(def);
	createBody(def);
	postCreateBody(def);
	setBodyUserObject();
}

/// Runs the thing's script, if it has one.
/** If this thing was made from a .muffin, this is called from Level.RunLevelScripts to make sure it runs after everything else is created.\n
	If it was made on the fly, it runs at the end of the constructor, as long as it's a valid level of course. */
void LThing::runScript()
{
	if (!script.empty())
		LKernel::getG<LuaMain>()->doFunctionForLThing(script, this);
}

void LThing::deleteIfStaticOrInstanced(PonykartParsers::ThingDefinition* def)
{
	if (def->getBoolProperty("Static", false) || def->getBoolProperty("Instanced", false))
	{
		auto sceneMgr = LKernel::gSceneManager;

		// this bool is to check we only fully dispose lthings if ALL of their model components are static/instanced
		bool removedAllModelComponents = true;
		for (auto mc : modelComponents) // delete all of the model components
			if (mc->getEntity() == nullptr)
				delete mc;
			else
				removedAllModelComponents = false;

		// if we have no ribbons, billboards, or sounds, we can get rid of the root node
		if (removedAllModelComponents && !ribbonComponents.size() && !billboardSetComponents.size() && !soundComponents.size())
		{
			if (!shapeComponents.size()) // if we have no shapes, we can get rid of everything
				delete this;
			// but otherwise we can still get rid of the root scene node
			else
			{
				sceneMgr->destroySceneNode(rootNode);
				delete rootNode;
				rootNode = nullptr;
			}
		}
	}
}

void LThing::setUpBodyInfo(PonykartParsers::ThingDefinition* def)
{
	// set up our collision shapes
	btCollisionShape* shape = LKernel::getG<CollisionShapeManager>()->createAndRegisterShape(this, def);

	// get the physics type and set up the mass of the body
	ThingEnum physicsType = def->getEnumProperty("physics");
	float mass = physicsType & ThingEnum::Static ? 0 : def->getFloatProperty("mass", 1);

	// create our construction info thingy
	btVector3 inertia;
	shape->calculateLocalInertia(mass, inertia);

	// if it's static and doesn't have a sound, we don't need a mogre motion state because we'll be disposing of the root node afterwards
	if (def->getBoolProperty("Static", false) && !soundComponents.size())
		motionState = new btDefaultMotionState;
	else
		motionState = getInitializationMotionState();

	info = new btRigidBody::btRigidBodyConstructionInfo(mass, motionState, shape, inertia);

	// physics material stuff from a .physmat file
	string physmat = def->getStringProperty("PhysicsMaterial", "Default");
	LKernel::getG<PhysicsMaterialFactory>()->applyMaterial(info, physmat);

	// we can override some of them in the .thing file
	auto ftoks = def->getFloatTokens();
	if (ftoks.find("bounciness") != ftoks.end())
		info->m_restitution = def->getFloatProperty("bounciness", PhysicsMaterial::DEFAULT_BOUNCINESS);
	if (ftoks.find("friction") != ftoks.end())
		info->m_friction = def->getFloatProperty("friction", PhysicsMaterial::DEFAULT_FRICTION);
	if (ftoks.find("angulardamping") != ftoks.end())
		info->m_angularDamping = def->getFloatProperty("angulardamping", PhysicsMaterial::DEFAULT_ANGULAR_DAMPING);
	if (ftoks.find("lineardamping") != ftoks.end())
		info->m_linearDamping = def->getFloatProperty("lineardamping", PhysicsMaterial::DEFAULT_LINEAR_DAMPING);

	// choose which group to use for a default
	ThingEnum defaultGroup;
	if (physicsType & ThingEnum::Dynamic)
		defaultGroup = ThingEnum::Default;
	else if (physicsType & ThingEnum::Static)
		defaultGroup = ThingEnum::Environment;
	else // kinematic
		defaultGroup = ThingEnum::Default;

	// collision group
	ThingEnum teCollisionGroup = def->getEnumProperty("CollisionGroup", defaultGroup);
	int pcg=0;
	if (teCollisionGroup&ThingEnum::All) {pcg=(int)PonykartCollisionGroups::All; goto enumPCGConversionDone;}
	if (teCollisionGroup&ThingEnum::Default) pcg|=(int)PonykartCollisionGroups::Default;
	if (teCollisionGroup&ThingEnum::Environment) pcg|=(int)PonykartCollisionGroups::Environment;
	if (teCollisionGroup&ThingEnum::Affectors) pcg|=(int)PonykartCollisionGroups::Affectors;
	if (teCollisionGroup&ThingEnum::Road) pcg|=(int)PonykartCollisionGroups::Road;
	if (teCollisionGroup&ThingEnum::Triggers) pcg|=(int)PonykartCollisionGroups::Triggers;
	if (teCollisionGroup&ThingEnum::Karts) pcg|=(int)PonykartCollisionGroups::Karts;
	if (teCollisionGroup&ThingEnum::InvisibleWalls) pcg|=(int)PonykartCollisionGroups::InvisibleWalls;
	enumPCGConversionDone:
	if (pcg!=(int)teCollisionGroup)
		throw string("Invalid collision group!");
	collisionGroup = (PonykartCollisionGroups)pcg;

	// collides-with group
	ThingEnum teCollidesWith = def->getEnumProperty("CollidesWith", defaultGroup);
	int pcwg=0;
	if (teCollidesWith&ThingEnum::All) {pcwg=(int)PonykartCollidesWithGroups::All; goto enumPCWGConversionDone;}
	if (teCollidesWith&ThingEnum::Default) pcwg|=(int)PonykartCollidesWithGroups::Default;
	if (teCollidesWith&ThingEnum::Environment) pcwg|=(int)PonykartCollidesWithGroups::Environment;
	if (teCollidesWith&ThingEnum::Affectors) pcwg|=(int)PonykartCollidesWithGroups::Affectors;
	if (teCollidesWith&ThingEnum::Road) pcwg|=(int)PonykartCollidesWithGroups::Road;
	if (teCollidesWith&ThingEnum::Triggers) pcwg|=(int)PonykartCollidesWithGroups::Triggers;
	if (teCollidesWith&ThingEnum::Karts) pcwg|=(int)PonykartCollidesWithGroups::Karts;
	if (teCollidesWith&ThingEnum::InvisibleWalls) pcwg|=(int)PonykartCollidesWithGroups::InvisibleWalls;
	enumPCWGConversionDone:
	if (pcwg!=(int)teCollidesWith)
		throw string("Invalid collides-with group!");
	collidesWith = (PonykartCollidesWithGroups)pcwg;

	// update the transforms
	Matrix4 trans;
	trans.makeTransform(spawnPosition, spawnScale, spawnOrientation);
	btTransform btTrans{toBtQuaternion(trans.extractQuaternion()), toBtVector3(trans.getTrans())};
	info->m_startWorldTransform = btTrans;
	motionState->setWorldTransform(btTrans);
}

btMotionState* LThing::getInitializationMotionState()
{
	if (soundComponents.size())
		return new OgreMotionState(spawnPosition, spawnOrientation, rootNode, this);
	else
		return new OgreMotionState(spawnPosition, spawnOrientation, rootNode);
}

void LThing::createBody(ThingDefinition* def)
{
	body = new btRigidBody(*info);

	// stick on our flags
	ThingEnum te = def->getEnumProperty("physics");
	int flags = body->getCollisionFlags();
	if (te & ThingEnum::Static)
		flags |= btCollisionObject::CF_STATIC_OBJECT;
	else if (te &ThingEnum::Kinematic)
		flags |= btCollisionObject::CF_KINEMATIC_OBJECT;

	if (def->getBoolProperty("CollisionEvents", false))
		flags |= btCollisionObject::CF_CUSTOM_MATERIAL_CALLBACK;

	if (def->getBoolProperty("DisableVisualization", false))
		flags |= btCollisionObject::CF_DISABLE_VISUALIZE_OBJECT;
	body->setCollisionFlags(flags);

	body->setWorldTransform(info->m_startWorldTransform);

	LKernel::getG<PhysicsMain>()->getWorld()->addRigidBody(body, (int)collisionGroup, (int)collidesWith);

	if (def->getBoolProperty("Deactivated", false))
		body->forceActivationState(WANTS_DEACTIVATION);
}

void LThing::setBodyUserObject()
{
	body->setUserPointer(new CollisionObjectDataHolder(this));
}
