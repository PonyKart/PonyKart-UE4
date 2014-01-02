using Mogre;
using MOIS;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Stuff;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Core {
	public class FreeCamera : LCamera {
		protected Vector3 Offset;
		protected float moveMultiplier = DEFAULT_MOVE_MULTIPLIER;
		protected float turnMultiplier = DEFAULT_TURN_MULTIPLIER;
		private const float DEFAULT_MOVE_MULTIPLIER = 0.5f;
		private const float DEFAULT_TURN_MULTIPLIER = 0.125f;

		public FreeCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.1f;
			Camera.FarClipDistance = 700f;
			Camera.AutoAspectRatio = true;

			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "FreeNode");
			CameraNode.AttachObject(Camera);
			CameraNode.SetFixedYawAxis(true);

			var inputMain = LKernel.GetG<InputMain>();
			inputMain.OnKeyboardPress_Anything += OnKeyboardPress_Anything;
			inputMain.OnKeyboardRelease_Anything += OnKeyboardRelease_Anything;
			inputMain.OnMouseMove += OnMouseMove;
			inputMain.OnMousePress_Right += OnMousePress_Right;
		}

		void OnMousePress_Right(MouseEvent eventArg1, MouseButtonID eventArg2) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			Vector3 pos = new Vector3(), norm = new Vector3();
			if (new MogreRaycaster().RaycastFromPoint(CameraNode.Position, -CameraNode.GetLocalZAxis(), ref pos, ref norm)) {
				var kart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
				kart.Body.Activate();

				Matrix4 mat = new Matrix4();
				mat.MakeTransform(pos, Vector3.UNIT_SCALE, kart.ActualOrientation);
				kart.Body.WorldTransform = mat;
			}
		}

		/// <summary>
		/// yaw/pitch the camera around
		/// </summary>
		protected virtual void OnMouseMove(MouseEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			CameraNode.Yaw(new Degree(-eventArgs.state.X.rel * turnMultiplier), Node.TransformSpace.TS_WORLD);
			CameraNode.Pitch(new Degree(-eventArgs.state.Y.rel * turnMultiplier), Node.TransformSpace.TS_LOCAL);
		}

		/// <summary>
		/// undo anything that happens when we press anything
		/// </summary>
		void OnKeyboardRelease_Anything(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			switch (eventArgs.key) {
				case KeyCode.KC_UP:
					Offset.z += 1;
					break;
				case KeyCode.KC_DOWN:
					Offset.z -= 1;
					break;
				case KeyCode.KC_LEFT:
					Offset.x += 1;
					break;
				case KeyCode.KC_RIGHT:
					Offset.x -= 1;
					break;
				case KeyCode.KC_RSHIFT:
					Offset.y -= 1;
					break;
				case KeyCode.KC_RCONTROL:
					Offset.y += 1;
					break;
			}

			var keyboard = LKernel.GetG<InputMain>().InputKeyboard;
			if (!keyboard.IsKeyDown(KeyCode.KC_UP) && !keyboard.IsKeyDown(KeyCode.KC_DOWN)
				&& !keyboard.IsKeyDown(KeyCode.KC_LEFT) && !keyboard.IsKeyDown(KeyCode.KC_RIGHT)
				&& !keyboard.IsKeyDown(KeyCode.KC_RSHIFT) && !keyboard.IsKeyDown(KeyCode.KC_RCONTROL))
			{
				Offset = Vector3.ZERO;
			}
		}

		void OnKeyboardPress_Anything(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			switch (eventArgs.key) {
				case KeyCode.KC_UP:
					Offset.z -= 1;
					break;
				case KeyCode.KC_DOWN:
					Offset.z += 1;
					break;
				case KeyCode.KC_LEFT:
					Offset.x -= 1;
					break;
				case KeyCode.KC_RIGHT:
					Offset.x += 1;
					break;
				case KeyCode.KC_RSHIFT:
					Offset.y += 1;
					break;
				case KeyCode.KC_RCONTROL:
					Offset.y -= 1;
					break;
				case KeyCode.KC_PGUP:
					moveMultiplier *= 2;
					break;
				case KeyCode.KC_PGDOWN:
					moveMultiplier /= 2;
					break;
				case KeyCode.KC_HOME:
					turnMultiplier *= 1.5f;
					break;
				case KeyCode.KC_END:
					turnMultiplier /= 1.5f;
					break;
			}
		}

		/// <summary>
		/// make the camera jump to wherever the kart is
		/// </summary>
		public override void OnSwitchToActive(LCamera oldCamera) {
			base.OnSwitchToActive(oldCamera);

			var kart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			CameraNode.Position = kart.ActualPosition;
			CameraNode.Orientation = kart.ActualOrientation;

			CameraNode.Translate(new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset), Node.TransformSpace.TS_LOCAL);
			CameraNode.LookAt(kart.ActualPosition, Node.TransformSpace.TS_WORLD);
		}

		/// <summary>
		///  set the offset to zero so when we switch back to being active, we don't start zooming off
		/// </summary>
		public override void OnSwitchToInactive(LCamera newCamera) {
			base.OnSwitchToInactive(newCamera);

			Offset = Vector3.ZERO;
			moveMultiplier = DEFAULT_MOVE_MULTIPLIER;
			turnMultiplier = DEFAULT_TURN_MULTIPLIER;
		}

		protected override bool UpdateCamera(FrameEvent evt) {
			CameraNode.Translate(Offset * moveMultiplier, Node.TransformSpace.TS_LOCAL);
			return true;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				var inputMain = LKernel.GetG<InputMain>();
				inputMain.OnKeyboardPress_Anything -= OnKeyboardPress_Anything;
				inputMain.OnKeyboardRelease_Anything -= OnKeyboardRelease_Anything;
				inputMain.OnMouseMove -= OnMouseMove;
				inputMain.OnMousePress_Right -= OnMousePress_Right;
			}

			base.Dispose(disposing);
		}
	}
}
