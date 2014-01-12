using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;

namespace Ponykart.Core {
	public delegate void NlerperEvent(Nlerper nlerper, LThing thing);

	/// <summary>
	/// A little class to help us nlerp things.
	/// 
	/// The main difference between a nlerper and a rotater is that a nlerper "forces" the orientation during its change, and any other
	/// changes in the kart's orientation is ignored. This is ideal when you want to "lock" the kart against external forces.
	/// A rotater on the other hand keeps changing orientation while taking in other external rotations into account, essentially
	/// "adding" its own rotation on top of that every frame. This is best when the nlerper's locking effect is not desired.
	/// </summary>
	public class Nlerper {
		Quaternion orientSrc;
		Quaternion orientDest;
		float progress = 0;
		readonly float duration;
		LThing thing;

		/// <summary>
		/// This runs when the nlerper is finished doing its job. Use it for doing something that you need to do after it's done
		/// </summary>
		public static event NlerperEvent Finished;

		/// <summary>
		/// USE THE LThingHelperManager FOR THIS!
		/// 
		/// A nlerper is something that rotates a thing over a certain time to a certain orientation.
		/// Multiple nlerpers running at the same can cause problems, so you should check for other
		/// nlerpers and do something with them first before creating more.
		/// </summary>
		/// <param name="thingToNlerp">The thing we want to rotate</param>
		/// <param name="duration">The time the rotation should take, in seconds</param>
		/// <param name="orientDest">The destination orientation the kart should be when this is done</param>
		public Nlerper(LThing thingToNlerp, float duration, Quaternion orientDest) {
			this.duration = duration;
			this.orientSrc = thingToNlerp.Body.Orientation;
			this.orientDest = orientDest;
			this.thing = thingToNlerp;

			PhysicsMain.PreSimulate += PreSimulate;
		}

		/// <summary>
		/// nlerp!
		/// </summary>
		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (Pauser.IsPaused)
				return;

			progress += evt.timeSinceLastFrame;
			if (progress > duration || thing == null || thing.IsDisposed) {
				Detach();
				return;
			}

			Quaternion delta = Quaternion.Nlerp(progress / duration, orientSrc, orientDest, true);
			thing.Body.SetOrientation(delta);
		}

		public LThing Owner {
			get {
				return thing;
			}
		}

		/// <summary>
		/// Clean up
		/// </summary>
		public void Detach() {
			if (thing != null) {
				if (Finished != null)
					Finished(this, thing);

				PhysicsMain.PreSimulate -= PreSimulate;

				thing = null;
			}
		}
	}
}