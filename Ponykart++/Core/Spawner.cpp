#include "Actors/DashJavelin.h"
#include "Actors/Driver.h"
#include "Actors/TwiCutlass.h"
#include "Core/Pauser.h"
#include "Core/Spawner.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelManager.h"
#include "Thing/ThingDatabase.h"

using namespace std;
using namespace PonykartParsers;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;

SpawnEvent<LThing*> Spawner::onThingCreation;
SpawnEvent<Kart*> Spawner::onKartCreation;
SpawnEvent<Driver*> Spawner::onDriverCreation;

Spawner::Spawner()
{
	database = LKernel::getG<ThingDatabase>();
	levelManager = LKernel::getG<LevelManager>();
}

Kart* Spawner::spawnKart(std::string thingName, PonykartParsers::ThingBlock* thingTemplate)
{
	if (Pauser::isPaused)
		throw string("Attempted to spawn \"" + thingName + "\" while paused!");

	_spawnLock.lock();
	try
	{
		auto definition = database->getThingDefinition(thingName);
		Kart* kart;
		if (thingName == "DashJavelin")
			kart = new DashJavelin(thingTemplate, definition);
		//else if (thingName == "TwiCutlass") //TODO: Implement TwiCutlass
		//	kart = new TwiCutlass(thingTemplate, definition);
		else
			kart = new Kart(thingTemplate, definition);
		levelManager->getCurrentLevel()->addThing(kart);

		invoke(onKartCreation, kart);
		invoke(onThingCreation, (LThing*)kart);
		_spawnLock.unlock();
		return kart;
	}
	catch (...)
	{
		_spawnLock.unlock();
		throw string("Spawner::spawnKart: Unknown exception caught ! Emergency unlock of _spawnLock.");
	}
}

Driver* Spawner::spawnDriver(string thingName, ThingBlock* thingTemplate)
{
	if (Pauser::isPaused)
		throw string("Spawner::spawnDriver: Attempted to spawn \"" + thingName + "\" while paused!");
	_spawnLock.lock();
	try
	{
		auto definition = database->getThingDefinition(thingName);
		Driver* driver = new Driver(thingTemplate, definition);

		levelManager->getCurrentLevel()->addThing((LThing*)driver);

		invoke(onDriverCreation, driver);
		invoke<LThing*>(onThingCreation, (LThing*)driver);
		_spawnLock.unlock();
		return driver;
	}
	catch (...)
	{
		_spawnLock.unlock();
		throw string("Spawner::spawnDriver: Unknown exception caught ! Emergency unlock of _spawnLock.");
	}
}

template<typename T> void Spawner::invoke(SpawnEvent<T> evt, T actor)
{
	if (evt.size())
		for (auto fun : evt)
			fun(actor);
}
