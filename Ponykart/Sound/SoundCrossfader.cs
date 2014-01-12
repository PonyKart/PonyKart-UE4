using IrrKlang;
using Mogre;
using Ponykart.Core;

namespace Ponykart.Sound {
	public class SoundCrossfader {
		ISound soundToFadeOut;
		ISound soundToFadeIn;
		readonly float initialFadeOutVolume;
		readonly float targetFadeInVolume;

		float progress = 0;
		readonly float duration;

		/// <summary>
		/// Creates a crossfader that will crossfade two sounds over the specified duration.
		/// </summary>
		/// <param name="toFadeOut">The sound you want to fade out.</param>
		/// <param name="toFadeIn">The sound you want to fade in.</param>
		/// <param name="duration">How long you want the crossfade to take, in seconds.</param>
		/// <param name="toFadeInVolume">What volume you want the "fade in" sound to have when it is completed</param>
		public SoundCrossfader(ISound toFadeOut, ISound toFadeIn, float duration, float toFadeInVolume = 1f) {
			// NOTE: Sounds will not exist when we're using the null sound driver!
			//       Make sure that the sound is not null before touching it.

			this.duration = duration;
			this.soundToFadeIn = toFadeIn;
			this.soundToFadeOut = toFadeOut;
			this.initialFadeOutVolume = toFadeOut == null ? 0.0f : toFadeOut.Volume;
			this.targetFadeInVolume = toFadeIn == null ? 1.0f : toFadeInVolume;

			LKernel.GetG<Root>().FrameEnded += FrameEnded;
		}

		/// <summary>
		/// After every frame, adjust the volumes appropriately
		/// </summary>
		bool FrameEnded(FrameEvent evt) {
			if (Pauser.IsPaused)
				return true;

			progress += evt.timeSinceLastFrame;
			// if the progress is over the duration, or sounds don't exist, we've finished
			if (progress > duration || soundToFadeOut == null || soundToFadeIn == null) {
				Detach();
				return true;
			}

			// adjust volumes relatively
			float relProgress = progress / duration;
			soundToFadeOut.Volume = 1f - (relProgress * initialFadeOutVolume);
			soundToFadeIn.Volume = relProgress * targetFadeInVolume;

			return true;
		}

		/// <summary>
		/// Make sure the volumes are "finished", then detach from the frame event
		/// </summary>
		public void Detach() {
			if (soundToFadeOut != null)
				soundToFadeOut.Volume = 0f;
			if (soundToFadeIn != null)
				soundToFadeIn.Volume = targetFadeInVolume;

			LKernel.GetG<Root>().FrameEnded -= FrameEnded;

			soundToFadeOut = null;
			soundToFadeIn = null;
		}
	}
}
