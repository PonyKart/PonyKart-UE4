using Mogre;
using MOIS;
using Ponykart.Actors;
using Ponykart.Players;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race, "WhitetailWoods")]
	public class WTW_JumpAround : ILevelHandler {

		public WTW_JumpAround() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress;
		}

		void OnKeyboardPress(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			Kart kart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;

			Vector3 pos;
			Quaternion quat;

			switch (eventArgs.key) {
				case KeyCode.KC_NUMPAD0:
					pos = new Vector3(122.245f, 55f, 135.99f);
					quat = new Quaternion(0.3107f, 0.001f, 0.9505f, 0.0029f);
					break;
				default:
					return;
			}

			Matrix4 mat = new Matrix4();
			mat.MakeTransform(pos, Vector3.UNIT_SCALE, quat);

			kart.Body.WorldTransform = mat;
			kart.Body.Activate();
		}

		public void Detach() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything -= OnKeyboardPress;
		}
	}
}
