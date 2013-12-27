#include "Misc/ogreExtensions.h"

using namespace Ogre;

namespace Ponykart
{
	ColourValue toColourValue(const Quaternion& quat)
	{
		return ColourValue(quat.x, quat.y, quat.z, quat.w);
	}

	Quaternion degreeVectorToGlobalQuaternion(const Vector3& vec)
	{
		return fromGlobalEulerDegrees(vec);
	}
}
