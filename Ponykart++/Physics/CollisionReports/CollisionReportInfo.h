#ifndef COLLISIONREPORTINFO_H_INCLUDED
#define COLLISIONREPORTINFO_H_INCLUDED
#include "Physics/CollisionGroups.h"

class btCollisionObject;
namespace Ogre { class Vector3; }

namespace Ponykart
{
namespace Physics
{
	/// Some flags to let us know whether two objects started touching each other inappropriately or whether they finally finished
	enum class ObjectTouchingFlags
	{
		/// Two collision objects just started touching
		StartedTouching = 1,
		/// Two collision objects just stopped touching
		StoppedTouching = 2
	};

	/// class to pass around for CollisionReport events. The order of the two objects doesn't matter.
	struct CollisionReportInfo
	{
		/// Collision group of our first object
		PonykartCollisionGroups firstGroup;
		/// Collision group of our second object
		PonykartCollisionGroups secondGroup;
		/// First object
		const btCollisionObject* firstObject;
		/// Second object
		const btCollisionObject* secondObject;
		/// Where the two objects first collided. Note that if A started touching B in some other place while still touching B in the same place
		/// as before, we would not fire a new event for that. Why? Because things are gonna be sliding around a lot so that would make
		/// far too many events.
		/// Null if this is a StoppedTouching event.
		Ogre::Vector3* position;
		/// The normal of the second object at the position where the two objects first collided.
		/// Null if this is a StoppedTouching event.
		Ogre::Vector3* normal;
		/// Lets us see whether the two objects just started touching or whether they just stopped.
		ObjectTouchingFlags flags;
	};
}
}
#endif // COLLISIONREPORTINFO_H_INCLUDED