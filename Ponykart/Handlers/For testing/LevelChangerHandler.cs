using MOIS;
using Ponykart.Levels;
using Ponykart.Properties;

namespace Ponykart.Handlers {
	/// <summary>
	/// this is hooked up to the keyboard events and tells the level manager to change levels when certain keys are pressed
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LevelChangerHandler {

		public LevelChangerHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress_Anything;
		}

		void OnKeyboardPress_Anything(KeyEvent ke) {
			// if the input is swallowed, don't do anything
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			string s = string.Empty;
			switch (ke.key) {
				case KeyCode.KC_0:
					s = Settings.Default.MainMenuName; break;
				case KeyCode.KC_1:
					s = "flat"; break;
				case KeyCode.KC_2:
					s = "testlevel"; break;
				case KeyCode.KC_3:
					s = "SweetAppleAcres"; break;
				case KeyCode.KC_4:
					s = "WhitetailWoods"; break;
				case KeyCode.KC_5:
					s = "TestAI"; break;
			}
			if (!string.IsNullOrEmpty(s)) {
				LKernel.GetG<LevelManager>().LoadLevel(new LevelChangeRequest() {
					NewLevelName = s,
					CharacterNames = LKernel.GetG<Players.PlayerManager>().MakeCharacterString(),
				});
			}
		}
	}
}
