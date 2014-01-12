#ifndef BULLETEXTENSIONS_H_INCLUDED
#define BULLETEXTENSIONS_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifndef DLLEXPORT
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif
#endif

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
	DLLEXPORT btQuaternion toBtQuaternion(const Ogre::Quaternion& quat);
	DLLEXPORT btTransform toBtTransform(const Ogre::Matrix4& mat);
	DLLEXPORT btTransform toBtTransform(const Ogre::Quaternion& rotScale, const Ogre::Vector3& pos);
	DLLEXPORT btVector3 toBtVector3(const Ogre::Vector3& vec);
} // Extensions

#endif // BULLETEXTENSIONS_H_INCLUDED
