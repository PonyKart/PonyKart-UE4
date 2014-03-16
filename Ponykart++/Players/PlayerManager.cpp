#include "pch.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Players/Player.h"
#include "Players/PlayerManager.h"

using namespace std;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Players;

PlayerManager::PlayerManager()
{
	log("[Loading] Creating PlayerManager...");
	LevelManager::onLevelLoad.push_back(bind(&PlayerManager::onLevelLoad,this,placeholders::_1));
	LevelManager::onLevelUnload.push_back(bind(&PlayerManager::onLevelUnload, this, placeholders::_1));

	RaceCountdown::onCountdown.push_back(bind(&PlayerManager::raceCountdown_onCountdown, this, placeholders::_1));
}

const Player* const PlayerManager::getMainPlayer() const
{
	return mainPlayer;
}