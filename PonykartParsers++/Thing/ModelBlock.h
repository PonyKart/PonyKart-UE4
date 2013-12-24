#ifndef MODELBLOCK_H_INCLUDED
#define MODELBLOCK_H_INCLUDED

#include "TokenHolder.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a Model { } block in a .thing file
class ModelBlock : public TokenHolder
{
public:
	ModelBlock(ThingDefinition* Owner);
	// Getters
	const ThingDefinition* const getOwner();
protected:
		ThingDefinition* owner;
};

} // PonykartParsers

#endif // MODELBLOCK_H_INCLUDED
