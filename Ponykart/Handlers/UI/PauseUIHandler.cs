using Miyagi.UI;
using Ponykart.Core;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class hooks up to whenever the game is paused and displays the pause UI.
	/// At the moment it's just a big label that says "PAUSED" but hey it's a start.
	/// Later we can expand this to bring up a menu or something.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class PauseUIHandler {

		private GUI pauseGui;

		public PauseUIHandler() {
			pauseGui = LKernel.GetG<UIMain>().GetGUI("pause gui");

			// hook up to the pause event
			LKernel.GetG<Pauser>().PauseEvent += new PauseEvent(DoOnPause);
		}

		void DoOnPause(PausingState state) {
			if (state == PausingState.Pausing)
				pauseGui.Visible = true;
			else
				pauseGui.Visible = false;
		}
	}
}
