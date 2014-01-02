using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.UI;
using Ponykart.Core;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Handlers.UI {
    [Handler(HandlerScope.Global)]
    public class RaceResultUIHandler {
        private Label winnerLabel;
        private GUI winnerGui;

        public RaceResultUIHandler() {
            winnerGui = LKernel.Get<UIMain>().GetGUI("winner gui");
            winnerLabel = winnerGui.GetControl<Label>("winner label");
            LapCounter.OnFirstFinish += new RaceFinishEvent(OnFirstFinish);
            winnerGui.Visible = false;
        }

        public void OnFirstFinish(Kart target) {
            if (target.Player == LKernel.Get<PlayerManager>().MainPlayer) {
                winnerLabel.Text = "You win!";
                winnerGui.Visible = true;
            } else {
                winnerLabel.Text = "You lose!";
                winnerGui.Visible = true;
            }
        }

    }
}
