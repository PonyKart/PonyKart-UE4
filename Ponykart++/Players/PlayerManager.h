#ifndef PLAYERMANAGER_H_INCLUDED
#define PLAYERMANAGER_H_INCLUDED

#include <functional>
#include <vector>
#include "Core/RaceCountdown.h"
#include "Kernel/LKernelObject.h"

namespace Ponykart
{
namespace Levels{ class LevelChangedEventArgs; }
namespace Players
{
class Player;

using PlayerEvent = std::vector<std::function<void ()>>;

// This class manages the players
class PlayerManager : public LKernel::LKernelObject // TODO: Implement PlayerManager properly
{
public:
	 PlayerManager(); // Hook up to the level load/unload events
	// Getters
	const Player* const getMainPlayer() const;
	const std::vector<Player*>* const getPlayers() const;
public:
	PlayerEvent onPostPlayerCreation;
	void onLevelLoad(Levels::LevelChangedEventArgs* eventArgs);
	void onLevelUnload(Levels::LevelChangedEventArgs* eventArgs);
	void raceCountdown_onCountdown(Core::RaceCountdownState state);
private:
	Player* mainPlayer;
	std::vector<Player*> players;
};

} // Players
} // Ponykart

#endif // PLAYERMANAGER_H_INCLUDED
