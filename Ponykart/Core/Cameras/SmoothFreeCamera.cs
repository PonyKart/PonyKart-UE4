using Mogre;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Core {
	public class SmoothFreeCamera : FreeCamera {
		protected SceneNode TargetNode;

		public SmoothFreeCamera(string name) : base(name) {
			TargetNode = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode();
			TargetNode.SetFixedYawAxis(true);

			CameraNode.DetachObject(Camera);
			TargetNode.AttachObject(Camera);
		}

		protected readonly float _cameraTightness = 3;

		protected override bool UpdateCamera(FrameEvent evt) {
			base.UpdateCamera(evt);

			Vector3 displacement = CameraNode._getDerivedPosition() - TargetNode.Position;

			TargetNode.Orientation = CameraNode.Orientation;
			TargetNode.Translate(displacement * _cameraTightness * evt.timeSinceLastFrame);

			return true;
		}

		public override void OnSwitchToActive(LCamera oldCamera) {
			base.OnSwitchToActive(oldCamera);

			TargetNode.Position = CameraNode.Position;
		}
	}
}
