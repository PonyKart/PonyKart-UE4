#ifndef THINGBLOCK_H_INCLUDED
#define THINGBLOCK_H_INCLUDED

#include <string>
#include <Ogre.h>
#include "TokenHolder.h"

//#include "MuffinDefinition.h"

namespace PonykartParsers
{
class MuffinDefinition;
// These represent each Thing in the .muffin files
class ThingBlock : public TokenHolder
{
public:
	ThingBlock(std::string ThingName, MuffinDefinition* Owner);
	ThingBlock(std::string ThingName, Ogre::Vector3 Position);
	ThingBlock(std::string ThingName, Ogre::Vector3 Position, Ogre::Quaternion Orientation);
	void finish();
	// Getters
	std::string getThingName() const;
	MuffinDefinition* getOwner();
	Ogre::Vector3 getPosition();

private: // Set-private public members
	std::string thingName; // The name of the .thing file this corresponds with
	MuffinDefinition* owner;
	Ogre::Vector3 position;
};
} // PonykartParsers

#endif // THINGBLOCK_H_INCLUDED
