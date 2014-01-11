#include <BulletDynamics/Vehicle/btRaycastVehicle.h>
#include "Actors/Kart.h"
#include "Actors/Wheels/DriftState.h"
#include "Actors/Wheels/Wheel.h"
#include "Core/Pauser.h"
#include "Kernel/LKernel.h"
#include "Misc/bulletExtensions.h"
#include "Misc/ogreExtensions.h"
#include "Physics/PhysicsMain.h"

using namespace std;
using namespace std::placeholders;
using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Actors;
using namespace Ponykart::Actors::KartDriftStateExtensions;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;
using namespace Ponykart::Physics;

// Static members
const float Wheel::MINIMUM_FRICTION=3.f;
const float Wheel::STOP_DRIFT_STEER_CHANGE = 0.0104719755f /*(0.6 degrees)*/ * 0.5f * 5.f /*(slow turn angle multiplier)*/;
const float Wheel::START_DRIFT_STEER_CHANGE = 0.0104719755f /*(0.6 degrees)*/ * 3.f * 5.f /*(slow turn angle multiplier)*/;

Wheel::Wheel(Kart* owner, const Vector3& connectionPoint, WheelID wheelID, 
	const unordered_map<string, float>& dict, const string& meshName)
	: defaultRadius(dict.at("Radius")), defaultWidth(dict.at("Width")),
	defaultSuspensionRestLength(dict.at("SuspensionRestLength")), defaultSpringStiffness(dict.at("SpringStiffness")),
	defaultSpringCompression(dict.at("SpringCompression")), defaultSpringDamping(dict.at("SpringDamping")),
	friction(dict.at("FrictionSlip")), defaultRollInfluence(dict.at("RollInfluence")),
	defaultBrakeForce(dict.at("BrakeForce")), defaultMotorForce(dict.at("MotorForce")),
	defaultMaxTurnAngle(Degree(dict.at("TurnAngle")).valueRadians()), defaultSlowSpeed(dict.at("SlowSpeed")),
	defaultHighSpeed(dict.at("HighSpeed")), defaultSlowTurnAngleMultiplier(dict.at("SlowTurnAngleMultiplier")),
	defaultSlowTurnSpeedMultiplier(dict.at("SlowTurnSpeedMultiplier")), 
	defaultDriftingTurnAngle(Degree(dict.at("DriftingTurnAngle")).valueRadians()),
	defaultDriftingTurnSpeed(Degree(dict.at("DriftingTurnSpeed")).valueRadians()),
	defaultSteerIncrementTurn(Degree(dict.at("SteerIncrementTurn")).valueRadians()),
	defaultSteerDecrementTurn(Degree(dict.at("SteerDecrementTurn")).valueRadians()),
	intWheelID((int)wheelID), defaultFrictionSlip(friction)
{
	// set up these
	kart = owner;
	id = wheelID;
	vehicle = kart->getVehicle();

	// give our fields some default values
	accelerateMultiplier = 0;
	turnMultiplier = 0;
	isBrakeOn = false;
	driftState = WheelDriftState::None;
	idealSteerAngle = 0.f;

	// need to tell bullet whether it's a front wheel or not
	bool isFrontWheel;
	if (id == WheelID::FrontLeft || id == WheelID::FrontRight)
		isFrontWheel = true;
	else
		isFrontWheel = false;

	vehicle->addWheel(toBtVector3(connectionPoint), toBtVector3(wheelDirection), toBtVector3(wheelAxle), 
		defaultSuspensionRestLength, defaultRadius, *kart->getTuning(), isFrontWheel);

	btWheelInfo info = vehicle->getWheelInfo(intWheelID);
	info.m_suspensionStiffness = defaultSpringStiffness;
	info.m_wheelsDampingRelaxation = defaultSpringDamping;
	info.m_wheelsDampingCompression = defaultSpringCompression;
	info.m_frictionSlip = friction;
	info.m_rollInfluence = defaultRollInfluence;

	axlePoint = connectionPoint + Vector3(0, -defaultSuspensionRestLength, 0);

	// create our node and entity
	node = owner->getRootNode()->createChildSceneNode("wheelNode" + kart->getID() + id, axlePoint);
	entity = LKernel::getG<SceneManager>()->createEntity("wheelNode" + kart->getID() + id, meshName);
	node->attachObject(entity);
	node->setInheritOrientation(false);

	node->setOrientation(kart->getActualOrientation());

	// and then hook up to the event
	PhysicsMain::postSimulate.push_back(bind(&Wheel::postSimulate,this,_1,_2));
}

