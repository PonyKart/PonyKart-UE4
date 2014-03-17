#include "pch.h"
#include "Actors/LThing.h"
#include "Actors/InstancedGeometryManager.h"
#include "Actors/StaticGeometryManager.h"
#include "Actors/Components/ModelComponent.h"
#include "Core/IDs.h"
#include "Core/Options.h"
#include "Core/Animation/AnimationBlender.h"
#include "Core/Animation/AnimationManager.h"
#include "Kernel/LKernel.h"
#include "Misc/ogreExtensions.h"
#include "Thing/ThingDefinition.h"
#include "Thing/Blocks/ModelBlock.h"
#include "Thing/Blocks/ThingBlock.h"

using namespace std;
using namespace Ogre;
using namespace Extensions;
using namespace PonykartParsers;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;

ModelComponent::ModelComponent(LThing* lthing, ThingBlock* thingTemplate, ModelBlock* block, ThingDefinition* def)
: 	spawnPosition(block->getVectorProperty("position", Vector3::ZERO)),
	spawnOrientation(initSpawnOrientation(block)),
	spawnScale(block->getVectorProperty("scale", Vector3::UNIT_SCALE))
{
	id = IDs::incremental();
	owner = lthing;
	auto sceneMgr = LKernel::gSceneManager;

	name = block->getStringProperty("name", thingTemplate->getThingName());

	ThingEnum shad = block->getEnumProperty("CastsShadows", ThingEnum::Some);
	// if we're static, set up the static geometry
	// don't set up static geometry if we want to cast shadows though, since static geometry doesn't work with shadows
	if ((block->getBoolProperty("static", false) || def->getBoolProperty("static", false))
		// make static if we never want shadows
		&& (shad == ThingEnum::None
		// or if the mesh has "some" shadows but we don't want any
			|| (shad == ThingEnum::Some && Options::shadowDetail == ShadowDetailOption::None)
		// or if the mesh has "many" shadows but we only want those with "some"
			|| (shad == ThingEnum::Many && Options::shadowDetail != ShadowDetailOption::Many)))
	{
		LKernel::getG<StaticGeometryManager>()->add(this, thingTemplate, block, def);
		entity = nullptr;
	}
	else if (block->getBoolProperty("instanced", false) || def->getBoolProperty("instanced", false))
	{
		LKernel::getG<InstancedGeometryManager>()->add(this, thingTemplate, block, def);
		entity = nullptr;
	}
	// for attachments
	else if (block->getBoolProperty("Attached", false))
	{
		setupEntity(sceneMgr, block);
		setupAnimation(block);

		string boneName = block->getStringProperty("AttachBone");
		int modelComponentID = block->getFloatProperty("AttachComponentID"); // TODO: BUG:? Check float->int conversion
		Quaternion offsetQuat = block->getQuatProperty("AttachOffsetOrientation", Quaternion::IDENTITY);
		Vector3 offsetVec = block->getVectorProperty("AttachOffsetPosition", Vector3::ZERO);

		lthing->getModelComponents()[modelComponentID]->getEntity()->attachObjectToBone(boneName, entity, offsetQuat, offsetVec);
	}
	// otherwise continue as normal
	else
	{
		ostringstream ss; ss<<name<<"Node"<<id;
		node = lthing->getRootNode()->createChildSceneNode(ss.str());

		node->setPosition(spawnPosition);
		node->setOrientation(spawnOrientation);
		node->setScale(spawnScale);

		node->setInheritScale(block->getBoolProperty("InheritScale", true));
		node->setInheritOrientation(block->getBoolProperty("InheritOrientation", true));
		node->setInitialState();

		// make our entity
		setupEntity(sceneMgr, block);

		setupAnimation(block);

		// then attach it to the node!
		node->attachObject(entity);
	}
}

Entity* ModelComponent::getEntity()
{
	return entity;
}

unsigned ModelComponent::getId() const
{
	return id;
}

string ModelComponent::getName() const
{
	return name;
}

Ogre::SceneNode* ModelComponent::getNode()
{
	return node;
}

const LThing* const ModelComponent::getOwner() const
{
	return owner;
}

Quaternion ModelComponent::initSpawnOrientation(const ModelBlock* const block) const
{
	Quaternion q; Vector3 v;
	return (q=block->getQuatProperty("orientation", Quaternion::IDENTITY),
		q==Quaternion::IDENTITY?((v=block->getVectorProperty("rotation", Vector3::ZERO),
			v!=Vector3::ZERO?(degreeVectorToGlobalQuaternion(v)):q)):q);
}

void ModelComponent::setupAnimation(ModelBlock* block)
{
	if (block->getBoolProperty("animated", false))
	{
		int numAnims = 0;
		auto it = entity->getAllAnimationStates()->getAnimationStateIterator();
		for(auto i = it.begin(); i!=it.end(); ++i) ++numAnims;
		if (numAnims == 1)
		{
			animationState = entity->getAnimationState(block->getStringProperty("AnimationName"));
			animationState->setLoop(block->getBoolProperty("AnimationLooping", true));
			animationState->setEnabled(true);

			LKernel::getG<AnimationManager>()->add(animationState);
		}
		else if (numAnims > 1)
		{
			animationBlender = new AnimationBlender(entity);
			animationBlender->init(block->getStringProperty("AnimationName"), block->getBoolProperty("AnimationLooping", true));

			LKernel::getG<AnimationManager>()->add(animationBlender);
		}
	}
}

void ModelComponent::setupEntity(SceneManager* sceneMgr, ModelBlock* block)
{
	// make a new one if it isn't created yet, clone an existing one
	string meshName = block->getStringProperty("mesh");
	if (sceneMgr->hasEntity(meshName))
	{
		ostringstream ss; ss<<meshName<<id;
		entity = sceneMgr->getEntity(meshName)->clone(ss.str());
	}
	else
		entity = sceneMgr->createEntity(meshName, meshName);

	auto ft = block->getFloatTokens();
	if (ft.find("renderingdistance")!=ft.end())
		entity->setRenderingDistance(block->getFloatProperty("RenderingDistance"));

	// material name
	string materialName = block->getStringProperty("material", string());
	if (!materialName.empty() && materialName.find_first_not_of(' ') != string::npos)
		entity->setMaterialName(materialName);

	// some other properties
	ThingEnum shad = block->getEnumProperty("CastsShadows", ThingEnum::Some);
	if (Options::shadowDetail == ShadowDetailOption::Many)
		entity->setCastShadows(shad == ThingEnum::Many || shad == ThingEnum::Some);
	else if (Options::shadowDetail == ShadowDetailOption::Some)
		entity->setCastShadows(shad == ThingEnum::Some);
	else
		entity->setCastShadows(false);
}
