#include <math.h>
#include "Misc/ogreExtensions.h"

using namespace Ogre;

namespace Extensions
{
	ColourValue toColourValue(const Quaternion& quat)
	{
		return ColourValue(quat.x, quat.y, quat.z, quat.w);
	}

	Vector3 degreeVectorToRadianVector(const Ogre::Vector3& vec)
	{
		const double twopi = 6.28318530717958647692; ///< 2*PI (double precision)
		return Vector3(vec.x*twopi/360.0, vec.y*twopi/360.0, vec.z*twopi/360.0);
	}

	Vector3 radianVectorToDegreeVector(const Ogre::Vector3& vec)
	{
		const double twopi = 6.28318530717958647692; ///< 2*PI (double precision)
		return Vector3(vec.x*360.0/twopi, vec.y*360.0/twopi, vec.z*360.0/twopi);
	}

	Quaternion degreeVectorToGlobalQuaternion(const Vector3& vec)
	{
		return fromGlobalEulerDegrees(vec);
	}

	/// Given three euler radian angles of global axes, we make a new quaternion from those angles and return it.
	/** Keep in mind that this doesn't modify the original quaternion.
		@param rotX Rotation (in radians) on the global X axis
		@param rotY Rotation (in radians) on the global Y axis
		@param rotZ Rotation (in radians) on the global Z axis
		@return A new quaternion */
	Ogre::Quaternion fromGlobalEuler(Ogre::Radian rotX, Ogre::Radian rotY, Ogre::Radian rotZ)
	{
		return globalEulerToQuat(rotX, rotY, rotZ);
	}

	/// Given three euler radian angles from global axes, we make a new quaternion from those angles and return it.
	/** Keep in mind that this doesn't modify the original quaternion.
		@param radianRotations A vector3 representing the rotations on global axes, so that rotations.x represents the rotation on the X axis, etc.\n
		This must be in radians! Use fromGlobalEulerDegrees if your vector is in degrees!
		@return A new quaternion */
	Ogre::Quaternion fromGlobalEuler(const Ogre::Vector3& radianRotations)
	{
		return globalEulerToQuat(Radian(radianRotations.x), Radian(radianRotations.y), Radian(radianRotations.z));
	}

	Quaternion fromGlobalEulerDegrees(const Ogre::Vector3& degreeRotations)
	{
		return fromGlobalEuler(degreeVectorToRadianVector(degreeRotations));
	}

	Quaternion globalEulerToQuat(Radian rotX, Radian rotY, Radian rotZ)
	{
		Quaternion q1, q2, q3; // global axes
		q1.FromAngleAxis(rotX, Vector3::UNIT_X);
		q2.FromAngleAxis(rotY, Vector3::UNIT_Y);
		q3.FromAngleAxis(rotZ, Vector3::UNIT_Z);

		return q3 * q2 * q1;
	}
} // Extensions
