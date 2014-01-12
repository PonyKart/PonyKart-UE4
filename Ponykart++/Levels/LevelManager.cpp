#include "Levels/LevelManager.h"

using namespace Ponykart;
using namespace Levels;

const float LevelManager::INITIAL_DELAY{0.1f};
LevelProgressEvent LevelManager::onLevelLoadProgress;
LevelEvent LevelManager::onLevelPreUnload; // Is fired a few frames before we even start unloading anything. Mostly used for stuff that still requires screen rendering, such as putting up a loading screen
LevelEvent LevelManager::onLevelLoad; // Is fired after the .muffins have been read and the .scene file has been used, but before we start actually creating any Things
LevelEvent LevelManager::onLevelUnload; // Is fired after the level handlers have been disposed, but before we clean out the scene manager.
LevelEvent LevelManager::onLevelPostLoad; // Is fired a few frames after the entire level load process, including after scripts have been run.

LevelManager::LevelManager()
{
	isValidLevel = false;
	hasRunPostInitEvents = false;
	elapsed = 0;
	frameOneRendered = frameTwoRendered = false;

}
