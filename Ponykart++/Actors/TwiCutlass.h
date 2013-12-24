#ifndef TWICUTLASS_H_INCLUDED
#define TWICUTLASS_H_INCLUDED

#include <ik_ISound.h>
#include "Actors/Kart.h"

namespace PonykartParsers
{
	class ThingBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ponykart
{
namespace Sound
{
	class SoundMain;
} // Sound

namespace Actors
{
	class TwiCutlass : public Kart
	{
	public:
		TwiCutlass(PonykartParsers::ThingBlock* block, PonykartParsers::ThingDefinition* def);
		// Getters

	private:
        const float topSpeedKmHour;
        Sound::SoundMain* soundMain;
        irrklang::ISound *idleSound, *fullSound;
        bool idleState;
	};
} // Actors
} // Ponykart

#endif // TWICUTLASS_H_INCLUDED
