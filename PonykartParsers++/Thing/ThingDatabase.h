#ifndef THINGDATABASE_H_INCLUDED
#define THINGDATABASE_H_INCLUDED

#include <unordered_map>
#include <string>
#include "Thing/ThingDefinition.h"

namespace PonykartParsers
{

class ThingDatabase
{
public:
	ThingDatabase();
	ThingDefinition* getThingDefinition(std::string name);
public:
	std::unordered_map<std::string, ThingDefinition> definitions;
};

} // PonykartParsers

#endif // THINGDATABASE_H_INCLUDED
