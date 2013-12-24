#ifndef THINGDEFINITION_H_INCLUDED
#define THINGDEFINITION_H_INCLUDED

#include <vector>
#include <string>
#include "TokenHolder.h"

namespace PonykartParsers
{
	class ShapeBlock;
	class ModelBlock;
	class RibbonBlock;
	class BillboardSetBlock;
	class SoundBlock;

    // Represents a .thing file - these are one per file and not one per LThing object!
    class ThingDefinition : public TokenHolder
    {
    public:
        ThingDefinition(std::string name);
        ~ThingDefinition();
        void finish() override; // Must be called after you're done importing everything into the dictionaries
        // Getters
        std::string getName() const;
        const std::vector<ShapeBlock*>& getShapeBlocks() const;
		const std::vector<ModelBlock*>& getModelBlocks() const;
		const std::vector<RibbonBlock*>& getRibbonBlocks() const;
		const std::vector<BillboardSetBlock*>& getBillboardSetBlocks() const;
		const std::vector<SoundBlock*>& getSoundBlocks() const;
    protected:
		std::string name;
		std::vector<ShapeBlock*> shapeBlocks;
		std::vector<ModelBlock*> modelBlocks;
		std::vector<RibbonBlock*> ribbonBlocks;
		std::vector<BillboardSetBlock*> billboardSetBlocks;
		std::vector<SoundBlock*> soundBlocks;
	};
} // PonykartParsers

#endif // THINGDEFINITION_H_INCLUDED
