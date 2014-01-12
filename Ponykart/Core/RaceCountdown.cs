#define ENABLE_COUNTDOWN

using Mogre;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Networking;
using Ponykart.Items;

namespace Ponykart.Core {
	/// <summary>
	/// A delegate for each of the countdown states
	/// </summary>
	public delegate void RaceCountdownEvent(RaceCountdownState state);

	public enum RaceCountdownState {
		Three = 3,
		Two = 2,
		One = 1,
		Go = 0,
		OneSecondAfterGo = -1
	}

	public class RaceCountdown {
		float elapsed;
		bool three, two, one, go, oneSecondAfterGo;
		const float INITIAL_DELAY = 1;

		/// <summary>
		/// our events
		/// </summary>
		public static event RaceCountdownEvent OnCountdown;

		/// <summary>
		/// Hook up to the level un/load events
		/// </summary>
		public RaceCountdown() {
			LevelManager.OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
			LevelManager.OnLevelUnload += (ea) => Detach();
		}

        public void Start() {
            three = two = one = go = oneSecondAfterGo = false;
            elapsed = 0;

            foreach (var player in LKernel.Get<PlayerManager>().Players) {
                // first make sure all of the karts can't be controlled
#if ENABLE_COUNTDOWN
                player.IsControlEnabled = false;
#else
					player.IsControlEnabled = true;
#endif
            }

            LKernel.Get<Root>().FrameStarted += FrameStarted;
        }

		/// <summary>
		/// Reset the elapsed time, reset the bools, disable control of all of the players, and hook up to the frame started event.
		/// </summary>
		private void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			// only run this on race levels!
			if (eventArgs.NewLevel.Type == LevelType.Race && (!eventArgs.Request.IsMultiplayer || LKernel.Get<NetworkManager>().AllConnectionsReady)) {
                Start();
			}
		}

		/// <summary>
		/// Count down!
		/// </summary>
		private bool FrameStarted(FrameEvent evt) {
			if (!Pauser.IsPaused) {
				if (!three && elapsed >= INITIAL_DELAY) {
					Invoke(RaceCountdownState.Three);
					three = true;
					elapsed = INITIAL_DELAY;
				}
				else if (!two && elapsed >= INITIAL_DELAY + 1) {
					Invoke(RaceCountdownState.Two);
					two = true;
					elapsed = INITIAL_DELAY + 1;
				}
				else if (!one && elapsed >= INITIAL_DELAY + 2) {
					Invoke(RaceCountdownState.One);
					one = true;
					elapsed = INITIAL_DELAY + 2;
				}
				else if (!go && elapsed >= INITIAL_DELAY + 3) {
#if ENABLE_COUNTDOWN
					foreach (var player in LKernel.Get<PlayerManager>().Players) {
						// first make sure all of the karts can't be controlled
						player.IsControlEnabled = true;
					}
                    LKernel.GetG<ItemManager>().spawning = true;
#endif

					Invoke(RaceCountdownState.Go);
					go = true;
					elapsed = INITIAL_DELAY + 3;
				}
				else if (!oneSecondAfterGo && elapsed >= INITIAL_DELAY + 4) {
					Invoke(RaceCountdownState.OneSecondAfterGo);
					oneSecondAfterGo = true;

					// don't need to keep checking the time any more
					Detach();
				}

				elapsed += evt.timeSinceLastFrame;
			}
			return true;
		}

		/// <summary>
		/// helper method
		/// </summary>
		private void Invoke(RaceCountdownState state) {
			if (OnCountdown != null)
				OnCountdown(state);
		}

		/// <summary>
		/// Unhook from the frame started event
		/// </summary>
		private void Detach() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
		}
	}
}