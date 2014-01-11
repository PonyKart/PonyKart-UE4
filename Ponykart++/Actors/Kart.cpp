#include "Actors/Kart.h"
#include "Misc/ogreExtensions.h"
#include "Physics/MotionStates/KartMotionState.h"
#include "Thing/ThingDefinition.h"
#include "Thing/Blocks/ThingBlock.h"

using namespace Ogre;
using namespace Extensions;
using namespace PonykartParsers;
using namespace Ponykart::Actors;
using namespace Ponykart::Physics;

Kart::Kart(ThingBlock* block, ThingDefinition* def) : LThing (block, def),
	defaultMaxSpeed(def->getFloatProperty("maxspeed", 180)),
	frontDriftAngle(Degree(def->getFloatProperty("FrontDriftAngle", 46)).valueRadians()),
	backDriftAngle(Degree(def->getFloatProperty("BackDriftAngle", 55)).valueRadians()),
	driftTransitionAngle(Degree(def->getFloatProperty("DriftTransitionAngle", 40)))
{
	maxSpeed = defaultMaxSpeed;
	maxReverseSpeed = def->getFloatProperty("maxreversespeed", 4);

	maxSpeedSquared = maxSpeed * maxSpeed;
	maxReverseSpeedSquared = maxReverseSpeed * maxReverseSpeed;

	isInAir = false;
}

float Kart::getMaxSpeed() const
{
	return maxSpeed;
}

void Kart::setMaxSpeed(float speed)
{
	maxSpeed = speed;
	maxSpeedSquared = speed*speed;
}

float Kart::getMaxReverseSpeed() const
{
	return maxReverseSpeed;
}

void Kart::setMaxReverseSpeed(float speed)
{
	maxReverseSpeed = speed;
	maxReverseSpeedSquared = speed*speed;
}

const btRaycastVehicle* const Kart::getVehicle() const
{
	return _vehicle;
}

btRaycastVehicle* Kart::getVehicle()
{
	return _vehicle;
}

const btRaycastVehicle::btVehicleTuning* const Kart::getTuning() const
{
	return tuning;
}

Ogre::Quaternion Kart::getActualOrientation() const
{
	return toOgreQuaternion(kartMotionState->actualOrientation);
}

float Kart::getVehicleSpeed() const
{
	return _vehicle->getCurrentSpeedKmHour();
}

bool Kart::isDriftingAtAll() const
{
	return KartDriftStateExtensions::isDriftingAtAll(driftState);
}
