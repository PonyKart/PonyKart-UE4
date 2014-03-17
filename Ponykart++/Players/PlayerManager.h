#ifndef PLAYERMANAGER_H_INCLUDED
#define PLAYERMANAGER_H_INCLUDED

#include <functional>
#include <string>
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
	/// When a level loads, we create the players. For now, we just have one human player and 7 computer-controlled ones
	void onLevelLoad(Levels::LevelChangedEventArgs* eventArgs);
	void onLevelUnload(Levels::LevelChangedEventArgs* eventArgs); ///< Delete all of the players
	void raceCountdown_onCountdown(Core::RaceCountdownState state);
	std::vector<std::string> fillCharacterString(std::vector<std::string> characters);
public:
	static const std::string availableCharacters[];
	static const std::string defaultCharacter;
private:
	Player* mainPlayer;
	std::vector<Player*> players;
};

} // Players
} // Ponykart

#endif // PLAYERMANAGER_H_INCLUDED
