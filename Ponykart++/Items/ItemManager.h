#ifndef ITEMMANAGER_H_INCLUDED
#define ITEMMANAGER_H_INCLUDED
#include <string>
#include <vector>
#include "Items/Item.h"

namespace Ponykart
{
namespace Players{ class Player; }
namespace Items
{
	class ItemManager
	{
	public:
		ItemManager();
		Item* spawnItem(Players::Player* user, std::string itemName);

	public:
		std::vector<Item*> activeItems;
	};
}
}
#endif // ITEMMANAGER_H_INCLUDED
