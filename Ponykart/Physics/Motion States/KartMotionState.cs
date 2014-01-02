using Mogre;
using Ponykart.Actors;

namespace Ponykart.Physics {
	public class KartMotionState : MogreMotionState {
		private Kart kart;

		/// <param name="thing">The connected lthing, used for updating sounds. You can pass null to skip updating sounds.</param>
		public KartMotionState(Kart kart, Vector3 position, Quaternion orientation, SceneNode node)
			: base(kart, position, orientation, node)
		{
			transform = new Matrix4(orientation);
			transform.MakeTransform(position, Vector3.UNIT_SCALE, orientation);
			this.node = node;
			this.kart = kart;

			lastPosition = position;
			lastOrientation = orientation;
		}

		public Vector3 lastPosition;
		public Quaternion lastOrientation;
		// how much "weight" the new transform has
		const float BIAS = 0.6f;
		// how much weight the old transform has
		const float INV_BIAS = 1 - BIAS;

		public Vector3 actualPosition;
		public Quaternion actualOrientation;

		public override Matrix4 WorldTransform {
			get {
				return transform;
			}
			set {
				if (node == null)
					base.Dispose();

				// interpolate the karts' movement to remove jittery-ness

				Vector3 newPos = value.GetTrans();
				Quaternion newOrient = value.ExtractQuaternion();

				// bias the newer orientations
				Vector3 avgPos = (lastPosition * INV_BIAS) + (newPos * BIAS);
				Quaternion avgOrient = Quaternion.Slerp(BIAS, lastOrientation, newOrient, true);

				node.Position = avgPos;
				node.Orientation = avgOrient;

				lastPosition = avgPos;
				lastOrientation = avgOrient;
				actualPosition = newPos;
				actualOrientation = newOrient;

				// update the sounds
				if (kart != null) {
					kart.SoundsNeedUpdate = true;
				}

				transform = value;
			}
		}
	}
}
