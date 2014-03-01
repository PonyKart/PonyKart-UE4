#include "pch.h"
#include "Actors/Wheels/WheelFactory.h"
#include "Core/Cameras/CameraManager.h"
#include "Core/Settings.h"
#include "Core/InputMain.h"
#include "Core/KeyBindingManager.h"
#include "Core/InputSwallowerManager.h"
#include "Core/Pauser.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Physics/PhysicsMain.h"
#include "Physics/CollisionShapeManager.h"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/Materials/PhysicsMaterialFactory.h"
#include "Physics/TriggerRegions/TriggerReporter.h"
#include "Sound/SoundMain.h"

using namespace Ogre;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Physics;
using namespace Ponykart::Sound;
using namespace LKernel::details;

void LKernel::loadInitialObjects(Splash& splash)
{
	splash.increment("Setting up level manager...");
	Levels::LevelManager* levelManager = addGlobalObject(new Levels::LevelManager());

	// Ogre
	splash.increment("Initialising resources and resource groups...");
	initOgreResources();
	loadOgreResourceGroups();

	// Bullet
	splash.increment("Initialising Bullet physics engine...");
	try
	{
		addGlobalObject(new PhysicsMain());
		addGlobalObject(new CollisionShapeManager());
		addGlobalObject(new CollisionReporter());
		addGlobalObject(new TriggerReporter());
		addGlobalObject(new PhysicsMaterialFactory());
	}
	catch (...)
	{
#ifdef _WIN32
		throw std::string("Bullet loading unsuccessful! Try installing the 2010 VC++ Redistributable (x86) - google it!");
#else
		throw std::string("Bullet loading unsuccessful!");
#endif
	}

	// level
	splash.increment("Loading first level physics...");
	//GetG<PhysicsMain>().LoadPhysicsLevel(Settings::MainMenuName); // TODO: Implement LoadPhysicsLevel
	addGlobalObject(new CameraManager());

	// OIS and input
	splash.increment("Starting input system...");
	addGlobalObject(new InputMain());
	addGlobalObject(new KeyBindingManager());
	addGlobalObject(new InputSwallowerManager());
	addGlobalObject(new Pauser());

	// spawner
	splash.increment("Creating spawners...");
	addGlobalObject(new WheelFactory());
	//addGlobalObject(new ThingDatabase());
	//addGlobalObject(new Spawner());
	//addGlobalObject(new AnimationManager());

	// Sound
	addGlobalObject(new SoundMain());

}
