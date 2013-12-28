#ifndef BULLETEXTENSIONS_H_INCLUDED
#define BULLETEXTENSIONS_H_INCLUDED

#include <LinearMath/btVector3.h>
#include <LinearMath/btQuaternion.h>
#include <LinearMath/btTransform.h>

namespace Ogre
{
	class Matrix4;
	class Quaternion;
	class Vector3;
} // Ogre

namespace Extensions
{
	btQuaternion toBtQuaternion(const Ogre::Quaternion& quat);
	btTransform toBtTransform(const Ogre::Matrix4& mat);
	btTransform toBtTransform(const Ogre::Quaternion& rotScale, const Ogre::Vector3& pos);
	btVector3 toBtVector3(const Ogre::Vector3& vec);
} // Extensions

#endif // BULLETEXTENSIONS_H_INCLUDED
