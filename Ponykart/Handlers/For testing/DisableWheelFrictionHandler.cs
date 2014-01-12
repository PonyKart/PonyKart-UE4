using MOIS;
using Ponykart.Players;

namespace Ponykart.Handlers {
	//[Handler(HandlerScope.Level, LevelType.Race)]
	public class DisableWheelFrictionHandler : ILevelHandler {
		public DisableWheelFrictionHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += Press;
			LKernel.GetG<InputMain>().OnKeyboardRelease_Anything += Release;
		}

		void Release(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			if (eventArgs.key == KeyCode.KC_H) {
				LKernel.GetG<PlayerManager>().MainPlayer.Kart.ForEachWheel(w => w.Friction = w.DefaultFrictionSlip);
			}
		}

		void Press(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			if (eventArgs.key == KeyCode.KC_H)
				LKernel.GetG<PlayerManager>().MainPlayer.Kart.ForEachWheel(w => w.Friction = 0.8f);
		}

		public void Detach() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything -= Press;
			LKernel.GetG<InputMain>().OnKeyboardRelease_Anything -= Release;
		}
	}
}
