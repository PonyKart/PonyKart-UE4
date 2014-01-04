#ifndef RIBONBLOCK_H_INCLUDED
#define RIBONBLOCK_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a Ribbon { } block in a .thing file
class RibbonBlock : public TokenHolder
{
public:
	DLLEXPORT RibbonBlock(ThingDefinition* Owner);
	// Getters
	DLLEXPORT ThingDefinition* getOwner();

protected:
	ThingDefinition* owner;
};
} // PonykartParsers


#endif // RIBONBLOCK_H_INCLUDED
