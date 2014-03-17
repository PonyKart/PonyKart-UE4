#include "pch.h"
#include "Items/SpeedMuffin.h"
#include "Players/Player.h"

using namespace Ponykart::Items;
using namespace Ponykart::Players;

SpeedMuffin::SpeedMuffin(Player* user)
: Item(user, "SpeedMuffin")
{
	origin = user->getNodePosition();
	//User = user;
}
