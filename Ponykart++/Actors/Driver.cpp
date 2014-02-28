#include "pch.h"
#include "Actors/Driver.h"
#include "Actors/Kart.h"
#include "Actors/Components/ModelComponent.h"

using namespace std;
using namespace Ogre;
using namespace PonykartParsers;
using namespace Ponykart::Actors;

Driver::Driver(ThingBlock* block, ThingDefinition* def)
: LThing(block, def)
{
}

void Driver::attachToKart(Kart* newKart, Ogre::Vector3 offset)
{
	newKart->driver = this;
	kart = newKart;

	rootNode->getParent()->removeChild(rootNode);
	rootNode->addChild(rootNode);

	rootNode->setPosition(offset);
	rootNode->setOrientation(Quaternion::IDENTITY);
	rootNode->setInitialState();
}

void Driver::changeAnimation(const string& animationName) 
{
	changeAnimation(animationName, AnimationBlendingTransition::BlendSwitch);
}

void Driver::changeAnimation(const string& animationName, AnimationBlendingTransition transition, float duration)
{
	modelComponents[0]->animationBlender->blend(animationName, transition, duration, true);
}

void Driver::changeAnimation(DriverAnimation anim, AnimationBlendingTransition transition, float duration)
{
	changeAnimation(driverAnimationNames[anim], transition, duration);
}