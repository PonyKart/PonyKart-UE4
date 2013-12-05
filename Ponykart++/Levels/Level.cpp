#include "Levels/Level.h"

using namespace Ponykart;
using namespace Ponykart::Levels;

const std::string& Level::getName()
{
	return name;
}

LevelType Level::getType()
{
	return type;
}
