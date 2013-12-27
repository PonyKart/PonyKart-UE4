#include <OgreVector3.h>
#include "Misc/irrklangExtensions.h"

using namespace irrklang;
using namespace Ogre;

namespace Ponykart
{
	vec3df toSoundVector(const Vector3& vec)
	{
		return vec3df(vec.x, vec.y, vec.z);
	}
} // Ponykart
