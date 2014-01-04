#ifndef MODELBLOCK_H_INCLUDED
#define MODELBLOCK_H_INCLUDED

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

// Represents a Model { } block in a .thing file
class ModelBlock : public TokenHolder
{
public:
	DLLEXPORT ModelBlock(ThingDefinition* Owner);
	// Getters
	DLLEXPORT const ThingDefinition* const getOwner();
protected:
		ThingDefinition* owner;
};

} // PonykartParsers

#endif // MODELBLOCK_H_INCLUDED
