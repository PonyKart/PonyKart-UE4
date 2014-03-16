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

void PlayerManager::raceCountdown_onCountdown(RaceCountdownState state)
{
	if (state == RaceCountdownState::Go)
		for (auto player : players)
			player->isControlEnabled = true;
}

void PlayerManager::onLevelUnload(LevelChangedEventArgs* eventArgs)
{
	if (eventArgs->oldLevel.getType() == LevelType::Race) 
	{
		for (int a = 0; a < players.size(); a++) 
		{
			if (players[a] != nullptr) 
			{
				players[a]->detach();
				delete players[a];
				players[a] = nullptr;
			}
		}
		mainPlayer = nullptr;
	}
}