void Wheel::postSimulate(btDiscreteDynamicsWorld* world, Ogre::FrameEvent* evt)
{
	if (!Pauser::isPaused) 
	{
		btWheelInfo info = kart->getVehicle()->getWheelInfo(intWheelID);
		float currentSpeed = vehicle->getCurrentSpeedKmHour();

		if (kart->getBody()->isActive() && (kart->getVehicleSpeed() > 1.f || kart->getVehicleSpeed() < -1.f)) 
		{
			// don't change the kart's orientation when we're drifting
			if (kart->isDriftingAtAll())
				node->setOrientation(kart->getActualOrientation());
			else
				node->setOrientation(toOgreQuaternion(info.m_worldTransform.getRotation()));
		}
		else 
		{
			// TODO: fix it so wheels aren't spinning when we're stopped, but they can still move left and right
			node->setOrientation(toOgreQuaternion(info.m_worldTransform.getRotation()));
		}

		// the wheel sorta "comes off" when it's moving quickly in the air, so we only need to update the z translation then
		if (!kart->isInAir) 
		{
			Vector3 trans = toOgreVector3(info.m_worldTransform.getOrigin());
			node->setPosition(axlePoint.x, kart->getRootNode()->convertWorldToLocalPosition(trans).y, axlePoint.z);
		}
		else
			node->setPosition(axlePoint);

		changeFriction(&info, currentSpeed);
		accelerate(currentSpeed);
		brake(currentSpeed);
		turn(evt->timeSinceLastFrame, currentSpeed);
	}
}

void Wheel::changeFriction(btWheelInfo* info, float currentSpeed)
{
	// really strong friction so we don't roll down hills when we're stopped
	if (accelerateMultiplier == 0 && currentSpeed > -2.f && currentSpeed < 2.f)
		info->m_frictionSlip = 10000.f;
	// if we're going slower than the slow speed, keep friction at maximum
	else if (currentSpeed < defaultSlowSpeed)
		info->m_frictionSlip = defaultFrictionSlip;
	// otherwise, change friction based on speed to a minimum
	else 
	{
		float newFric = friction * (1 - ((currentSpeed - defaultSlowSpeed) / ((kart->getMaxSpeed() * 3.6f) - defaultSlowSpeed)));
		info->m_frictionSlip = newFric > MINIMUM_FRICTION ? newFric : MINIMUM_FRICTION;
	}
}

