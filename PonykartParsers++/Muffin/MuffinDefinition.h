#ifndef MUFFINDEFINITION_H_INCLUDED
#define MUFFINDEFINITION_H_INCLUDED

#include <string>
#include <vector>
#include "TokenHolder.h"
#include "Muffin/ThingBlock.h"

namespace PonykartParsers
{
// Represents a .muffin file
class MuffinDefinition : public TokenHolder // TODO: Implement this class and TokenHolder properly
{
public:
	MuffinDefinition(std::string Name);
	void finish() override;
	// Getters
	std::string getName();
	std::vector<ThingBlock> getThingBlocks();
	std::vector<std::string> getExtraFiles();
private: // Set-private public members
	std::string name;
	std::vector<ThingBlock> thingBlocks;
	std::vector<std::string> extraFiles; // Other .muffin files this one should load.
};
} // PonykartParsers

#endif // MUFFINDEFINITION_H_INCLUDED
