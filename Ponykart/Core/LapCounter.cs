using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Core {
	public delegate void LapCounterEvent(Kart kart, int newLapCount);
	public delegate void RaceFinishEvent(Kart kart);

	/// <summary>
	/// A class to detect when karts go over the finish line and counts their laps.
	/// </summary>
	public class LapCounter {
		/// <summary>
		/// Is fired when any kart complete a lap, but not when they finish.
		/// </summary>
		public static event LapCounterEvent OnLap;
		/// <summary>
		/// Is fired when the player completes a lap.
		/// </summary>
		public static event LapCounterEvent OnPlayerLap;
		/// <summary>
		/// Is fired when we complete the required number of laps.
		/// </summary>
		public static event RaceFinishEvent OnFinish;
		/// <summary>
		/// Is only fired once per race, when the first kart finishes.
		/// </summary>
		public static event RaceFinishEvent OnFirstFinish;
		/// <summary>
		/// Is only fired once per race, when the player finishes.
		/// </summary>
		public static event RaceFinishEvent OnPlayerFinish;
		bool anyoneFinishedYet;


		// boolean indicates whether we've passed the halfway point or not
		// int is the lap counter. We start on 0 because the karts spawn somewhere behind the start line
		Pair<bool, int>[] lapData;
		

		public LapCounter() {
			// we connect to PostLoad, because the trigger regions are created on regular Load and we don't want to try hooking up to trigger regions that don't exist yet!
			LevelManager.OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		

		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Name == "SweetAppleAcres") {

				// create lap data array
				lapData = new Pair<bool, int>[Settings.Default.NumberOfPlayers];
				// fill it with default data
				for (int a = 0; a < lapData.Length; a++) {
					lapData[a] = new Pair<bool, int>(true, 0);
				}

				anyoneFinishedYet = false;

				// hook up to the trigger regions
				var triggerReporter = LKernel.GetG<TriggerReporter>();
				triggerReporter.AddEvent("AITriggerRegion1", CrossFinishLine);
				triggerReporter.AddEvent("AITriggerRegion28", Halfway);

			}
		}

		void CrossFinishLine(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info) {
			// get the kart out of the object
			Kart kart = ((CollisionObjectDataHolder) otherBody.UserObject).GetThingAsKart();

			// make sure it's passed the halfway point first
			if (kart != null && lapData[kart.OwnerID].first) {
				lapData[kart.OwnerID].first = false;

				if (lapData[kart.OwnerID].second >= 2) {
					// we have completed three laps, fire the finish event
					if (OnFinish != null)
						OnFinish(kart);

					// if it's the player, fire its event
					if (kart == LKernel.GetG<PlayerManager>().MainPlayer.Kart) {
						if (OnPlayerFinish != null)
							OnPlayerFinish(kart);
					}

					// and if it's the first one to finish, fire that one too
					if (!anyoneFinishedYet) {
						anyoneFinishedYet = true;
						if (OnFirstFinish != null)
							OnFirstFinish(kart);
					}
				}
				else {
					// increment the counter
					lapData[kart.OwnerID].second++;

					// don't fire the event when we just cross the line when the race starts
					if (lapData[kart.OwnerID].second != 1) {
						// we have completed a lap, fire the lap event
						if (OnLap != null)
							OnLap(kart, lapData[kart.OwnerID].second);

						if (kart == LKernel.GetG<PlayerManager>().MainPlayer.Kart) {
							if (OnPlayerLap != null)
								OnPlayerLap(kart, lapData[kart.OwnerID].second);
						}
					}
				}
			}
		}

		/// <summary>
		/// Need to check somewhere else on the track otherwise we can just drive in circles over the finish line
		/// </summary>
		void Halfway(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info) {
			Kart kart = ((CollisionObjectDataHolder) otherBody.UserObject).GetThingAsKart();

			if (kart != null && !lapData[kart.OwnerID].first) {
				lapData[kart.OwnerID].first = true;
			}
		}

		/// <summary>
		/// Gets the lap count for the given kart.
		/// </summary>
		public int GetLapCount(Kart kart) {
			return lapData[kart.OwnerID].second;
		}
		/// <summary>
		/// Gets the lap count for the given player.
		/// </summary>
		public int GetLapCount(Player player) {
			return lapData[player.ID].second;
		}


		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			// unhook from to the trigger regions
			if (eventArgs.OldLevel.Name == "SweetAppleAcres") {
				var triggerReporter = LKernel.GetG<TriggerReporter>();
				triggerReporter.RemoveEvent("AITriggerRegion1", CrossFinishLine);
				triggerReporter.RemoveEvent("AITriggerRegion28", Halfway);
			}
		}
	}
}
