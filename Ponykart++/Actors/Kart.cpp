#include "Actors/Kart.h"
#include "Muffin/ThingBlock.h"
#include "Thing/ThingDefinition.h"

using namespace Ogre;
using namespace PonykartParsers;
using namespace Ponykart::Actors;

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
