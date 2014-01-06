#ifndef DRIVER_H_INCLUDED
#define DRIVER_H_INCLUDED

#include <string>
#include <Ogre.h>
#include "Actors/LThing.h"
#include "Core/Animation/AnimationBlender.h"
#include "Thing/ThingDefinition.h"
#include "Thing/Blocks/ThingBlock.h"

namespace Ponykart
{
namespace Players{ class Player; }
namespace Actors
{
	class Kart;

	/// Don't forget to update driverAnimationNames if you update DriverAnimation
	enum DriverAnimation
	{
		Drive,
		TurnLeft,
		TurnRight,
		Idle,
		Stand,
		Reverse,
		DriftLeft,
		DriftRight,
		Basis,
		_Unknown_
	};
	const std::string driverAnimationNames[] = {"Drive", "TurnLeft", "TurnRight", "Idle", "Stand", 
												"Reverse", "DriftLeft", "DriftRight", "Basis", "_Unknown_" };

    class Driver : LThing
    {
    public:
        Driver(PonykartParsers::ThingBlock* block, PonykartParsers::ThingDefinition* def);
        void attachToKart(Kart* newKart, Ogre::Vector3 offset);
        void changeAnimation(const std::string& animationName) override;
        void changeAnimation(const std::string& animationName, Ogre::AnimationBlendingTransition transition, float duration = 0.2f);
        void changeAnimation(DriverAnimation anim, Ogre::AnimationBlendingTransition transition = Ogre::AnimationBlendingTransition::BlendWhileAnimating, float duration = 0.2);
        void changeAnimationIfNotBlending(DriverAnimation anim, Ogre::AnimationBlendingTransition transition = Ogre::AnimationBlendingTransition::BlendWhileAnimating, float duration = 0.2); ///< Only runs successfully if it is currently not blending an animation.
    public:
		Kart* kart;
		Players::Player* player;
	};
} // Actors
} // Ponykart

#endif // DRIVER_H_INCLUDED
