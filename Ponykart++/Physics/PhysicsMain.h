#ifndef PHYSICSMAIN_H_INCLUDED
#define PHYSICSMAIN_H_INCLUDED

#include <OgreFrameListener.h>
#include "Levels/LevelChangedEventArgs.h"

namespace Ponykart
{
namespace Physics
{
	class PhysicsMain
	{
	public:
		PhysicsMain();
		static void OnLevelUnload(Levels::LevelChangedEventArgs eventArgs); // Deletes the world

	private:
		// Runs just before every frame. Simulates one frame of physics.
		// Physics simulation should be the only thing that's using FrameEnded!
		class FrameEndedListener : public Ogre::FrameListener
		{
			bool frameEnded(const Ogre::FrameEvent& evt) override;
		};

	private:
		static constexpr int _maxSubsteps = 60; //45;
		static constexpr float _fixedTimestep = 1.0/60.0; //1f / 45f;
		static FrameEndedListener* frameEnded;
		// TODO: Implement the missing OgreBullet classes, or wait for an official implementation
		//BroadphaseInterface* broadphase;
		//DefaultCollisionConfiguration* dcc;
		//CollisionDispatcher* dispatcher;
		//SequentialImpulseConstraintSolver* solver;
		//DiscreteDynamicsWorld* world;

	public:
		static bool DrawLines; // Should we draw debug lines or not?
		static bool SlowMo;
	};
} // Physics
} // Ponykart


#endif // PHYSICSMAIN_H_INCLUDED
