#ifndef DASHJAVELIN_H_INCLUDED
#define DASHJAVELIN_H_INCLUDED

#include <Ogre.h>
#include "Misc/alExtensions.h"
#include "Actors/Kart.h"


namespace PonykartParsers
{
	class ThingBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ponykart
{
namespace Sound{class SoundMain;}
namespace Actors
{

class DashJavelin : public Kart
{
public:
	DashJavelin(PonykartParsers::ThingBlock* block, PonykartParsers::ThingDefinition* def);
	// Getters
	float getTopSpeedKmHour() const;

private:
	class FrameStartedListener : public Ogre::FrameListener
	{
		bool frameStarted(const Ogre::FrameEvent& evt) override;
	};
	FrameStartedListener* frameStarted;

private:
	Ogre::AnimationState *jetMax;
	Ogre::AnimationState *jetMin;
	float topSpeedKmHour;
	float jetOpening;

	Ogre::RibbonTrail* jetRibbon;

	Sound::SoundMain* soundMain;
	Extensions::ALSource idleSound, fullSound;
	Extensions::ALBuffer revDownSoundData, revUpSoundData;
	bool idleState; ///< true if we're in the "play the slower sound" state, false if we're in the "play the faster sound" state
};

} // Actors
} // Ponykart

#endif // DASHJAVELIN_H_INCLUDED
