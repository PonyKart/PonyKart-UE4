#ifndef SOUNDBLOCK_H_INCLUDED
#define SOUNDBLOCK_H_INCLUDED

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

// Represents a Sound { } block in the .thing file
class SoundBlock : public TokenHolder
{
public:
	DLLEXPORT SoundBlock(ThingDefinition* Owner);
	// Getters
	DLLEXPORT const ThingDefinition* const getOwner();
protected:
	ThingDefinition* owner;
};
} // PonykartParsers


#endif // SOUNDBLOCK_H_INCLUDED
