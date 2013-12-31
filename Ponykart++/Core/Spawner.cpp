#include "Actors/DashJavelin.h"
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

Spawner::Spawner() : _spawnLock(false)
{
	database = LKernel::getG<ThingDatabase>();
	levelManager = LKernel::getG<LevelManager>();
}

void Spawner::_lock()
{
	while(_spawnLock);
	_spawnLock=true;
}

void Spawner::_unlock()
{
	if (_spawnLock)
		_spawnLock=false;
	else
		throw string("Spawnler::_unlock: Lock is already unlocked !");
}

Kart* Spawner::spawnKart(std::string thingName, PonykartParsers::ThingBlock* thingTemplate)
{
	if (Pauser::isPaused) {
		throw string("Attempted to spawn \"" + thingName + "\" while paused!");
	}
	_lock();
	{
		auto definition = database->getThingDefinition(thingName);

		Kart* kart;
		if (thingName == "DashJavelin")
			kart = new DashJavelin(thingTemplate, definition);
		else if (thingName == "TwiCutlass")
			kart = new TwiCutlass(thingTemplate, definition);
		else
			kart = new Kart(thingTemplate, definition);
		levelManager->getCurrentLevel()->addThing(kart);

		invoke(onKartCreation, kart);
		invoke(onThingCreation, (LThing*)kart);
		return kart;
	}
	_unlock();
}

template<typename T> void Spawner::invoke(SpawnEvent<T> evt, T actor)
{
	if (evt.size())
		for (auto fun : evt)
			fun(actor);
}
