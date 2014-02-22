#include <OgreFrameListener.h>
#include "Misc/alExtensions.h"

namespace Ponykart
{
namespace Sound
{
	class SoundCrossfader : public Ogre::FrameListener
	{
	public: 
		SoundCrossfader(Extensions::ALsource toFadeOut, Extensions::ALsource toFadeIn, float Duration, float toFadeInVolume = 1.f); ///< Creates a crossfader that will crossfade two sounds over the specified duration.
		bool frameEnded(const Ogre::FrameEvent& evt) override; ///< After every frame, adjust the volumes appropriately
		void detach(); ///< Make sure the volumes are "finished", then detach from the frame event

	public:
		Extensions::ALsource soundToFadeOut;
		Extensions::ALsource soundToFadeIn;
		const float initialFadeOutVolume;
		const float targetFadeInVolume;

		float progress = 0;
		const float duration;
	};
}
}
