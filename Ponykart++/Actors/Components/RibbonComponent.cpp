#include "pch.h"
#include <sstream>
#include "Actors/LThing.h"
#include "Actors/Components/RibbonComponent.h"
#include "Core/IDs.h"
#include "Core/Options.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"
#include "Misc/ogreExtensions.h"
#include "Thing/Blocks/RibbonBlock.h"
#include "Thing/Blocks/ThingBlock.h"

using namespace std;
using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;

RibbonComponent::RibbonComponent(LThing* lthing, PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::RibbonBlock* block)
{
	id = IDs::incremental();
	ostringstream ssId; ssId<<id;

	name = block->getStringProperty("name", thingTemplate->getThingName());

	// if ribbons are disabled, don't bother creating anything
	if (!Options::getBool("Ribbons"))
		return;

	ribbon = LKernel::gSceneManager->createRibbonTrail(name + ssId.str() + "Ribbon");

	// set up some properties
	ribbon->setMaterialName(block->getStringProperty("material", "ribbon"));
	ribbon->setTrailLength(block->getFloatProperty("length", 5));
	ribbon->setMaxChainElements((unsigned) block->getFloatProperty("elements", 10));
	ribbon->setInitialWidth(0, block->getFloatProperty("width", 1));
	ribbon->setInitialColour(0, toColourValue(block->getQuatProperty("colour", Quaternion(1, 1, 1, 1))));
	ribbon->setColourChange(0, toColourValue(block->getQuatProperty("colourchange", Quaternion(0, 0, 0, 3))));
	ribbon->setWidthChange(0, block->getFloatProperty("widthchange", 1));

	// attach it to the node
	ribbonNode = LKernel::gSceneManager->getRootSceneNode()->createChildSceneNode(name + ssId.str() + "RibbonNode");
	trackedRibbonNode = lthing->getRootNode()->createChildSceneNode(name + ssId.str() + "TrackedRibbonNode");
	ribbon->addNode(trackedRibbonNode);
	ribbonNode->attachObject(ribbon);

	trackedRibbonNode->setPosition(block->getVectorProperty("position"));
}

RibbonComponent::~RibbonComponent()
{
	auto sceneMgr = LKernel::gSceneManager;
	bool valid = LKernel::getG<LevelManager>()->getIsValidLevel();

	if (Options::getBool("Ribbons") && ribbon != nullptr && ribbonNode != nullptr)
	{
		//RibbonNode.DetachObject(Ribbon);
		//foreach (SceneNode n in Ribbon.GetNodeIterator())
		//	Ribbon.RemoveNode(n);
		if (valid)
		{
			sceneMgr->destroyRibbonTrail(ribbon);
			sceneMgr->destroySceneNode(ribbonNode);
			sceneMgr->destroySceneNode(trackedRibbonNode);
		}
		delete ribbon; ribbon=nullptr;
		delete ribbonNode; ribbonNode=nullptr;
	}
}

Ogre::RibbonTrail* RibbonComponent::getRibbon()
{
	return ribbon;
}
