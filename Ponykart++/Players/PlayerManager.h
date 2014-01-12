#ifndef PLAYERMANAGER_H_INCLUDED
#define PLAYERMANAGER_H_INCLUDED

#include <functional>
#include <vector>

namespace Ponykart
{
namespace Players
{
class Player;

using PlayerEvent = std::vector<std::function<void ()>>;

// This class manages the players
class PlayerManager // TODO: Implement PlayerManager properly
{
public:
	 PlayerManager(); // Hook up to the level load/unload events
	// Getters
	const Player* const getMainPlayer() const;
	const std::vector<Player*>* const getPlayers() const;
public:
	PlayerEvent onPostPlayerCreation;
private:
	Player* mainPlayer;
	std::vector<Player*> players;
};

} // Players
} // Ponykart

#endif // PLAYERMANAGER_H_INCLUDED
