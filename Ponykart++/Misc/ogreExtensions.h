#ifndef OGREEXTENSIONS_H_INCLUDED
#define OGREEXTENSIONS_H_INCLUDED

#include <OgreColourValue.h>
#include <OgreQuaternion.h>
#include <OgreVector3.h>

class btVector3;

namespace Ponykart
{
	Ogre::Vector3 toOgreVector3(const btVector3& vec);
	Ogre::ColourValue toColourValue(const Ogre::Quaternion& quat);
	Ogre::Quaternion degreeVectorToGlobalQuaternion(const Ogre::Vector3& vec); ///< Creates a quaternion from a global degree vector.
	Ogre::Quaternion fromGlobalEulerDegrees(const Ogre::Vector3& degreeRotations); ///< Given three euler degree angles from global axes, we make a new quaternion from those angles and return it.&
}

#endif // OGREEXTENSIONS_H_INCLUDED
