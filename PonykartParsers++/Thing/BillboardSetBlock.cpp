#include "Thing/BillboardSetBlock.h"

using namespace PonykartParsers;

BillboardSetBlock::BillboardSetBlock(ThingDefinition* Owner) : owner(Owner)
{
}

BillboardSetBlock::~BillboardSetBlock()
{
	billboardBlocks.clear();
}

const std::vector<BillboardBlock*>& BillboardSetBlock::getBillboardBlocks() const
{
    return billboardBlocks;
}
