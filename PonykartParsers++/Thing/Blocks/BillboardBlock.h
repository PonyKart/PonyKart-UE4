#ifndef BILLBOARDBLOCK_H_INCLUDED
#define BILLBOARDBLOCK_H_INCLUDED

#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class BillboardSetBlock;

// Represents a Billboard { } block in a .thing file
class BillboardBlock : public TokenHolder
{
public:
	BillboardBlock(BillboardSetBlock* Owner);
	// Getters
	BillboardSetBlock* getOwner();

protected:
	BillboardSetBlock* owner;
};
} // PonykartParsers

#endif // BILLBOARDBLOCK_H_INCLUDED
