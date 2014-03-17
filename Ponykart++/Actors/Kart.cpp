#include "pch.h"
#include "Actors/Kart.h"
#include "Misc/ogreExtensions.h"
#include "Physics/MotionStates/KartMotionState.h"
#include "Thing/ThingDefinition.h"
#include "Thing/Blocks/ThingBlock.h"

using namespace Ogre;
using namespace Extensions;
using namespace PonykartParsers;
using namespace Ponykart::Actors;
using namespace Ponykart::Actors::KartDriftStateExtensions;
using namespace Ponykart::Physics;

// Static members
KartEvent Kart::onStartDrifting, Kart::onDrifting, Kart::onStopDrifting, Kart::onFinishDrifting;

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

Ogre::Vector3 Kart::getActualPosition() const
{
	return toOgreVector3(kartMotionState->actualPosition);
}

void Kart::setTurnMultiplier(float multiplier)
{
	turnMultiplier = multiplier;

	wheelFL->turnMultiplier = multiplier;
	wheelFR->turnMultiplier = multiplier;
	wheelBL->turnMultiplier = multiplier;
	wheelBR->turnMultiplier = multiplier;
}

float Kart::getAcceleration() const
{
	return acceleration;
}

void Kart::setAcceleration(float newAcceleration)
{
	if (acceleration != 0.f)
		body->activate();
	acceleration = newAcceleration;

	wheelFL->accelerateMultiplier = newAcceleration;
	wheelFL->isBrakeOn = false;
	wheelFR->accelerateMultiplier = newAcceleration;
	wheelFR->isBrakeOn = false;
	wheelBL->accelerateMultiplier = newAcceleration;
	wheelBL->isBrakeOn = false;
	wheelBR->accelerateMultiplier = newAcceleration;
	wheelBR->isBrakeOn = false;
}

void Kart::stopDrifting()
{
	// "upgrade" our events
	if (driftState == KartDriftState::FullLeft || driftState == KartDriftState::StartLeft)
		driftState = KartDriftState::StopLeft;
	else if (driftState == KartDriftState::FullRight || driftState == KartDriftState::StartRight)
		driftState = KartDriftState::StopRight;

	// make the wheels back to normal
	stopDrifting_WheelFunction(wheelFL);
	stopDrifting_WheelFunction(wheelFR);
	stopDrifting_WheelFunction(wheelBL);
	stopDrifting_WheelFunction(wheelBR);

	// eeeeeveeeeeent
	if (onStopDrifting.size())
		for (auto& fun : onStopDrifting)
			fun(this);
}

void Kart::stopDrifting_WheelFunction(Wheel* w)
{
	w->driftState = WheelDriftState::None;
	w->idealSteerAngle = 0.f;

	if (w->getId() == WheelID::FrontRight || w->getId() == WheelID::FrontLeft)
		_vehicle->getWheelInfo(w->intWheelID).m_bIsFrontWheel = true;
	else
		_vehicle->getWheelInfo(w->intWheelID).m_bIsFrontWheel = false;
}

/// Start drifting in a certain direction
/// @param state This must be either StartDriftLeft or StartDriftRight
void Kart::startDrifting(KartDriftState state)
{
	// first check to make sure we weren't passed an incorrect argument
	if (!isStartDrift(state))
		throw std::string("You must pass either StartDriftLeft or StartDriftRight!");

	if (_vehicle->getCurrentSpeedKmHour() < 20.f || KartDriftStateExtensions::isDriftingAtAll(driftState))
		return;

	// update our state
	driftState = state;

	startDrifting_WheelFunction(wheelFL);
	startDrifting_WheelFunction(wheelFR);
	startDrifting_WheelFunction(wheelBL);
	startDrifting_WheelFunction(wheelBR);

	if (onStartDrifting.size())
		for (auto& fun : onStartDrifting)
			fun(this);
}

/// Put this in a separate function so we can edit it at runtime, since VS doesn't like us trying to edit anonymous functions
void Kart::startDrifting_WheelFunction(Wheel* w)
{
	// left
	if (driftState == KartDriftState::StartLeft) 
	{
		w->driftState = WheelDriftState::Left;

		// change the back wheels' angles
		if (w->getId() == WheelID::FrontRight || w->getId() == WheelID::BackRight) 
		{
			w->idealSteerAngle = backDriftAngle;
			_vehicle->getWheelInfo(w->intWheelID).m_bIsFrontWheel = false;
		}
		// change the front wheels' angles
		else 
		{
			w->idealSteerAngle = frontDriftAngle;
			_vehicle->getWheelInfo(w->intWheelID).m_bIsFrontWheel = true;
		}
	}
	// right
	else if (driftState == KartDriftState::StartRight) 
	{
		w->driftState = WheelDriftState::Right;

		// change the back wheels' angles
		if (w->getId() == WheelID::FrontLeft || w->getId() == WheelID::BackLeft) 
		{
			w->idealSteerAngle = -backDriftAngle;
			_vehicle->getWheelInfo(w->intWheelID).m_bIsFrontWheel = false;
		}
		// change the front wheels' angles
		else 
		{
			w->idealSteerAngle = -frontDriftAngle;
			_vehicle->getWheelInfo(w->intWheelID).m_bIsFrontWheel = true;
		}
	}
}
