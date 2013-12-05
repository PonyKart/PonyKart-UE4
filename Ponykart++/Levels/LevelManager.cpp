#include "Levels/LevelManager.h"

using namespace Ponykart;
using namespace Levels;

const float LevelManager::INITIAL_DELAY{0.1f};

LevelManager::LevelManager()
{
	isValidLevel = false;
	hasRunPostInitEvents = false;
	elapsed = 0;
	frameOneRendered = frameTwoRendered = false;

}
