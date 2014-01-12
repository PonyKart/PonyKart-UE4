#ifndef COLLISIONGROUPS_H_INCLUDED
#define COLLISIONGROUPS_H_INCLUDED

#include <BulletCollision/BroadphaseCollision/btBroadphaseProxy.h>

namespace Ponykart
{
namespace Physics
{
	/// Collision groups for bullet
	/** We can have a maximum of 15 different collision groups.
	// Remember to cast to btCollisionFilterGroups before using them. */
	enum class PonykartCollisionGroups
	{
		/// Everything!
		All = btBroadphaseProxy::CollisionFilterGroups::AllFilter, // -1
		// /// Collides with nothing. If you have one of these, why does it even have a physics object?
		//None = btBroadphaseProxy::CollisionFilterGroups::None, // 0
		/// Self-explanatory. Most of the stuff we can bump into and push around falls into this category.
		Default = btBroadphaseProxy::CollisionFilterGroups::DefaultFilter, // 1
		/// Static bodies that don't move, so scenery, fences, ramps, etc.
		Environment = btBroadphaseProxy::CollisionFilterGroups::StaticFilter, // 2
		/// For things that effect other stuff, such as speed boost panels
		Affectors = btBroadphaseProxy::CollisionFilterGroups::KinematicFilter, // 4
		/// For things we want to be able to drive on.
		Road = btBroadphaseProxy::CollisionFilterGroups::DebrisFilter, // 8
		/// Trigger regions and stuff
		Triggers = btBroadphaseProxy::CollisionFilterGroups::SensorTrigger, // 16
		/// Our karts!
		Karts = btBroadphaseProxy::CollisionFilterGroups::CharacterFilter, // 32
		/// For our invisible walls. We want to be able to collide with them, but not drive on them.
		InvisibleWalls = 64
	};

	enum class PonykartCollidesWithGroups
	{
		/// Collides with everything! Even things marked as "collides with nothing"! Use sparingly.
		All = -1,

		// /// Collides with nothing.
		//None = (int)PonykartCollisionGroups::None,

		/// Collides with other defaults, environment, kinematic, debris, karts, and invisible walls.
		Default = (int)PonykartCollisionGroups::Default
				| (int)PonykartCollisionGroups::Environment
				| (int)PonykartCollisionGroups::Affectors
				| (int)PonykartCollisionGroups::Road
				| (int)PonykartCollisionGroups::Karts
				| (int)PonykartCollisionGroups::InvisibleWalls,

		/// Collides with default, debris, and karts.
		Environment = (int)PonykartCollisionGroups::Default
					| (int)PonykartCollisionGroups::Karts,

		/// Collides with default and karts.
		Affectors = (int)PonykartCollisionGroups::Default
				  | (int)PonykartCollisionGroups::Karts,

		/// Collides with default and karts.
		Road = (int)PonykartCollisionGroups::Default
			 | (int)PonykartCollisionGroups::Karts,

		/// Only collides with karts.
		Triggers = (int)PonykartCollisionGroups::Karts,

		/// Collides with default, environment, kinematic, debris, triggers, other karts, and invisible walls.
		Karts = (int)PonykartCollisionGroups::Default
			  | (int)PonykartCollisionGroups::Environment
			  | (int)PonykartCollisionGroups::Affectors
			  | (int)PonykartCollisionGroups::Road
			  | (int)PonykartCollisionGroups::Triggers
			  | (int)PonykartCollisionGroups::Karts
			  | (int)PonykartCollisionGroups::InvisibleWalls,

		/// Collides with default and karts.
		InvisibleWalls = (int)PonykartCollisionGroups::Default
					   | (int)PonykartCollisionGroups::Karts
	};
} // Physics
} // Ponykart

#endif // COLLISIONGROUPS_H_INCLUDED
