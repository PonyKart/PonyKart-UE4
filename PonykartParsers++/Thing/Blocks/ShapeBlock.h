#ifndef SHAPEBLOCK_H_INCLUDED
#define SHAPEBLOCK_H_INCLUDED

#include <Ogre.h>
#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a Shape { } block in a .thing file.
class ShapeBlock : public TokenHolder
{
public:
	ShapeBlock(ThingDefinition* Owner);
	void finish() override;
	static Ogre::Quaternion globalEulerToQuat(Ogre::Radian rotX, Ogre::Radian rotY, Ogre::Radian rotZ);
	// Getters
	const ThingDefinition* getOwner();
	const Ogre::Matrix4& getTransform();

protected:
	ThingDefinition* owner;
	Ogre::Matrix4 transform;
};

} // PonykartParsers

#endif // SHAPEBLOCK_H_INCLUDED
