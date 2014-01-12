#include "Thing/Blocks/BillboardSetBlock.h"

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

void BillboardSetBlock::addBillboardBlock(BillboardBlock* block)
{
	billboardBlocks.push_back(block);
}
