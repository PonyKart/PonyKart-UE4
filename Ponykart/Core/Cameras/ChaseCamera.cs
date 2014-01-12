using Mogre;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Core {
	public class ChaseCamera : LCamera {
		private Kart FollowKart;

		public ChaseCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.1f;
			Camera.FarClipDistance = 700f;
			Camera.AutoAspectRatio = true;

			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "ChaseCamera");
			CameraNode.AttachObject(Camera);
			CameraNode.SetFixedYawAxis(true);

			FollowKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			Camera.SetAutoTracking(true, FollowKart.RootNode, new Vector3(0, 0.4f, 0));
		}

		public override void OnSwitchToActive(LCamera oldCamera) {
			base.OnSwitchToActive(oldCamera);

			Camera.Position = oldCamera.CameraNode._getDerivedPosition();
			Camera.Orientation = oldCamera.CameraNode._getDerivedOrientation();
		}
	}
}
