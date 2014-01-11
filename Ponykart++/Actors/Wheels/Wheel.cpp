#include <BulletDynamics/Vehicle/btRaycastVehicle.h>
#include "Actors/Kart.h"
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
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;
using namespace Ponykart::Physics;

// Static members
const float Wheel::MINIMUM_FRICTION=3.f;

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
