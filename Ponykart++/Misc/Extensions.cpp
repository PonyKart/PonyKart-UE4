#include "Misc/Extensions.h"

using namespace Ogre;
using namespace irrklang;
using namespace Ponykart;

namespace Ponykart
{

btQuaternion toBtQuaternion(const Ogre::Quaternion& quat)
{
	return btQuaternion(quat.x, quat.y, quat.z, quat.w);
}

btVector3 toBtVector3(const Vector3& vec)
{
	return btVector3(vec.x,vec.y,vec.z);
}

ColourValue toColourValue(const Quaternion& quat)
{
	return ColourValue(quat.x, quat.y, quat.z, quat.w);
}

vec3df toSoundVector(const Vector3& vec)
{
	return vec3df(vec.x, vec.y, vec.z);
}

Quaternion degreeVectorToGlobalQuaternion(const Vector3& vec)
{
    return fromGlobalEulerDegrees(vec);
}

} // Ponykart
