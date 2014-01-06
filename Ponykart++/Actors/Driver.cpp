#include "Actors/Kart.h"
#include "Actors/Driver.h"

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