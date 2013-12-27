#ifndef WHEEL_H_INCLUDED
#define WHEEL_H_INCLUDED

#include <unordered_map>
#include <string>
#include <Ogre.h>
#include "Actors/Wheels/WheelID.h"
#include "Actors/Wheels/DriftState.h"

namespace Ponykart
{
namespace Actors
{

class Kart;

// TODO: Implement members that make use if BulletSharp/Ogre classes
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
	Wheel(Kart* owner, Ogre::Vector3 connectionPoint, WheelID wheelID, std::unordered_map<std::string, float> dict, std::string meshName);
	~Wheel(); ///< clean up stuff
//	void postSimulate(DiscreteDynamicsWorld world, FrameEvent evt); ///< Update our node's position and orientation and accelerate/brake/turn if we aren't paused
	float CalculateTurnAngleMultiplier(float currentSpeed); ///< Same as CalculateTurnMultipliers(), but only does the Angle part.
	float CalculateTurnAngle(float turnAngleMultiplier); ///< Calculates the angle the wheel should try to be at (in radians). // Use this one if you already know what the turnAngleMultiplier is.
	// Getters
	const Ogre::SceneNode& getNode();
	const Ogre::Entity& getEntity();
protected:
	//void ChangeFriction(WheelInfo info, float currentSpeed); ///< Changes wheel friction depending on our current speed
	void Accelerate(float currentSpeed); ///< Apply some torque to the engine.
	void Brake(float currentSpeed); ///< Apply some brake torque.
	void CalculateTurnMultipliers(const float currentSpeed, float& turnAngleMultiplier, float& turnSpeedMultiplier); ///< Calculates the turning multipliers, based on our current speed and drift state.
	float CalculateSteerChange(float targetSteerAngle, float speedTurnSpeedMultiplier, float currentAngle, float timestep); ///< now we have to figure out how much we have to change by. smooth out the turning
	void Turn(float timeSinceLastFrame, float currentSpeed); ///< Rotates our wheels.
private:
	float GetMotorForceForDriftState(WheelID id, WheelDriftState driftState, float motorForce); ///< Depending on the wheel's ID and our drift state, this determines what its motor force should be, since the karts are rear-wheel drive
protected:
	Ogre::SceneNode node;
	Ogre::Entity entity;
};
} // Actors
} // Ponykart

#endif // WHEEL_H_INCLUDED
