using MOIS;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// Just a small handler to do stuff when we press Esc
	/// If the console is up, we hide the console. If the console isn't up, we quit.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class EscHandler {

		public EscHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Escape += OnEscPress;
		}

		/// <summary>
		/// Eventually we'll probably want something so if there's a panel open, pressing escape closes the topmost one,
		/// and when you've closed all of them, this should pause, and not quit. But oh well this is temporary really.
		/// </summary>
		void OnEscPress(KeyEvent eventArgs) {
			var lcm = LKernel.GetG<LuaConsoleManager>();

			if (lcm.IsVisible)
				lcm.Hide();
			else
				Launch.Quit = true;
		}
	}
}
