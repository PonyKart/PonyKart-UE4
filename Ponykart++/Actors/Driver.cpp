#include "Actors/Kart.h"
#include "Actors/Driver.h"

using namespace Ogre;
using namespace Ponykart::Actors;

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