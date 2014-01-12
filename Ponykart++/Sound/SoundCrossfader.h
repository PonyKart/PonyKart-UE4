#include <OgreFrameListener.h>

namespace irrklang
{
	class ISound;
}

namespace Ponykart
{
namespace Sound
{
	class SoundCrossfader : public Ogre::FrameListener
	{
	public: 
		SoundCrossfader(irrklang::ISound* toFadeOut, irrklang::ISound* toFadeIn, float Duration, float toFadeInVolume = 1.f); ///< Creates a crossfader that will crossfade two sounds over the specified duration.
		bool frameEnded(const Ogre::FrameEvent& evt) override; ///< After every frame, adjust the volumes appropriately
		void detach(); ///< Make sure the volumes are "finished", then detach from the frame event

	public:
		irrklang::ISound* soundToFadeOut;
		irrklang::ISound* soundToFadeIn;
		const float initialFadeOutVolume;
		const float targetFadeInVolume;

		float progress = 0;
		const float duration;
	};
}
}