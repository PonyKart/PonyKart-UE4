#ifndef BILLBOARDBLOCK_H_INCLUDED
#define BILLBOARDBLOCK_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{

class BillboardSetBlock;

// Represents a Billboard { } block in a .thing file
class BillboardBlock : public TokenHolder
{
public:
	DLLEXPORT BillboardBlock(BillboardSetBlock* Owner);
	// Getters
	DLLEXPORT BillboardSetBlock* getOwner();

protected:
	BillboardSetBlock* owner;
};
} // PonykartParsers

#endif // BILLBOARDBLOCK_H_INCLUDED
