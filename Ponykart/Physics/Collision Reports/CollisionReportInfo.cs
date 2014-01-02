using BulletSharp;
using Mogre;

namespace Ponykart.Physics {
	/// <summary>
	/// class to pass around for CollisionReport events. The order of the two objects doesn't matter.
	/// </summary>
	public class CollisionReportInfo {
		/// <summary>
		/// Collision group of our first object
		/// </summary>
		public PonykartCollisionGroups FirstGroup { get; set; }
		/// <summary>
		/// Collision group of our second object
		/// </summary>
		public PonykartCollisionGroups SecondGroup { get; set; }
		/// <summary>
		/// First object
		/// </summary>
		public CollisionObject FirstObject { get; set; }
		/// <summary>
		/// Second object
		/// </summary>
		public CollisionObject SecondObject { get; set; }
		/// <summary>
		/// Where the two objects first collided. Note that if A started touching B in some other place while still touching B in the same place
		/// as before, we would not fire a new event for that. Why? Because things are gonna be sliding around a lot so that would make
		/// far too many events.
		/// </summary>
		/// <remarks>
		/// Null if this is a StoppedTouching event.
		/// </remarks>
		public Vector3? Position { get; set; }
		/// <summary>
		/// The normal of the second object at the position where the two objects first collided.
		/// </summary>
		/// <remarks>
		/// Null if this is a StoppedTouching event.
		/// </remarks>
		public Vector3? Normal { get; set; }
		/// <summary>
		/// Lets us see whether the two objects just started touching or whether they just stopped.
		/// </summary>
		public ObjectTouchingFlags Flags { get; set; }

		/// <summary>
		/// For lua
		/// </summary>
		public int IntFlags {
			get {
				return (int) Flags;
			}
		}
		/// <summary>
		/// For lua
		/// </summary>
		public int IntFirstGroup {
			get {
				return (int) FirstGroup;
			}
		}
		/// <summary>
		/// For lua
		/// </summary>
		public int IntSecondGroup {
			get {
				return (int) SecondGroup;
			}
		}
	}

	/// <summary>
	/// Some flags to let us know whether two objects started touching each other inappropriately or whether they finally finished
	/// </summary>
	public enum ObjectTouchingFlags {
		/// <summary>
		/// Two collision objects just started touching
		/// </summary>
		StartedTouching = 1,
		/// <summary>
		/// Two collision objects just stopped touching
		/// </summary>
		StoppedTouching = 2,
	}
}