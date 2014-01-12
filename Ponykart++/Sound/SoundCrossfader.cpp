#include <ik_ISound.h>
#include <OgreRoot.h>
#include "Core/Pauser.h"
#include "Kernel/LKernel.h"
#include "Sound/SoundCrossfader.h"

using namespace irrklang;
using namespace Ponykart::Core;
using namespace Ponykart::Sound;

/**
/// @param toFadeOut The sound you want to fade out.
/// @param toFadeIn The sound you want to fade in.
/// @param duration How long you want the crossfade to take, in seconds.
/// @param toFadeInVolume What volume you want the "fade in" sound to have when it is completed
**/
SoundCrossfader::SoundCrossfader(ISound* toFadeOut, ISound* toFadeIn, float Duration, float toFadeInVolume)
: progress(0), duration(Duration), initialFadeOutVolume(toFadeOut == nullptr ? 0.f : toFadeOut->getVolume()),
	targetFadeInVolume(toFadeIn == nullptr ? 1.f : toFadeInVolume)
{
	// NOTE: Sounds will not exist when we're using the null sound driver!
	//       Make sure that the sound is not null before touching it.

	soundToFadeIn = toFadeIn;
	soundToFadeOut = toFadeOut;

	LKernel::gRoot->addFrameListener(this);
}

bool SoundCrossfader::frameEnded(const Ogre::FrameEvent& evt)
{
	if (Pauser::isPaused)
		return true;

	progress += evt.timeSinceLastFrame;
	// if the progress is over the duration, or sounds don't exist, we've finished
	if (progress > duration || soundToFadeOut == nullptr || soundToFadeIn == nullptr) 
	{
		detach();
		return true;
	}

	// adjust volumes relatively
	float relProgress = progress / duration;
	soundToFadeOut->setVolume(1.f - (relProgress * initialFadeOutVolume));
	soundToFadeIn->setVolume(relProgress * targetFadeInVolume);

	return true;
}

void SoundCrossfader::detach()
{
	if (soundToFadeOut != nullptr)
		soundToFadeOut->setVolume(0.f);
	if (soundToFadeIn != nullptr)
		soundToFadeIn->setVolume(targetFadeInVolume);

	LKernel::gRoot->removeFrameListener(this);

	soundToFadeOut = nullptr;
	soundToFadeIn = nullptr;
}