#ifndef WHEEL_H_INCLUDED
#define WHEEL_H_INCLUDED

#include <unordered_map>
#include <string>
#include <Ogre.h>
#include "Actors/Wheels/WheelID.h"
#include "Actors/Wheels/DriftState.h"

class btDiscreteDynamicsWorld;
class btRaycastVehicle;
struct btWheelInfo;
namespace Ponykart
{
namespace Actors
{

class Kart;

// TODO: Might want to make the constructor private and have WheelFactory as a friend
// TODO: might want to make this abstract and make two more classes for front and back wheels
class Wheel
{
public:
	/** @brief This should only be used by the WheelFactory
	@param owner Which kart is the wheel attached to?
	@param connectionPoint Where is the wheel attached?
	@param wheelID ID number of the wheel
	@param dict The properties and values from the .wheel file this wheel was built from
	@param meshName The filename of the mesh we should use for this wheel */
	Wheel(Kart* owner, const Ogre::Vector3& connectionPoint, WheelID wheelID, 
		const std::unordered_map<std::string, float>& dict, const std::string& meshName);
	~Wheel(); ///< clean up stuff
	void postSimulate(btDiscreteDynamicsWorld* world, const Ogre::FrameEvent& evt); ///< Update our node's position and orientation and accelerate/brake/turn if we aren't paused
	float calculateTurnAngleMultiplier(float currentSpeed); ///< Same as CalculateTurnMultipliers(), but only does the Angle part.
	float calculateTurnAngle(float turnAngleMultiplier); ///< Calculates the angle the wheel should try to be at (in radians). // Use this one if you already know what the turnAngleMultiplier is.
	// Getters
	const Ogre::SceneNode* const getNode() const;
	const Ogre::Entity* const getEntity() const;
	WheelID getId() const;
protected:
	void changeFriction(btWheelInfo* info, float currentSpeed); ///< Changes wheel friction depending on our current speed
	void accelerate(float currentSpeed); ///< Apply some torque to the engine.
	void brake(float currentSpeed); ///< Apply some brake torque.
	void calculateTurnMultipliers(const float currentSpeed, float& turnAngleMultiplier, float& turnSpeedMultiplier); ///< Calculates the turning multipliers, based on our current speed and drift state.
	float calculateSteerChange(float targetSteerAngle, float speedTurnSpeedMultiplier, float currentAngle, float timestep); ///< now we have to figure out how much we have to change by. smooth out the turning
	void turn(float timeSinceLastFrame, float currentSpeed); ///< Rotates our wheels.
private:
	float getMotorForceForDriftState(WheelID id, WheelDriftState driftState, float motorForce); ///< Depending on the wheel's ID and our drift state, this determines what its motor force should be, since the karts are rear-wheel drive
public:
	const float defaultRadius; ///< The radius of the wheel. 0.5 (lymph)
	const float defaultWidth; ///< The width of the wheel. 0.4 (demo)
	const float defaultSuspensionRestLength; ///< The length of the suspension when it's fully extended. 0.6 (demo). 0.3 (zg)
	const float defaultFrictionSlip; ///< The friction of the wheel. Higher numbers give more friction, 0 gives no friction. 1000 (demo)
	const int intWheelID; ///< Since we want the ID number of this wheel in int form so much, this is used to keep track of it without casting it every time.
	WheelDriftState driftState; ///< Keeps track of whether we're drifting or not, and if we are, which direction we're moving in.
	// we use these three things to control the wheels
	float accelerateMultiplier; ///< 1 for forwards, -1 for backwards, 0 for no torque
	float turnMultiplier; ///< 1 for left, -1 for right
	bool isBrakeOn; ///< true for brake on, false for brake off
	float idealSteerAngle; ///< (RADIANS) the angle the wheel should try to be at when they aren't turning
	float friction; ///< The current friction of the wheel
	Kart* kart;
	btRaycastVehicle* vehicle;
protected:
	Ogre::SceneNode* node;
	Ogre::Entity* entity;
	Ogre::Vector3 axlePoint; ///< The point on the kart where this wheel is connected.
	/// The stiffness of the suspension spring. Higher values can help make the suspension return to rest length more quickly,
	/// but can also make the suspension oscillate.
	const float defaultSpringStiffness; ///< 100 (zg)
	/// I have no idea what this does, since changing it doesn't seem to have any effect on the suspension whatsoever
	const float defaultSpringCompression; ///< 4.2 (zg)
	/// How much force the suspension spring can absorb before returning to rest length. Higher numbers can make the kart smoother,
	/// but too high and the kart starts to shake (around 100)
	const float defaultSpringDamping; ///< 20 (zg)
	/// How much the wheel resists rolling (around its "forward" axis). Lower numbers give more resistance.
	const float defaultRollInfluence; ///< 1 (zg)
	const float defaultBrakeForce; ///< How much force the wheel exerts when braking.
	const float defaultMotorForce; ///< How much force the wheel's motor exerts, if it's a back wheel.
	const float defaultMaxTurnAngle; ///< (RADIANS) The maximum amount the wheel can turn by, if it's a front wheel. 0.3 rads (demo)
	const Ogre::Vector3 wheelDirection = Ogre::Vector3::NEGATIVE_UNIT_Y; ///< Which way is "up"?
	const Ogre::Vector3 wheelAxle = Ogre::Vector3::NEGATIVE_UNIT_X; ///< Which axis does the wheel rotate around?
	const float defaultSlowSpeed; ///< any slower than this and you will have the fully multiplied turn angle
	const float defaultHighSpeed; ///< any faster than this and you will have the regular turn angle
	const float defaultSlowTurnAngleMultiplier; ///< how much should the wheel's turn angle increase by at slow speeds?
	const float defaultSlowTurnSpeedMultiplier; ///< how much should the wheel's rotation speed increase by at slow speeds?
	const float defaultDriftingTurnAngle; ///< how much should the wheel's turn anglebe when drifting?
	const float defaultDriftingTurnSpeed; ///< (RADIANS) how much to increment the wheel's angle by, each frame, when drifting
	const float defaultSteerIncrementTurn; ///< (RADIANS) how much to increment the wheel's angle by, each frame
	const float defaultSteerDecrementTurn; ///< (RADIANS) how much to decrement the wheel's angle by, each frame
private:
	WheelID id; ///< Lets us keep track of which wheel this is on the kart
	static const float MINIMUM_FRICTION;
	static const float STOP_DRIFT_STEER_CHANGE;
	static const float START_DRIFT_STEER_CHANGE;
};
} // Actors
} // Ponykart

#endif // WHEEL_H_INCLUDED
