#ifndef SPAWNER_H_INCLUDED
#define SPAWNER_H_INCLUDED

#include <functional>
#include <vector>
#include <string>
#include <Ogre.h>

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

class Spawner
{
public:
	Spawner();
	Actors::LThing* Spawn(std::string thingName, PonykartParsers::ThingBlock* thingTemplate); ///< Spawns something. Returns nullptr is you're paused.
	Actors::LThing* Spawn(std::string thingName, Ogre::Vector3 spawnPos);
	// TODO: The templated Spawn
	// Specific spawners
	Actors::Kart* spawnKart(std::string thingName, PonykartParsers::ThingBlock* thingTemplate); ///< Spawns a kart
	Actors::Driver* spawnDriver(std::string thingName, PonykartParsers::ThingBlock* thingTemplate); ///< Spawns a driver
private:
	void inline _lock(); // Lock the _spawnLock
	void inline _unlock(); // Unlock the _spawnLock
	template<typename T> void invoke(SpawnEvent<T> evt, T actor);
public:
	static SpawnEvent<Actors::LThing*> onThingCreation; ///< Fires whenever anything is spawned.
	static SpawnEvent<Actors::Kart*> onKartCreation; ///< Fires whenever a Kart is spawned.
	static SpawnEvent<Actors::Driver*> onDriverCreation; ///< Fires whenever a Driver is spawned.
private:
	PonykartParsers::ThingDatabase* database;
	Levels::LevelManager* levelManager;
	bool _spawnLock; ///< No mutexes with MinGW 4.8.1 TODO: Use a real mutex (when switching to CMake ?)
};
} // Core
} // Ponykart

#endif // SPAWNER_H_INCLUDED
