using IrrKlang;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Sound {
	[Handler(HandlerScope.Global)]
	public class BackgroundMusicHandler {
		private ISound bgMusic;

		public BackgroundMusicHandler() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
			RaceCountdown.OnCountdown += new RaceCountdownEvent(OnCountdown);
		}

		/// <summary>
		/// Runs whenever a new level is loaded. We create the background music now so it loads, then pause it until we need to play it.
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			// only want to load this on nonempty levels
			if (eventArgs.NewLevel.Type != LevelType.EmptyLevel) {
				// get the property from the .muffin file, if it has one
				string musicFile = eventArgs.NewLevel.Definition.GetStringProperty("Music", string.Empty);
				if (musicFile != string.Empty) {
					// if it's a race level, don't play the music until we need it
					bgMusic = LKernel.GetG<SoundMain>().PlayMusic(musicFile, eventArgs.NewLevel.Type == LevelType.Race);
				}
			}
		}

		/// <summary>
		/// Dispose all of the sound sources
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			bgMusic = null;
		}

		/// <summary>
		/// Start the background music, if it's a race level
		/// </summary>
		void OnCountdown(RaceCountdownState state) {
			if (state == RaceCountdownState.Go && bgMusic != null && LKernel.GetG<SoundMain>().IsMusicEnabled)
				bgMusic.Paused = false;
		}
	}
}
