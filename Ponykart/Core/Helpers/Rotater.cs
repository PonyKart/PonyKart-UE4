using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;

namespace Ponykart.Core {
	public delegate void RotaterEvent(Rotater rotater, LThing thing);

	/// <summary>
	/// A little class to help us rotate things over time.
	/// 
	/// The main difference between a nlerper and a rotater is that a nlerper "forces" the orientation during its change, and any other
	/// changes in the kart's orientation is ignored. This is ideal when you want to "lock" the kart against external forces.
	/// A rotater on the other hand keeps changing orientation while taking in other external rotations into account, essentially
	/// "adding" its own rotation on top of that every frame. This is best when the nlerper's locking effect is not desired.
	/// </summary>
	public class Rotater {
		LThing thing;
		float duration;
		float progress = 0, lastProgress = 0;
		Radian angle;
		Vector3 axis;
		RotaterAxisMode mode;
		Quaternion orient;

		public static event RotaterEvent Finished;

		/// <summary>
		/// USE THE LThingHelperManager FOR THIS!
		/// </summary>
		public Rotater(LThing thingToRotate, float duration, Radian angle, RotaterAxisMode mode)
			: this(thingToRotate, duration, angle, default(Vector3), mode)
		{ }

		/// <summary>
		/// USE THE LThingHelperManager FOR THIS!
		/// </summary>
		public Rotater(LThing thingToRotate, float duration, Radian angle, Vector3 axis, RotaterAxisMode mode = RotaterAxisMode.Explicit) {
			this.thing = thingToRotate;
			this.duration = duration;
			this.angle = angle;
			this.axis = axis;
			this.mode = mode;

			orient = new Quaternion();

			PhysicsMain.PreSimulate += PreSimulate;
		}

		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			// don't rotate if we're paused
			if (Pauser.IsPaused)
				return;

			progress += evt.timeSinceLastFrame;
			// if the thing's gone, we can get rid of this too
			if (lastProgress > duration || thing == null || thing.IsDisposed) {
				Detach();
				return;
			}

			// make the angle we should rotate by
			Radian angleThisFrame = (angle * ((progress - lastProgress) / duration));

			// and get our axis we're using
			Vector3 axisThisFrame;
			if (mode == RotaterAxisMode.Explicit)
				axisThisFrame = axis;
			else if (mode == RotaterAxisMode.RelativeX)
				axisThisFrame = thing.Body.Orientation.XAxis;
			else if (mode == RotaterAxisMode.RelativeY)
				axisThisFrame = thing.Body.Orientation.YAxis;
			else          // RotaterAxisMode.RelativeZ
				axisThisFrame = thing.Body.Orientation.ZAxis;

			// make a quaternion to use
			orient.FromAngleAxis(angleThisFrame, axisThisFrame);

			// and then rotate the body
			if (!thing.Body.IsActive)
				// if we rotate while we're deactivated, only the wheels will rotate and not our node
				thing.Body.Activate(true);
			thing.Body.SetOrientation(orient * thing.Body.Orientation);

			lastProgress = progress;
		}

		public LThing Owner {
			get {
				return thing;
			}
		}

		public void Detach() {
			if (thing != null) {
				if (Finished != null)
					Finished(this, thing);

				PhysicsMain.PreSimulate -= PreSimulate;
				thing = null;
			}
		}
	}

	public enum RotaterAxisMode {
		/// <summary>
		/// Rotate around an explicit axis
		/// </summary>
		Explicit,
		/// <summary>
		/// Rotate around the thing's local X axis
		/// </summary>
		RelativeX,
		/// <summary>
		/// Rotate around the thing's local Y axis
		/// </summary>
		RelativeY,
		/// <summary>
		/// Rotate around the thing's local Z axis
		/// </summary>
		RelativeZ,
	}
}
