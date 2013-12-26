#ifndef EXTENSIONS_H_INCLUDED
#define EXTENSIONS_H_INCLUDED

#include <Ogre.h>
#include <irrKlang.h>
#include <LinearMath/btVector3.h>
#include <LinearMath/btQuaternion.h>
#include <LinearMath/btTransform.h>

namespace Ponykart
{
	irrklang::vec3df toSoundVector(const Ogre::Vector3& vec);
	btQuaternion toBtQuaternion(const Ogre::Quaternion& quat);
	btTransform toBtTransform(const Ogre::Matrix4& mat);
	btVector3 toBtVector3(const Ogre::Vector3& vec);
	Ogre::Vector3 toOgreVector3(const btVector3& vec);
	Ogre::ColourValue toColourValue(const Ogre::Quaternion& quat);
	Ogre::Quaternion degreeVectorToGlobalQuaternion(const Ogre::Vector3& vec); // Creates a quaternion from a global degree vector.
	Ogre::Quaternion fromGlobalEulerDegrees(const Ogre::Vector3& degreeRotations); // Given three euler degree angles from global axes, we make a new quaternion from those angles and return it.&
} // Ponykart

#endif // EXTENSIONS_H_INCLUDED
