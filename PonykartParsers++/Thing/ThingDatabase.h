#ifndef THINGDATABASE_H_INCLUDED
#define THINGDATABASE_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include <unordered_map>
#include <string>
#include "Thing/ThingDefinition.h"

namespace PonykartParsers
{

class ThingDatabase
{
public:
	DLLEXPORT ThingDatabase();
	DLLEXPORT ThingDefinition* getThingDefinition(const std::string& name);
public:
	std::unordered_map<std::string, ThingDefinition*> definitions;
};

} // PonykartParsers

#endif // THINGDATABASE_H_INCLUDED
