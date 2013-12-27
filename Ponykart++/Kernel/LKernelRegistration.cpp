#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgreInit.h"
#include "Levels/LevelManager.h"
#include "Physics/PhysicsMain.h"
#include "Core/Cameras/CameraManager.h"
#include "Core/Settings.h"
#include "Core/InputMain.h"
#include "Core/KeyBindingManager.h"
#include "Core/InputSwallowerManager.h"
#include "Core/Pauser.h"
#include "Actors/Wheels/WheelFactory.h"

using namespace Ogre;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Physics;
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
		//addGlobalObject(new PhysicsMain());
		//addGlobalObject(new CollisionShapeManager());
		//addGlobalObject(new CollisionReporter());
		//addGlobalObject(new TriggerReporter());
		//addGlobalObject(new PhysicsMaterialFactory());
	}
	catch (...)
	{
		throw std::string("Bullet loading unsuccessful! Try installing the 2010 VC++ Redistributable (x86) - google it!");
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
	splash.increment("Creating spawner...");
	addGlobalObject(new WheelFactory());
	//addGlobalObject(new ThingDatabase());
	//addGlobalObject(new Spawner());
	//addGlobalObject(new AnimationManager());
}
