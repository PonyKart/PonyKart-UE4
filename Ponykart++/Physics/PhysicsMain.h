#ifndef PHYSICSMAIN_H_INCLUDED
#define PHYSICSMAIN_H_INCLUDED

#include <OgreFrameListener.h>
#include "Levels/LevelChangedEventArgs.h"

class btDiscreteDynamicsWorld;

namespace Ponykart
{
namespace Physics
{
	class PhysicsMain
	{
	public:
		PhysicsMain();
		static void onLevelUnload(Levels::LevelChangedEventArgs eventArgs); // Deletes the world
		// Getters
		btDiscreteDynamicsWorld* getWorld();

	private:
		// Runs just before every frame. Simulates one frame of physics.
		// Physics simulation should be the only thing that's using FrameEnded!
		class FrameEndedListener : public Ogre::FrameListener
		{
			bool frameEnded(const Ogre::FrameEvent& evt) override;
		};

	private:
		static const int _maxSubsteps;
		static const float _fixedTimestep;
		static FrameEndedListener* frameEnded;
		// TODO: Implement the missing OgreBullet classes, or wait for an official implementation
		//BroadphaseInterface* broadphase;
		//DefaultCollisionConfiguration* dcc;
		//CollisionDispatcher* dispatcher;
		//SequentialImpulseConstraintSolver* solver;
		btDiscreteDynamicsWorld* world;

	public:
		static bool drawLines; // Should we draw debug lines or not?
		static bool slowMo;
	};
} // Physics
} // Ponykart


#endif // PHYSICSMAIN_H_INCLUDED
