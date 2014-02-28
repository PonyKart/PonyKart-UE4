#include "pch.h"
#include "Players/Player.h"
#include "Players/PlayerManager.h"

using namespace Ponykart::Players;

const Player* const PlayerManager::getMainPlayer() const
{
	return mainPlayer;
}