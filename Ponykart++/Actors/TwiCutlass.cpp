#include <BulletDynamics/Vehicle/btRaycastVehicle.h>
#include "Kernel/LKernel.h"
#include "Actors/Components/SoundComponent.h"
#include "Sound/SoundCrossfader.h"
#include "Sound/SoundMain.h"
#include "TwiCutlass.h"

using namespace Ponykart::Actors;
using namespace Ponykart::LKernel;
using namespace Ponykart::Sound;

TwiCutlass::TwiCutlass(PonykartParsers::ThingBlock* block, PonykartParsers::ThingDefinition* def)
: Kart(block, def), topSpeedKmHour(defaultMaxSpeed * 3.6f) // convert from linear velocity to KPH
{
	// sounds
	soundMain = LKernel::getG<SoundMain>();

	idleSound = soundComponents[0]->getSound();
	fullSound = soundComponents[1]->getSound();
	
	LKernel::gRoot->addFrameListener(this);
}

bool TwiCutlass::frameStarted(const Ogre::FrameEvent& evt)
{
	// crop it to be between 0 and 1
	float relSpeed = _vehicle->getCurrentSpeedKmHour() / topSpeedKmHour;

	if (relSpeed < 0.5f && !idleState)
	{
		new SoundCrossfader(fullSound, idleSound, 1.65f, 2.0f);

		idleState = true;
	}
	if (relSpeed > 0.5f && idleState)
	{
		new SoundCrossfader(idleSound, fullSound, 1.45f, 2.0f);

		idleState = false;
	}
	return true;
}