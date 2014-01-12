using System;
using BulletSharp;

namespace Ponykart.Physics {
	/// <summary>
	/// Collision groups!
	/// We apparently have to use the built-in names for some reason. So that's what this class is for.
	/// Remember to cast to <see cref="BulletSharp.CollisionFilterGroups"/> before using them.
	/// 
	/// We can have a maximum of 15 different collision groups.
	/// </summary>
	/// <remarks>
	/// more info here:
	/// http://bulletphysics.org/mediawiki-1.5.8/index.php/Collision_Filtering
	/// </remarks>
	[Flags]
	public enum PonykartCollisionGroups {
		/// <summary>
		/// Everything!
		/// </summary>
		All = CollisionFilterGroups.AllFilter, // -1
		/// <summary>
		/// Collides with nothing. If you have one of these, why does it even have a physics object?
		/// </summary>
		None = CollisionFilterGroups.None, // 0
		/// <summary>
		/// Self-explanatory. Most of the stuff we can bump into and push around falls into this category.
		/// </summary>
		Default = CollisionFilterGroups.DefaultFilter, // 1
		/// <summary>
		/// Static bodies that don't move, so scenery, fences, ramps, etc.
		/// </summary>
		Environment = CollisionFilterGroups.StaticFilter, // 2
		/// <summary>
		/// For things that effect other stuff, such as speed boost panels
		/// </summary>
		Affectors = CollisionFilterGroups.KinematicFilter, // 4
		/// <summary>
		/// For things we want to be able to drive on.
		/// </summary>
		Road = CollisionFilterGroups.DebrisFilter, // 8
		/// <summary>
		/// Trigger regions and stuff
		/// </summary>
		Triggers = CollisionFilterGroups.SensorTrigger, // 16
		/// <summary>
		/// Our karts!
		/// </summary>
		Karts = CollisionFilterGroups.CharacterFilter, // 32
		/// <summary>
		/// For our invisible walls. We want to be able to collide with them, but not drive on them.
		/// </summary>
		InvisibleWalls = 64,
	}

	/// <summary>
	/// These say which groups collide with what. Remember to cast them to <seealso cref="BulletSharp.CollisionFilterGroups"/> before you use 'em.
	/// </summary>
	[Flags]
	public enum PonykartCollidesWithGroups {
		/// <summary>
		/// Collides with everything! Even things marked as "collides with nothing"! Use sparingly.
		/// </summary>
		All = -1, // -1 in binary is 111111111... so it's essentially the same as "everything"

		/// <summary>
		/// Collides with nothing.
		/// </summary>
		None = PonykartCollisionGroups.None,

		/// <summary>
		/// Collides with other defaults, environment, kinematic, debris, karts, and invisible walls.
		/// </summary>
		Default = PonykartCollisionGroups.Default
				| PonykartCollisionGroups.Environment
				| PonykartCollisionGroups.Affectors
				| PonykartCollisionGroups.Road
				| PonykartCollisionGroups.Karts
				| PonykartCollisionGroups.InvisibleWalls,

		/// <summary>
		/// Collides with default, debris, and karts.
		/// </summary>
		Environment = PonykartCollisionGroups.Default
					| PonykartCollisionGroups.Karts,

		/// <summary>
		/// Collides with default and karts.
		/// </summary>
		Affectors = PonykartCollisionGroups.Default
				  | PonykartCollisionGroups.Karts,

		/// <summary>
		/// Collides with default and karts.
		/// </summary>
		Road = PonykartCollisionGroups.Default
			 | PonykartCollisionGroups.Karts,

		/// <summary>
		/// Only collides with karts.
		/// </summary>
		Triggers = PonykartCollisionGroups.Karts,

		/// <summary>
		/// Collides with default, environment, kinematic, debris, triggers, other karts, and invisible walls.
		/// </summary>
		Karts = PonykartCollisionGroups.Default
			  | PonykartCollisionGroups.Environment
			  | PonykartCollisionGroups.Affectors
			  | PonykartCollisionGroups.Road
			  | PonykartCollisionGroups.Triggers
			  | PonykartCollisionGroups.Karts
			  | PonykartCollisionGroups.InvisibleWalls,

		/// <summary>
		/// Collides with default and karts.
		/// </summary>
		InvisibleWalls = PonykartCollisionGroups.Default
					   | PonykartCollisionGroups.Karts,
	}

	/// <summary>
	/// Silly C# won't let me add methods to enums
	/// </summary>
	static class CollisionExtensions {
		/// <summary>
		/// Converts this to a <see cref="BulletSharp.CollisionFilterGroups"/> so we can use it with bullet.
		/// </summary>
		public static CollisionFilterGroups ToBullet(this PonykartCollisionGroups pcg) {
			return (CollisionFilterGroups) pcg;
		}

		/// <summary>
		/// Is this bit flag set?
		/// </summary>
		/// <param name="other">The flag to test for</param>
		public static bool HasFlag(this PonykartCollisionGroups pcg, PonykartCollisionGroups other) {
			return (pcg & other) == other;
		}

		/// <summary>
		/// Converts this to a <see cref="BulletSharp.CollisionFilterGroups"/> so we can use it with bullet.
		/// </summary>
		public static CollisionFilterGroups ToBullet(this PonykartCollidesWithGroups pcwg) {
			return (CollisionFilterGroups) pcwg;
		}

		/// <summary>
		/// Is this bit flag set?
		/// </summary>
		/// <param name="other">The flag to test for</param>
		public static bool HasFlag(this PonykartCollidesWithGroups pcwg, PonykartCollidesWithGroups other) {
			return (pcwg & other) == other;
		}
	}
}
