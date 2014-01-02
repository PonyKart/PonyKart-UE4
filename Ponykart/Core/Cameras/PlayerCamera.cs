using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Core {
	/// <summary>
	/// A basic third-person camera with some smoothing.
	/// </summary>
	public class PlayerCamera : LCamera {
		SceneNode TargetNode;
		Kart followKart;
		SceneNode kartCamNode;
		SceneNode kartTargetNode;
        

		float rayLength;
		DiscreteDynamicsWorld world;

		public PlayerCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();
            
			// make our camera and set some properties
			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.1f;
			Camera.FarClipDistance = 300f;
			Camera.AutoAspectRatio = true;

			// create the nodes we're going to interpolate
			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "_PlayerCameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			TargetNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "_PlayerCameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

			CameraNode.SetAutoTracking(true, TargetNode);
			CameraNode.SetFixedYawAxis(true);
			CameraNode.AttachObject(Camera);

			// create the fixed nodes that are attached to the kart
			followKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			kartCamNode = followKart.RootNode.CreateChildSceneNode(name + "_KartCameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			kartTargetNode = followKart.RootNode.CreateChildSceneNode(name + "_KartCameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

			CameraNode.Position = kartCamNode._getDerivedPosition();
			TargetNode.Position = kartTargetNode._getDerivedPosition();

			// initialise some stuff for the ray casting
			rayLength = (CameraNode.Position - TargetNode.Position).Length;
			world = LKernel.GetG<PhysicsMain>().World;
		}

		protected readonly float _cameraTightness = Settings.Default.CameraTightness;
		protected readonly float _cameraTargetYOffset = Settings.Default.CameraTargetYOffset;
		/// <summary>
		/// Updates the camera
		/// </summary>
		protected override bool UpdateCamera(FrameEvent evt) {
			Vector3 camDisplacement, targetDisplacement,
				derivedCam = kartCamNode._getDerivedPosition(),
				derivedTarget = kartTargetNode._getDerivedPosition();

            Mogre.Vector3 axisA = new Mogre.Vector3(0, 1, 0);
            Quaternion quat1;
            quat1 = followKart.ActualOrientation.XAxis.GetRotationTo(axisA);
            Mogre.Radian rollMain = quat1.w;
            var kartRoll = Math.Sin((rollMain - new Radian(0.7f)) * 2);
            if ( 0.5f < kartRoll) { 
                kartRoll = 0.5f; 
            } else if (-0.5f > kartRoll) { 
                kartRoll = -0.5f; 
            }

            var camOr = Camera.Orientation;
            var Zdiff = kartRoll - Math.Sin(camOr.Roll + new Radian(Math.PI));
            if (Zdiff > 0 || Zdiff < 0)
            {
                Camera.Roll(-0.1f * Math.ASin(Zdiff).ValueRadians);
            }
			var callback = CastRay(derivedCam, derivedTarget);

			if (callback.HasHit) {
				camDisplacement = callback.HitPointWorld - CameraNode.Position;


				Vector3 newTarget = derivedTarget;
				newTarget.y -= (_cameraTargetYOffset * (1 - ((derivedTarget - callback.HitPointWorld).Length / rayLength)));

				targetDisplacement = (newTarget - TargetNode.Position);
			}
			else {
				camDisplacement = derivedCam - CameraNode.Position;
				targetDisplacement = derivedTarget - TargetNode.Position;
			}
			CameraNode.Translate(camDisplacement * _cameraTightness * evt.timeSinceLastFrame);
			TargetNode.Translate(targetDisplacement * _cameraTightness * evt.timeSinceLastFrame);

            CameraNode.Roll(10);
			callback.Dispose();
			return true;
		}

		readonly CollisionFilterGroups rayFilterGroup = (PonykartCollisionGroups.Environment | PonykartCollisionGroups.Road | PonykartCollisionGroups.InvisibleWalls).ToBullet();
		/// <summary>
		/// cast a ray from the target position to the camera position
		/// </summary>
		protected DynamicsWorld.ClosestRayResultCallback CastRay(Vector3 derivedCam, Vector3 derivedTarget) {
			Vector3 from = derivedTarget;
			Vector3 axis = (from - derivedCam);
			axis.Normalise();
			Vector3 to = from - (axis * rayLength); //CameraNode.Position;

			var callback = new DynamicsWorld.ClosestRayResultCallback(from, to);
			callback.CollisionFilterMask = rayFilterGroup;
			world.RayTest(from, to, callback);

			return callback;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				var sceneMgr = LKernel.GetG<SceneManager>();

				sceneMgr.DestroyCamera(Camera);
				sceneMgr.DestroySceneNode(CameraNode);
				sceneMgr.DestroySceneNode(TargetNode);
			}

			Camera.Dispose();
			CameraNode.Dispose();
			TargetNode.Dispose();

			base.Dispose(disposing);
		}
	}
}
