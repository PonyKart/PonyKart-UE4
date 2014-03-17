#include "pch.h"
#include "Actors/Wheels/WheelFactory.h"
#include "Core/Animation/AnimationManager.h"
#include "Core/Cameras/CameraManager.h"
#include "Core/Settings.h"
#include "Core/InputMain.h"
#include "Core/KeyBindingManager.h"
#include "Core/InputSwallowerManager.h"
#include "Core/Pauser.h"
#include "Core/Spawner.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Networking/NetworkManager.h"
#include "Physics/PhysicsMain.h"
#include "Physics/CollisionShapeManager.h"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/Materials/PhysicsMaterialFactory.h"
#include "Physics/TriggerRegions/TriggerReporter.h"
#include "Players/PlayerManager.h"
#include "Sound/SoundMain.h"
#include "Thing/ThingDatabase.h"

using namespace Ogre;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Networking;
using namespace Ponykart::Physics;
using namespace Ponykart::Players;
using namespace Ponykart::Sound;
using namespace PonykartParsers;
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
	getG<PhysicsMain>()->loadPhysicsLevel(Settings::MainMenuName);
	addGlobalObject(new CameraManager());

	// Input
	splash.increment("Starting input system...");
	addGlobalObject(new InputMain());
	addGlobalObject(new KeyBindingManager());
	addGlobalObject(new InputSwallowerManager());
	addGlobalObject(new Pauser());

	// spawner
	splash.increment("Creating spawners...");
	addGlobalObject(new WheelFactory());
	addGlobalObject(new ThingDatabase());
	addGlobalObject(new Spawner());
	addGlobalObject(new AnimationManager());

	// networking
	splash.increment("Creating Network manager...");
	addGlobalObject(new NetworkManager());

	// GUI
	//splash.increment("Initialising Miyagi...");
	//addGlobalObject(new DebugOverlayManager());
	//addGlobalObject(new UIMain());
	//addGlobalObject(new LuaConsoleManager());
	//addGlobalObject(new MainMenuManager());
	//addGlobalObject(new GameUIManager());

	// lua
	//splash.increment("Registering handlers and wrappers...");
	//setupHandlers();
	//splash.increment("Setting up Lua engine and wrappers...");
	//auto lua = addGlobalObject(new LuaMain());
	//loadLuaWrappers();
	//lua->runRegisterEvent();

	// players
	//splash.increment("Spawning players...");
	//addGlobalObject(new PlayerManager());
	//addGlobalObject(new RaceCountdown());
	//addGlobalObject(new LapCounter());
	//addGlobalObject(new LThingHelperManager());

	//items
	//splash.increment("Loading items...");
	//addGlobalObject(new ItemManager());

	// Ogre : electric boogaloo
	//splash.increment("Setting up static and instanced geometry managers...");
	//addGlobalObject(new StaticGeometryManager());
	//addGlobalObject(new InstancedGeometryManager());
	//addGlobalObject(new ImposterBillboarder());

	// Sound
	addGlobalObject(new SoundMain());

	// handlers
	//splash.increment("Loading global handlers...");
	//LoadGlobalHandlers();

	// Post-init
	//splash.increment("Running post-initialisation events...");
	//levelManager->runPostInitEvents();
}
