using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Core {
	/// <summary>
	/// A basic third-person camera with some smoothing.
	/// </summary>
	public class KnightyCamera : LCamera {
		SceneNode TargetNode;
		Kart followKart;
		SceneNode kartCamNode;
		SceneNode kartTargetNode;

		float rayLength;
		DiscreteDynamicsWorld world;

		public KnightyCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			// make our camera and set some properties
			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.1f;
			Camera.FarClipDistance = 700f;
			Camera.AutoAspectRatio = true;

			// create the nodes we're going to interpolate
			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "_KnightyCameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			TargetNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "_KnightyCameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

			CameraNode.SetAutoTracking(true, TargetNode);
			CameraNode.SetFixedYawAxis(true);

			CameraNode.AttachObject(Camera);

			// create the fixed nodes that are attached to the kart
			followKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			kartCamNode = followKart.RootNode.CreateChildSceneNode(name + "_KartKnightyCameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			kartTargetNode = followKart.RootNode.CreateChildSceneNode(name + "_KartKnightyCameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

			CameraNode.Position = kartCamNode._getDerivedPosition();
			TargetNode.Position = kartTargetNode._getDerivedPosition();

			// initialise some stuff for the ray casting
			rayLength = (CameraNode.Position - TargetNode.Position).Length;
			world = LKernel.GetG<PhysicsMain>().World;
		}

		private Vector3 mLastPosition = Vector3.ZERO;
		private Vector3 mLastTargetPosition = Vector3.ZERO;
		//private float mRoll = 0.0f;

		private readonly float _cameraTightness = Settings.Default.CameraTightness;
		/// <summary>
		/// Updates the camera
		/// TODO: stop it from going through the terrain
		/// </summary>
		protected override bool UpdateCamera( FrameEvent evt )
		{
			Vector3 camDisplacement, targetDisplacement,
				derivedCam = kartCamNode._getDerivedPosition( ),
				derivedTarget = kartTargetNode._getDerivedPosition( );

			if ( Math.Abs( derivedCam.y - ( derivedTarget.y + ( kartCamNode.Position.y - kartTargetNode.Position.y ) ) ) < 4 )
			{
				//derivedCam.y = derivedTarget.y + ( kartCamNode.Position.y - kartTargetNode.Position.y );
			}

			var callback = CastRay( derivedCam, derivedTarget );

			if ( callback.HasHit )
			{
				camDisplacement = callback.HitPointWorld - CameraNode.Position;


				Vector3 newTarget = derivedTarget;
				newTarget.y -= ( Settings.Default.CameraTargetYOffset * ( 1 - ( ( derivedTarget - callback.HitPointWorld ).Length / rayLength ) ) );

				targetDisplacement = ( newTarget - TargetNode.Position );
			}
			else
			{
				camDisplacement = derivedCam - CameraNode.Position;
				targetDisplacement = derivedTarget - TargetNode.Position;
			}

			// xi+1 = xi + (xi - xi-1) + a * dt * dt

			if ( mLastPosition == Vector3.ZERO )
			{
				mLastPosition = CameraNode._getDerivedPosition( );
				mLastTargetPosition = TargetNode._getDerivedPosition( );
			}

			{
				Vector3 last_pos = CameraNode.Position;
				CameraNode.Position = CameraNode.Position + ( CameraNode.Position - mLastPosition ) * (float)System.Math.Pow( 0.89f, evt.timeSinceLastFrame * 60.0f ) +camDisplacement * ( evt.timeSinceLastFrame * evt.timeSinceLastFrame ) * 60.0f;
				mLastPosition = last_pos;
			}

			{
				Vector3 last_pos = TargetNode.Position;
				TargetNode.Position = TargetNode.Position + ( TargetNode.Position - mLastTargetPosition ) * (float)System.Math.Pow( 0.8f, evt.timeSinceLastFrame * 60.0f ) + targetDisplacement * ( evt.timeSinceLastFrame * evt.timeSinceLastFrame ) * 100.0f;
				mLastTargetPosition = last_pos;
			}

			//Vector3 direction = camDisplacement * evt.timeSinceLastFrame * 0.75f;
			/*if ( direction.Length > 0.05f )
			{
				direction.Normalise( );
				direction *= 0.05f;
			}*/

			//mVelocity += direction;

			//CameraNode.Translate( mVelocity );
			//TargetNode.Translate( targetDisplacement * _cameraTightness * evt.timeSinceLastFrame );

			//mVelocity *= (float)System.Math.Pow( 0.97f, evt.timeSinceLastFrame * 200.0f );

			//this.Camera.FOVy += ( ( 45 + followKart.Body.LinearVelocity.Length * 0.25f ) * 3.14159f / 180.0f - this.Camera.FOVy ) * 0.1f;

			/*float desiredRoll = ( ( -followKart.Body.AngularVelocity.y ) * 0.1f + mRoll * 20.0f ) / 21.0f;
			float deltaRoll = desiredRoll - mRoll;
			Camera.Roll( deltaRoll );
			mRoll += deltaRoll;*/

			callback.Dispose( );
			return true;
		}

		readonly CollisionFilterGroups rayFilterGroup = (PonykartCollisionGroups.Environment | PonykartCollisionGroups.Road | PonykartCollisionGroups.InvisibleWalls).ToBullet();
		/// <summary>
		/// cast a ray from the target position to the camera position
		/// </summary>
		private DynamicsWorld.ClosestRayResultCallback CastRay(Vector3 derivedCam, Vector3 derivedTarget) {
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
