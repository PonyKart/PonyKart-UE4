#ifndef BILLBOARDSETBLOCK_H_INCLUDED
#define BILLBOARDSETBLOCK_H_INCLUDED

#include <vector>
#include "TokenHolder.h"
#include "Thing/BillboardBlock.h"

namespace PonykartParsers
{

class ThingDefinition;

// Represents a BillboardSet { } block in a .thing file
class BillboardSetBlock : public TokenHolder
{
public:
	BillboardSetBlock(ThingDefinition* Owner);
	~BillboardSetBlock();
	// Getters
	const ThingDefinition* const getOwner() const;
	const std::vector<BillboardBlock*>& getBillboardBlocks() const;
protected:
	ThingDefinition* owner;
	std::vector<BillboardBlock*> billboardBlocks;
};
} // PonykartParsers

#endif // BILLBOARDSETBLOCK_H_INCLUDED
