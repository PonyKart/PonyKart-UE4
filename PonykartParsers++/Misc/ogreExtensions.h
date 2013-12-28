#ifndef OGREEXTENSIONS_H_INCLUDED
#define OGREEXTENSIONS_H_INCLUDED

#include <OgreColourValue.h>
#include <OgreQuaternion.h>
#include <OgreVector3.h>

class btVector3;

namespace Extensions
{
	Ogre::Vector3 toOgreVector3(const btVector3& vec);
	Ogre::ColourValue toColourValue(const Ogre::Quaternion& quat);
	Ogre::Vector3 degreeVectorToRadianVector(const Ogre::Vector3& vec); ///< If you have a vector to be used for rotation but it's in degrees and you want radians, use this!
	Ogre::Vector3 radianVectorToDegreeVector(const Ogre::Vector3& vec); /// If you have a vector to be used for rotation but it's in radians and you want degrees, use this!
	Ogre::Quaternion degreeVectorToGlobalQuaternion(const Ogre::Vector3& vec); ///< Creates a quaternion from a global degree vector.
	Ogre::Quaternion fromGlobalEuler(Ogre::Radian rotX, Ogre::Radian rotY, Ogre::Radian rotZ);
	Ogre::Quaternion fromGlobalEuler(const Ogre::Vector3& radianRotations);
	Ogre::Quaternion fromGlobalEulerDegrees(const Ogre::Vector3& degreeRotations); ///< Given three euler degree angles from global axes, we make a new quaternion from those angles and return it.
	Ogre::Quaternion globalEulerToQuat(Ogre::Radian rotX, Ogre::Radian rotY, Ogre::Radian rotZ);
} // Extensions

#endif // OGREEXTENSIONS_H_INCLUDED