void Wheel::accelerate(float currentSpeed)
{
	// if we're mostly stopped and we aren't trying to accelerate, then brake
	if (accelerateMultiplier == 0.f) 
	{
		if (currentSpeed > -2.f || currentSpeed < 2.f) 
		{
			vehicle->applyEngineForce(0.f, intWheelID);
			isBrakeOn = true;
		}
		else 
		{
			vehicle->applyEngineForce(0.f, intWheelID);
			isBrakeOn = true; //false;
		}
	}
	else 
	{
		// the wheels with motor force change depending on whether the kart is drifting or not
		// rear-wheel drive, remember!
		float _motorForce = getMotorForceForDriftState(id, driftState, defaultMotorForce);
		//Old accleration
		//vehicle.ApplyEngineForce(_motorForce * AccelerateMultiplier, IntWheelID);

		//Here is the new line that makes it accelerate but the max speed need to be increase inorder to make if over jumps.
		vehicle->applyEngineForce(_motorForce * accelerateMultiplier * 
			(((currentSpeed + kart->getMaxSpeed() *3.6f) / (kart->getMaxSpeed() * 3.6f)) / 2.f), intWheelID);

		// if we are trying to accelerate in the opposite direction that we're moving, then brake
		if ((accelerateMultiplier > 0.f && currentSpeed < -2.f) || (accelerateMultiplier < 0.f && currentSpeed > 2.f))
			isBrakeOn = true;
		// if we're either mostly stopped or going in the correct direction, take off the brake and accelerate
		else if ((accelerateMultiplier > 0.f && currentSpeed > -2.f) || (accelerateMultiplier < 0.f && currentSpeed < 2.f))
			isBrakeOn = false;
	}
}

void Wheel::brake(float currentSpeed)
{
	if (isBrakeOn) 
	{
		// handbrake
		if (accelerateMultiplier == 0.f && (currentSpeed > -2.f && currentSpeed < 2.f)) 
		{
			// the point of this is to lock the wheels in place so we don't move when we're stopped
			vehicle->setBrake(10000.f, intWheelID);
		}
		// normal brake
		else if ((accelerateMultiplier > 0.f && currentSpeed < -2.f) || (accelerateMultiplier < 0.f && currentSpeed > 2.f)) 
		{
			// brake to apply when we're changing direction
			vehicle->setBrake(defaultBrakeForce, intWheelID);
		}
		// normal brake
		else 
		{
			// brake to apply when we're just slowing down
			vehicle->setBrake(1.f/*BrakeForce * 0.25f*/, intWheelID);
		}
	}
	else 
	{
		vehicle->setBrake(0.f, intWheelID);
	}
}

void Wheel::turn(float timeSinceLastFrame, float currentSpeed)
{
	float speedTurnSpeedMultiplier;
	// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
	float speedTurnAngleMultiplier;

	calculateTurnMultipliers(currentSpeed, speedTurnAngleMultiplier, speedTurnSpeedMultiplier);

	// pick whether the wheel can turn or not depending on whether it's drifting
	// only "front" wheels turn!
	float targetSteerAngle = calculateTurnAngle(speedTurnAngleMultiplier);

	float currentAngle = vehicle->getSteeringValue(intWheelID);
	// now we have to figure out how much we have to change by
	// smooth out the turning
	float steerChange = calculateSteerChange(targetSteerAngle, speedTurnSpeedMultiplier, currentAngle, timeSinceLastFrame * 100.f);

	// turn the wheels! All of the logic here makes sure that we don't turn further than the wheel can turn,
	// and don't turn too far when we're straightening out
	if (currentAngle < targetSteerAngle) 
	{
		if (currentAngle + steerChange <= targetSteerAngle)
			vehicle->setSteeringValue(currentAngle + steerChange, intWheelID);
		// close enough to the ideal angle, so we snap
		else if (currentAngle + steerChange > targetSteerAngle)
			vehicle->setSteeringValue(targetSteerAngle, intWheelID);
	}
	else if (currentAngle > targetSteerAngle) 
	{
		if (currentAngle - steerChange >= targetSteerAngle)
			vehicle->setSteeringValue(currentAngle - steerChange, intWheelID);
		// can't decrement any more to the ideal angle, so we snap
		else if (currentAngle - steerChange < targetSteerAngle)
			vehicle->setSteeringValue(targetSteerAngle, intWheelID);
	}
}

