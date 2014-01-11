#include <Ogre.h>
#include "Physics/PhysicsMain.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"

using namespace Ogre;
using namespace Ponykart::Physics;
using namespace Ponykart::LKernel;
using namespace Ponykart::Levels;

// Define static members
bool PhysicsMain::drawLines = false;
bool PhysicsMain::slowMo = false;
const int PhysicsMain::_maxSubsteps = 60; //45;
const float PhysicsMain::_fixedTimestep = 1.0f / 60.0f; //1.0/45.0;
PhysicsWorldEvent PhysicsMain::postCreateWorld;
PhysicsSimulateEvent PhysicsMain::preSimulate;
PhysicsSimulateEvent PhysicsMain::postSimulate;
PhysicsSimulateEvent PhysicsMain::finaliseBeforeSimulation;
/*

PhysicsMain::PhysicsMain()
{
	log("[Loading] Creating PhysicsMain...");
	LevelManager::OnLevelUnload.push_back(OnLevelUnload);

	// Create our frame listeners
	frameEnded = new FrameEndedListener;
}

*/

btDiscreteDynamicsWorld* PhysicsMain::getWorld()
{
    return world;
}

/*
void PhysicsMain::OnLevelUnload(LevelChangedEventArgs eventArgs)
{
	LKernel::GetG<Root>()->removeFrameListener(frameEnded);

	if (world)
	{
		for (int a = 0; a < world->collisionObjectArray.Count; a++)
		{
			auto obj = world->collisionObjectArray[a];
			if (obj)
			{
				world->removeCollisionObject(obj);
				delete obj;
			}
		}

		delete broadphase; broadphase=nullptr;
		delete solver; solver=nullptr;
		delete dcc; dcc=nullptr;
		delete dispatcher; dispatcher=nullptr;

		delete world; world=nullptr;
	}
}

bool FrameEndedListener::frameEnded(const Ogre::FrameEvent& evt)
{
	if (Pauser.IsPaused || world.IsDisposed)
		return true;

	// run the events that go just before we simulate
	if (PreSimulate != null)
		PreSimulate(world, evt);

	if (FinaliseBeforeSimulation != null)
		FinaliseBeforeSimulation(world, evt);

	world->stepSimulation(PhysicsMain::SlowMo ? evt.timeSinceLastFrame / 10.0 : evt.timeSinceLastFrame, _maxSubsteps, _fixedTimestep);

	// run the events that go just after we simulate
	if (PostSimulate != null)
		PostSimulate(world, evt);

	if (PhysicsMain::DrawLines)
		world.DebugDrawWorld();

	//foreach (var item in LKernel.GetG<LevelManager>().CurrentLevel.Things) {
	//	if (item.Value.Name.Contains("Apple") && !item.Value.Name.Contains("Tree")) {
	//		System.Console.WriteLine(item.Value.Body.IsActive);
	//	}
	//}

	return true;
}
*/
