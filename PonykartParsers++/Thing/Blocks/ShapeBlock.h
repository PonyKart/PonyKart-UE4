#ifndef SHAPEBLOCK_H_INCLUDED
#define SHAPEBLOCK_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifndef DLLEXPORT
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif
#endif

#include <Ogre.h>
#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a Shape { } block in a .thing file.
class ShapeBlock : public TokenHolder
{
public:
	DLLEXPORT ShapeBlock(ThingDefinition* Owner);
	DLLEXPORT void finish() override;
	DLLEXPORT static Ogre::Quaternion globalEulerToQuat(Ogre::Radian rotX, Ogre::Radian rotY, Ogre::Radian rotZ);
	// Getters
	DLLEXPORT const ThingDefinition* getOwner();
	DLLEXPORT const Ogre::Matrix4& getTransform();

protected:
	ThingDefinition* owner;
	Ogre::Matrix4 transform;
};

} // PonykartParsers

#endif // SHAPEBLOCK_H_INCLUDED
