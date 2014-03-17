#include "pch.h"
#include <string>
#include "Actors/Kart.h"
#include "Core/Spawner.h"
#include "Items/Item.h"
#include "Kernel/LKernel.h"
#include "Misc/OgreExtensions.h"
#include "Players/Player.h"

using namespace Extensions;
using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ponykart::Items;
using namespace Ponykart::Players;

Item::Item(Player* User, std::string itemName)
{
	user = User;
	try
	{
		body = LKernel::getG<Spawner>()->spawn(itemName, user->getKart()->getActualPosition() 
												+ toOgreVector3(user->getKart()->getVehicle()->getForwardVector()) * 5.0f);
	}
	catch (...)
	{
	}

	onUse();
	LKernel::onEveryUnpausedTenthOfASecondEvent.push_back(bind(&Item::everyTenth,this,std::placeholders::_1));
}