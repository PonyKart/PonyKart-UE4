#include "pch.h"
#include "Items/SmartApple.h"
#include "Kernel/LKernel.h"
#include "Players/Player.h"
#include "Players/PlayerManager.h"

using namespace Ponykart::Items;
using namespace Ponykart::LKernel;
using namespace Ponykart::Players;

SmartApple::SmartApple(Player* user)
: Item(user, "SpeedMuffin")
{
	origin = user->getNodePosition();
	playerManager = LKernel::getG<PlayerManager>();
}
