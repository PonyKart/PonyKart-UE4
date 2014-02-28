#include <al.h>
#include <OgreRoot.h>
#include "Core/Pauser.h"
#include "Kernel/LKernel.h"
#include "Sound/SoundCrossfader.h"
#include "Misc/alExtensions.h"

using namespace Ponykart::Core;
using namespace Ponykart::Sound;
using namespace Extensions;

/**
/// @param toFadeOut The sound you want to fade out.
/// @param toFadeIn The sound you want to fade in.
/// @param duration How long you want the crossfade to take, in seconds.
/// @param toFadeInVolume What volume you want the "fade in" sound to have when it is completed
**/
SoundCrossfader::SoundCrossfader(ALSource toFadeOut, ALSource toFadeIn, float Duration, float toFadeInVolume)
: progress(0), duration(Duration), initialFadeOutVolume(toFadeOut == -1 ? 0.f : alGetSourceGain(toFadeOut)),
	targetFadeInVolume(toFadeIn == -1 ? 1.f : toFadeInVolume)
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

	// TODO: Revisit this once the use-case for this class is better understood.

	progress += evt.timeSinceLastFrame;
	// if the progress is over the duration, or sounds don't exist, we've finished
	if (progress > duration || soundToFadeOut == -1 || soundToFadeIn == -1)
	{
		detach();
		return true;
	}

	// adjust volumes relatively
	float relProgress = progress / duration;
	alSourcef(soundToFadeOut, AL_GAIN, 1.f - (relProgress * initialFadeOutVolume));
	alSourcef(soundToFadeIn, AL_GAIN, relProgress * targetFadeInVolume);

	return true;
}

void SoundCrossfader::detach()
{
	if (soundToFadeOut != -1)
	{
		alSourcef(soundToFadeOut, AL_GAIN, 0.f);
	}
	if (soundToFadeIn != -1)
	{
		alSourcef(soundToFadeIn, AL_GAIN, targetFadeInVolume);
	}

	LKernel::gRoot->removeFrameListener(this);

	soundToFadeOut = -1;
	soundToFadeIn = -1;
}
