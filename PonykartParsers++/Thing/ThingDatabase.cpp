#include "Thing/ThingDatabase.h"
#include "Thing/ThingImporter.h"

using namespace std;
using namespace PonykartParsers;

/// Returns the ThingDefinition for the given name
/** If the definition is not in the map, we'll try to load it with the ThingImporter */
ThingDefinition* ThingDatabase::getThingDefinition(const std::string& name)
{
    auto it = definitions.find(name);
    if (it != definitions.end())
		return it->second;
	else
	{
		ThingDefinition* def = ThingImporter().parse(name);
		definitions.insert(pair<string,ThingDefinition*>(name, def));
		return def;
	}
}
