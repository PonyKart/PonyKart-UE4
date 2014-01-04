#ifndef THINGBLOCK_H_INCLUDED
#define THINGBLOCK_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include <string>
#include <Ogre.h>
#include "Muffin/TokenHolder.h"

//#include "MuffinDefinition.h"

namespace PonykartParsers
{
class MuffinDefinition;
// These represent each Thing in the .muffin files
class ThingBlock : public TokenHolder
{
public:
	DLLEXPORT ThingBlock(std::string ThingName, MuffinDefinition* Owner);
	DLLEXPORT ThingBlock(std::string ThingName, Ogre::Vector3 Position);
	DLLEXPORT ThingBlock(std::string ThingName, Ogre::Vector3 Position, Ogre::Quaternion Orientation);
	DLLEXPORT void finish() override;
	// Getters
	DLLEXPORT std::string getThingName() const;
	DLLEXPORT MuffinDefinition* getOwner();
	DLLEXPORT Ogre::Vector3 getPosition();

private: // Set-private public members
	std::string thingName; // The name of the .thing file this corresponds with
	MuffinDefinition* owner;
	Ogre::Vector3 position;
};
} // PonykartParsers

#endif // THINGBLOCK_H_INCLUDED
