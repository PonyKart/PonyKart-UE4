#ifndef RIBONBLOCK_H_INCLUDED
#define RIBONBLOCK_H_INCLUDED

#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a Ribbon { } block in a .thing file
class RibbonBlock : public TokenHolder
{
public:
	RibbonBlock(ThingDefinition* Owner);
	// Getters
	ThingDefinition* getOwner();

protected:
	ThingDefinition* owner;
};
} // PonykartParsers


#endif // RIBONBLOCK_H_INCLUDED
