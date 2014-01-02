using System.Threading;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Timer = System.Threading.Timer;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DerpyHandler : ILevelHandler {
		// we'll keep track of her here
		Derpy derpy;
		// we use a timer to show the flag anim for 3 seconds, then go back to normal
		Timer timer;

		public DerpyHandler() {
			// spawn derpy
			derpy = LKernel.GetG<Spawner>().Spawn<Derpy>("Derpy", Vector3.ZERO, (t, d) => new Derpy(t, d));
			derpy.ChangeAnimation("HoldStartLight1");
			if (!Options.GetBool("Twh")) {
				derpy.AttachToKart(new Vector3(-1f, 1f, 2f), LKernel.GetG<Players.PlayerManager>().MainPlayer.Kart);
			}

			// setup the timer
			timer = new Timer((x) => { derpy.ChangeAnimation("Forward1"); }, null, Timeout.Infinite, Timeout.Infinite);

			LapCounter.OnPlayerLap += OnPlayerLap;
			LapCounter.OnPlayerFinish += OnPlayerFinish;
			RaceCountdown.OnCountdown += OnCountdown;
		}

		void OnCountdown(RaceCountdownState state) {
			if (state == RaceCountdownState.OneSecondAfterGo)
				derpy.ChangeAnimation("Forward1");
		}

		void OnPlayerFinish(Kart kart) {
			derpy.ChangeAnimation("FlagWave1");
		}

		void OnPlayerLap(Kart kart, int newLapCount) {
			derpy.ChangeAnimation("FlagWave2");

			timer.Change(3000, Timeout.Infinite);
		}

		public void Detach() {
			LapCounter.OnPlayerLap -= OnPlayerLap;
			LapCounter.OnPlayerFinish -= OnPlayerFinish;
			RaceCountdown.OnCountdown -= OnCountdown;

			timer.Dispose();
			derpy.Dispose();
		}
	}
}
