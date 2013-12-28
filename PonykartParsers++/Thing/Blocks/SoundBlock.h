#ifndef SOUNDBLOCK_H_INCLUDED
#define SOUNDBLOCK_H_INCLUDED

#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a Sound { } block in the .thing file
class SoundBlock : public TokenHolder
{
public:
	SoundBlock(ThingDefinition* Owner);
	// Getters
	const ThingDefinition* const getOwner();
protected:
	ThingDefinition* owner;
};
} // PonykartParsers


#endif // SOUNDBLOCK_H_INCLUDED
