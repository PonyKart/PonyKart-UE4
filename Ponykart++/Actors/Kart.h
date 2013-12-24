#ifndef KART_H_INCLUDED
#define KART_H_INCLUDED

#include <Ogre.h>
#include <OgreFrameListener.h>
#include "Actors/LThing.h"
#include "Actors/Wheels/Wheel.h"

namespace PonykartParsers
{
	class ThingBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ponykart
{
namespace Players{ class Player; }
namespace Actors
{
class Driver;

//delegate void KartEvent(Kart kart); // TODO: Implement

// Base class for karts. -Z is forwards!
class Kart : public LThing
{
public:
	Kart(PonykartParsers::ThingBlock* block, PonykartParsers::ThingDefinition* def);
	// Getters
	float getMaxSpeed() const;
	float getMaxReverseSpeed() const;
	float getMaxSpeedSquared() const;
	float getMaxReverseSpeedSquared() const;
	const Wheel* const getWheelFL() const;
	const Wheel* const getWheelFR() const;
	const Wheel* const getWheelBL() const;
	const Wheel* const getWheelBR() const;
	const Ogre::SceneNode* const getLeftParticleNode() const;
	const Ogre::SceneNode* const getRightParticleNode() const;
	// Setters
	void setMaxSpeed(float speed);
	void setMaxReverseSpeed(float speed);
protected:
//	MotionState initializationMotionState() override; // TODO: Find MotionState definition and implement
public:
	int ownerID; // A special ID number just for the karts. 0 is usually the player kart, but don't rely on this.
	const float defaultMaxSpeed;
	const float frontDriftAngle; // (RADIANS) The angle of the "front" wheels during drifting
	const float backDriftAngle; // (RADIANS) The angle of the "back" wheels during drifting
	const Ogre::Radian driftTransitionAngle; // (RADIANS) The angle the kart "jumps" through when starting drifting
	bool isInAir; // Should only be set by KartHandler
	KartDriftState driftState;
	Driver* driver;
	Players::Player* player;
	// our wheelshapes
protected:
	Wheel* wheelFL;
	Wheel* wheelFR;
	Wheel* wheelBL;
	Wheel* wheelBR;
private:
	float maxSpeed;
	float maxSpeedSquared;
	float maxReverseSpeed;
	float maxReverseSpeedSquared;
	Ogre::SceneNode* leftParticleNode;
	Ogre::SceneNode* rightParticleNode;
};
} // Actors
} // Ponykart

#endif // KART_H_INCLUDED