void Wheel::calculateTurnMultipliers(const float currentSpeed, float& turnAngleMultiplier, float& turnSpeedMultiplier)
{
	if (driftState == WheelDriftState::None) 
	{
		// less than the slow speed = extra turn multiplier
		if (currentSpeed < defaultSlowSpeed) 
		{
			turnAngleMultiplier = defaultSlowTurnAngleMultiplier;
			turnSpeedMultiplier = defaultSlowTurnSpeedMultiplier;
		}
		// more than the high speed = no extra multiplier
		else if (currentSpeed > defaultHighSpeed) 
		{
			turnAngleMultiplier = 1.f;
			turnSpeedMultiplier = 1.f;
		}
		// somewhere in between = time for a cosine curve!
		else 
		{
			float relativeSpeed = currentSpeed - defaultSlowSpeed;
			float maxRelativeSpeed = defaultHighSpeed - defaultSlowSpeed;
			turnAngleMultiplier = 1.f + (Ogre::Math::Cos((relativeSpeed * Ogre::Math::PI) / (maxRelativeSpeed * 2.f)) * (defaultSlowTurnAngleMultiplier - 1.f));
			turnSpeedMultiplier = 1.f + (Ogre::Math::Cos((relativeSpeed * Ogre::Math::PI) / (maxRelativeSpeed * 2.f)) * (defaultSlowTurnSpeedMultiplier - 1.f));
		}
	}
	// no multiplier when we're drifting
	else 
	{
		turnAngleMultiplier = 1.f;
		turnSpeedMultiplier = 1.f;
	}
}

float Wheel::calculateSteerChange(float targetSteerAngle, float speedTurnSpeedMultiplier, float currentAngle, float timestep)
{
	if (driftState == WheelDriftState::None) 
	{
		if (isStopDrift(kart->driftState))
			return STOP_DRIFT_STEER_CHANGE * timestep;
		else if (Ogre::Math::Abs(targetSteerAngle - idealSteerAngle) < Ogre::Math::Abs(currentAngle - idealSteerAngle)) 
		{
			// we are not turning any more, so the wheels are moving back to their forward positions
			return defaultSteerDecrementTurn * speedTurnSpeedMultiplier * timestep;
		}
		else 
		{
			// we are turning, so the wheels are moving to their turned positions
			return defaultSteerIncrementTurn * speedTurnSpeedMultiplier * timestep;
		}
	}
	else 
	{
		if (isStartDrift(kart->driftState))
			return START_DRIFT_STEER_CHANGE * timestep;
		return defaultDriftingTurnSpeed * timestep;
	}
}

float Wheel::calculateTurnAngle(float turnAngleMultiplier)
{
	bool isFrontWheel = vehicle->getWheelInfo(intWheelID).m_bIsFrontWheel;
	if (driftState == WheelDriftState::None && (id == WheelID::FrontLeft || id == WheelID::FrontRight)) 
	{
		// front wheels, no drift
		// calculate what angle the wheels should try to be at
		return (defaultMaxTurnAngle * turnMultiplier * turnAngleMultiplier) + idealSteerAngle;
	}
	else if ((driftState == WheelDriftState::Left && (id == WheelID::FrontLeft || id == WheelID::BackLeft))
		|| (driftState == WheelDriftState::Right && (id == WheelID::FrontRight || id == WheelID::BackRight))) 
	{
		// "front" wheels, yes drift
		return (defaultDriftingTurnAngle * turnMultiplier) + idealSteerAngle;
	}
	else 
	{
		// back wheels
		return idealSteerAngle;
	}
}

float Wheel::getMotorForceForDriftState(WheelID id, WheelDriftState driftState, float motorForce)
{
	if (driftState == WheelDriftState::None) 
	{
		if (id == WheelID::BackLeft || id == WheelID::BackRight)
			return motorForce;
	}
	else if (driftState == WheelDriftState::Left) 
	{
		if (id == WheelID::FrontRight || id == WheelID::BackRight)
			return motorForce;
	}
	else if (driftState == WheelDriftState::Right) 
	{
		if (id == WheelID::FrontLeft || id == WheelID::BackLeft)
			return motorForce;
	}
	return 0.f;
}
