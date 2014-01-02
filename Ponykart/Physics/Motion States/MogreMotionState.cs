using BulletSharp;
using Mogre;
using Ponykart.Actors;

namespace Ponykart.Physics {
	/// <summary>
	/// Handy thing that bullet has to keep graphics stuff synched up with physics stuff!
	/// Best thing is, it only updates the node when it's actually changed instead of every frame!
	/// </summary>
	public class MogreMotionState : MotionState {

		protected SceneNode node;
		protected Matrix4 transform;
		private LThing owner;

		/// <param name="thing">The connected lthing, used for updating sounds. You can pass null to skip updating sounds.</param>
		public MogreMotionState(LThing thing, Matrix4 transform, SceneNode node) {
			this.node = node;
			this.transform = transform;
			this.owner = thing;
		}

		/// <param name="thing">The connected lthing, used for updating sounds. You can pass null to skip updating sounds.</param>
		public MogreMotionState(LThing thing, Vector3 position, Quaternion orientation, SceneNode node) {
			transform = new Matrix4(orientation);
			transform.MakeTransform(position, Vector3.UNIT_SCALE, orientation);
			this.node = node;
			this.owner = thing;
		}

		/// <param name="thing">The connected lthing, used for updating sounds. You can pass null to skip updating sounds.</param>
		public MogreMotionState(LThing thing, SceneNode node)
			: this(thing, node.Position, node.Orientation, node)
		{ }


		public override Matrix4 WorldTransform {
			get {
				return transform;
			}
			set {
				if (node == null)
					base.Dispose();

				node.Orientation = value.ExtractQuaternion();
				node.Position = value.GetTrans();

				// update the sounds
				if (owner != null) {
					owner.SoundsNeedUpdate = true;
				}

				transform = value;
			}
		}
	}
}
