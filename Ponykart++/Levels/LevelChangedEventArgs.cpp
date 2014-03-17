#include "pch.h"
#include "Levels/LevelChangedEventArgs.h"

using namespace Ponykart::Levels;

LevelChangedEventArgs::LevelChangedEventArgs(const Level& NewLevel, const Level& OldLevel, LevelChangeRequest& Request)
: newLevel(NewLevel), oldLevel(OldLevel), request(Request)
{

}
