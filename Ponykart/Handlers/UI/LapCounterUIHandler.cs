using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Core;
using Ponykart.Handlers;
using Ponykart.UI;
using Ponykart.Actors;
using Ponykart.Players;
using Ponykart.Levels;
using System;

namespace Ponykart.Handlers.UI {
    [Handler(HandlerScope.Global)]
    public class LapCounterUIHandler {

        private GUI lapCountGUI;
        private Label lapCountLabel;

        public LapCounterUIHandler() {
            lapCountGUI = LKernel.Get<UIMain>().GetGUI("lap count gui");
            lapCountLabel = lapCountGUI.GetControl<Label>("lap count label");

            LapCounter.OnLap += new LapCounterEvent(OnLap);
            LevelManager.OnLevelPostLoad += new LevelEvent(OnPostLoad);
        }

        void OnPostLoad(LevelChangedEventArgs lcea) {
            lapCountLabel.Text = "1/2";
            lapCountGUI.Visible = true;

        }
        void OnLap(Kart target, int lap) {
            if (target.Player == LKernel.Get<PlayerManager>().MainPlayer) {
                lapCountLabel.Text = String.Format("{0}/2", lap);
            }
        }

    }
}
