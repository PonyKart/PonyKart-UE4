using Mogre;
using MOIS;
using Ponykart.Actors;
using Ponykart.Players;
using Vector3 = Mogre.Vector3;
using Ponykart.Physics;
using BulletSharp;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race, "SweetAppleAcres")]
	public class SAA_JumpAround : ILevelHandler {
		TriggerRegion tr;

		public SAA_JumpAround() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress;

			tr = new TriggerRegion("RiverTriggerRegion", new Vector3(-12.4589f, 2.30107f, -138.952f), new Quaternion(-0.00359472f, 0.000143133f, 0.999202f, 0.0397857f),
				new BoxShape(10.9985f, 2.71301f, 32.6104f));

			tr.OnTrigger += new TriggerReportEvent(tr_OnTrigger);
		}

		void tr_OnTrigger(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info) {
			var pos = new Vector3(-305.8f, 45.4037f, -693.169f) / 5f;
			var quat = new Quaternion(0.7143f, 0, -0.6998f, 0);

			quat = quat * new Quaternion(0, 0, 1, 0);

			Matrix4 mat = new Matrix4();
			mat.MakeTransform(pos, Vector3.UNIT_SCALE, quat);

			Kart kart = (otherBody.UserObject as CollisionObjectDataHolder).GetThingAsKart();

			if (kart != null) {
				kart.Body.WorldTransform = mat;
				kart.Body.Activate();
			}
		}

		void OnKeyboardPress(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			Kart kart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;

			Vector3 pos;
			Quaternion quat;

			switch (eventArgs.key) {
				case KeyCode.KC_NUMPAD0:
					pos = new Vector3(-50.8083f, -6.74291f, 324.711f) / 5f;
					quat = new Quaternion(0.7039f, 0, 0.7102f, 0);
					break;
				case KeyCode.KC_NUMPAD1:
					pos = new Vector3(-282.546f, 20.7933f, 327.82f) / 5f;
					quat = new Quaternion(0.7789f, 0, 0.627f, 0);
					break;
				case KeyCode.KC_NUMPAD2:
					pos = new Vector3(-354.992f, -3.4451f, -204.558f) / 5f;
					quat = new Quaternion(1, 0, 0, 0);
					break;
				case KeyCode.KC_NUMPAD3:
					pos = new Vector3(-305.8f, 45.4037f, -693.169f) / 5f;
					quat = new Quaternion(0.7143f, 0, -0.6998f, 0);
					break;
				case KeyCode.KC_NUMPAD4:
					pos = new Vector3(79.2f, 45.2845f, -696.161f) / 5f;
					quat = new Quaternion(0.70514f, 0, -0.709f, 0);
					break;
				case KeyCode.KC_NUMPAD5:
					pos = new Vector3(283.799f, 14.8f, -350.52f) / 5f;
					quat = new Quaternion(0, 0, 1, 0);
					break;
				case KeyCode.KC_NUMPAD6:
					pos = new Vector3(218.534f, 2.806f, -13.362f) / 5f;
					quat = new Quaternion(0.8834f, 0, 0.4686f, 0);
					break;
				case KeyCode.KC_NUMPAD7:
					pos = new Vector3(85.0738f, -2.5893f, 114.471f) / 5f;
					quat = new Quaternion(-0.0593f, 0, 0.9982f, 0);
					break;
				default:
					return;
			}

			// I copied all of the rotations wrong so we need to rotate them by 180 degrees around the Y axis
			quat = quat * new Quaternion(0, 0, 1, 0);

			Matrix4 mat = new Matrix4();
			mat.MakeTransform(pos, Vector3.UNIT_SCALE, quat);

			kart.Body.WorldTransform = mat;
			kart.Body.Activate();
		}

		public void Detach() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything -= OnKeyboardPress;

			tr.Dispose();
		}
	}
}
