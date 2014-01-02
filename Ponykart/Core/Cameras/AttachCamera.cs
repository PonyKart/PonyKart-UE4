using Mogre;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Core {
	public class AttachCamera : LCamera {
		private Kart FollowKart;

		public AttachCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.1f;
			Camera.FarClipDistance = 700f;
			Camera.AutoAspectRatio = true;

			FollowKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;

			CameraNode = FollowKart.RootNode.CreateChildSceneNode(name + "AttachCamera");
			CameraNode.AttachObject(Camera);
			CameraNode.SetFixedYawAxis(true);
		}

		public override void OnSwitchToActive(LCamera oldCamera) {
			base.OnSwitchToActive(oldCamera);

			Camera.Position = FollowKart.RootNode.ConvertWorldToLocalPosition(oldCamera.CameraNode._getDerivedPosition());
			Camera.Orientation = FollowKart.RootNode.ConvertWorldToLocalOrientation(oldCamera.CameraNode._getDerivedOrientation());
		}
	}
}
