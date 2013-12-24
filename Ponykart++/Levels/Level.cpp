#include "Levels/Level.h"

using namespace Ponykart;
using namespace Ponykart::Levels;

const std::string& Level::getName() const
{
	return name;
}

LevelType Level::getType() const
{
	return type;
}
