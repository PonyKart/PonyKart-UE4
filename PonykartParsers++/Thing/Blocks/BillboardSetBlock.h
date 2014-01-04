#ifndef BILLBOARDSETBLOCK_H_INCLUDED
#define BILLBOARDSETBLOCK_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include <vector>
#include "Muffin/TokenHolder.h"
#include "Thing/Blocks/BillboardBlock.h"

namespace PonykartParsers
{

class ThingDefinition;

/// Represents a BillboardSet { } block in a .thing file
class BillboardSetBlock : public TokenHolder
{
public:
	DLLEXPORT BillboardSetBlock(ThingDefinition* Owner);
	DLLEXPORT ~BillboardSetBlock();
	DLLEXPORT void addBillboardBlock(BillboardBlock* block);
	// Getters
	DLLEXPORT const ThingDefinition* const getOwner() const;
	DLLEXPORT const std::vector<BillboardBlock*>& getBillboardBlocks() const;
protected:
	ThingDefinition* owner;
	std::vector<BillboardBlock*> billboardBlocks;
};
} // PonykartParsers

#endif // BILLBOARDSETBLOCK_H_INCLUDED
