using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// Instead of doing 8 raycasts every frame to self-right stuff, instead we only raycast every few frames and then only run
	/// this when we need to self-right, and then get rid of it afterwards
	/// </summary>
	public class SelfRighter {
		Kart kart;

		public SelfRighter(Kart kartToFlip) {
			kart = kartToFlip;

			PhysicsMain.PreSimulate += PreSimulate;
		}

		private static readonly Radian closeEnoughToUp = new Degree(2);

		/// <summary>
		/// We can't use constraints because otherwise we wouldn't be able to do loops.
		/// </summary>
		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			// if the kart's gone, then we can get rid of this handler too
			if (kart == null || kart.Vehicle.IsDisposed) {
				Detach();
				return;
			}
			// don't self-right if we're paused
			else if (Pauser.IsPaused)
				return;
			
			// so first we get the kart's orientation
			// then we basically get its local Y axis and average it with the global Y axis to make more of a smooth transition
			Vector3 locY = kart.ActualOrientation.YAxis;

			// first of all, if we're self righted enough, we can get rid of this handler
			if (locY.DirectionEquals(Vector3.UNIT_Y, closeEnoughToUp)) { // 3 degrees
				//Detach();
				return;
			}

			// stop it spinning
			kart.Body.AngularVelocity = Vector3.ZERO;

			// update its rotation to point upwards
			var quat = kart.ActualOrientation;
			// make the x and z factors smaller, so that all that's left at the end is the Y pointing upwards
			quat.x *= 0.92f;
			quat.z *= 0.92f;
			quat.Normalise();

			// then update the body's transform
			kart.Body.SetOrientation(quat);
		}

		public void Detach() {
			PhysicsMain.PreSimulate -= PreSimulate;
			kart = null;
		}
	}
}
