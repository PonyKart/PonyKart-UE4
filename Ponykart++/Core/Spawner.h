#ifndef SPAWNER_H_INCLUDED
#define SPAWNER_H_INCLUDED

#include <functional>
#include <mutex>
#include <string>
#include <vector>
#include <Ogre.h>
#include "Kernel/LKernelObject.h"

namespace PonykartParsers
{
	class ThingBlock;
	class ThingDatabase;
} // PonykartParsers

namespace Ponykart
{

namespace Actors
{
	class LThing;
	class Kart;
	class Driver;
} // Actors

namespace Levels
{
	class LevelManager;
} // Levels

namespace Core
{

template<typename T> using SpawnEvent = typename std::vector<std::function<void (T thing)>>; ///< Can take arguments of any type

class Spawner : public LKernel::LKernelObject
{
public:
	Spawner();
	Actors::LThing* spawn(std::string thingName, PonykartParsers::ThingBlock* thingTemplate);
	Actors::LThing* spawn(std::string thingName, Ogre::Vector3 spawnPos);

	// TODO: The templated Spawn
	// Specific spawners
	Actors::Kart* spawnKart(std::string thingName, PonykartParsers::ThingBlock* thingTemplate); ///< Spawns a kart
	Actors::Driver* spawnDriver(std::string thingName, PonykartParsers::ThingBlock* thingTemplate); ///< Spawns a driver
private:
	template<typename T> void invoke(SpawnEvent<T> evt, T actor);
public:
	static SpawnEvent<Actors::LThing*> onThingCreation; ///< Fires whenever anything is spawned.
	static SpawnEvent<Actors::Kart*> onKartCreation; ///< Fires whenever a Kart is spawned.
	static SpawnEvent<Actors::Driver*> onDriverCreation; ///< Fires whenever a Driver is spawned.
private:
	PonykartParsers::ThingDatabase* database;
	Levels::LevelManager* levelManager;
	std::mutex _spawnLock;
};
} // Core
} // Ponykart

#endif // SPAWNER_H_INCLUDED
