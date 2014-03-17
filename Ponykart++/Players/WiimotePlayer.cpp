#include "pch.h"
#include "Players/WiimotePlayer.h"

using namespace Ponykart::Levels;
using namespace Ponykart::Players;

WiimotePlayer::WiimotePlayer(LevelChangedEventArgs* eventArgs, int id)
: Player(eventArgs, id, false)
{
	throw std::string("WiimotePlayer::WiimotePlayer: Not implemented ! Need wiimotelib first");
}

void WiimotePlayer::useItem()
{
	throw std::string("Not implemented. Wasn't either in the C# version.");
}
