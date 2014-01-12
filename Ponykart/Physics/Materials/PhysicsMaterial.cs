
namespace Ponykart.Physics {
	public class PhysicsMaterial {
		public const float DEFAULT_FRICTION = 0.3f;
		public const float DEFAULT_BOUNCINESS = 0.3f;
		public const float DEFAULT_ANGULAR_DAMPING = 0.3f;
		public const float DEFAULT_LINEAR_DAMPING = 0.3f;

		/// <summary>
		/// 0 is no friction, 1 is lots of friction, >1 is more friction
		/// </summary>
		public float Friction { get; set; }
		/// <summary>
		/// 0 is no bounce at all, 1 is no loss of kinetic energy, >1 and the object gains energy when it bounces
		/// </summary>
		public float Bounciness { get; set; }
		/// <summary>
		/// 0 is no damping, 1 is full damping, >1 is overdamping
		/// </summary>
		public float AngularDamping { get; set; }
		/// <summary>
		/// 0 is no damping, 1 is full damping, >1 is overdamping
		/// </summary>
		public float LinearDamping { get; set; }

		/// <summary>
		/// Constructor and yeah
		/// </summary>
		/// <param name="friction"></param>
		/// <param name="bounciness"></param>
		/// <param name="angularDamping"></param>
		/// <param name="linearDamping"></param>
		public PhysicsMaterial(float friction = DEFAULT_FRICTION, float bounciness = DEFAULT_BOUNCINESS,
			float angularDamping = DEFAULT_ANGULAR_DAMPING, float linearDamping = DEFAULT_LINEAR_DAMPING)
		{
			Friction = friction;
			Bounciness = bounciness;
			AngularDamping = angularDamping;
			LinearDamping = linearDamping;
		}
	}
}
