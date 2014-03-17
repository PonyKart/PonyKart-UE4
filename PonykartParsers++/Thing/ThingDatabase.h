#ifndef THINGDATABASE_H_INCLUDED
#define THINGDATABASE_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifndef DLLEXPORT
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif
#endif

#include <unordered_map>
#include <string>
#include "Kernel/LKernelObject.h"
#include "Thing/ThingDefinition.h"

namespace PonykartParsers
{

class ThingDatabase : public Ponykart::LKernel::LKernelObject
{
public:
	DLLEXPORT ThingDatabase() = default;
	DLLEXPORT ThingDefinition* getThingDefinition(const std::string& name);
public:
	std::unordered_map<std::string, ThingDefinition*> definitions;
};

} // PonykartParsers

#endif // THINGDATABASE_H_INCLUDED
