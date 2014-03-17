#include "pch.h"
#include "Items/ItemManager.h"
#include "Items/SmartApple.h"
#include "Items/SpeedMuffin.h"

using namespace std;
using namespace Ponykart::Players;
using namespace Ponykart::Items;

Item* ItemManager::spawnItem(Player* user, std::string itemName)
{
	Item* spawnedItem = nullptr;

	//There is probably a better way to do this.
	if (itemName == "SmartApple")
	{
		spawnedItem = new SmartApple(user);
		activeItems.push_back(spawnedItem);
	}
	else if (itemName == "SpeedMuffin")
	{
		spawnedItem = new SpeedMuffin(user);
		activeItems.push_back(spawnedItem);
	}

	return spawnedItem;
}
