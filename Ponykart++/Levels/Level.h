#ifndef LEVEL_H_INCLUDED
#define LEVEL_H_INCLUDED

#include <string>
#include <unordered_map>
#include "Levels/LevelType.h"
#include "Muffin/MuffinDefinition.h"
#include "Actors/LThing.h"

namespace Ponykart
{
namespace Levels
{
// Represents a level or world in the game.
class Level // TODO: Translate the whole class correctly
{
public:
	// Getters
	const std::string& getName();
	LevelType getType();
	PonykartParsers::MuffinDefinition* getDefinition();
	std::unordered_map<std::string, Actors::LThing> getThings();
private:
	// Set-private members. Getters are public.
	std::string name; // The world's name - this serves as its identifier
	LevelType type; // The type of this level
	PonykartParsers::MuffinDefinition* definition;
	std::unordered_map<std::string, Actors::LThing> things; // We use the thing's Name as the key
};
} // Levels
} // Ponykart

#endif // LEVEL_H_INCLUDED
