#include <OgreMatrix4.h>
#include <OgreQuaternion.h>
#include <OgreVector3.h>
#include "Misc/bulletExtensions.h"

using namespace Ogre;

namespace Ponykart
{
	btQuaternion toBtQuaternion(const Quaternion& quat)
	{
		return btQuaternion(quat.x, quat.y, quat.z, quat.w);
	}

	btTransform toBtTransform(const Matrix4& mat)
	{
		return btTransform(toBtQuaternion(mat.extractQuaternion()), toBtVector3(mat.getTrans()));
	}

	btTransform toBtTransform(const Quaternion& rotScale, const Vector3& pos)
	{
        return btTransform(toBtQuaternion(rotScale), toBtVector3(pos));
	}

	btVector3 toBtVector3(const Vector3& vec)
	{
		return btVector3(vec.x,vec.y,vec.z);
	}
}
