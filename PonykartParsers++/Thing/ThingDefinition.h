#ifndef THINGDEFINITION_H_INCLUDED
#define THINGDEFINITION_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include <vector>
#include <string>
#include "Muffin/TokenHolder.h"

namespace PonykartParsers
{
	class ShapeBlock;
	class ModelBlock;
	class RibbonBlock;
	class BillboardSetBlock;
	class SoundBlock;

    /// Represents a .thing file - these are one per file and not one per LThing object!
    class ThingDefinition : public TokenHolder
    {
    public:
		DLLEXPORT ThingDefinition(const std::string& Name);
		DLLEXPORT ~ThingDefinition();
		DLLEXPORT void finish() override; ///< Must be called after you're done importing everything into the dictionaries
		DLLEXPORT void addShapeBlock(ShapeBlock* block); ///< Adds a block to the vector
		DLLEXPORT void addModelBlock(ModelBlock* block); ///< Adds a block to the vector
		DLLEXPORT void addRibbonBlock(RibbonBlock* block); ///< Adds a block to the vector
		DLLEXPORT void addBillboardSetBlock(BillboardSetBlock* block); ///< Adds a block to the vector
		DLLEXPORT void addSoundBlock(SoundBlock* block); ///< Adds a block to the vector
		// Getters
		DLLEXPORT  std::string getName() const;
		DLLEXPORT  const std::vector<ShapeBlock*>& getShapeBlocks() const; ///< Gets a const ref to the vector
		DLLEXPORT const std::vector<ModelBlock*>& getModelBlocks() const; ///< Gets a const ref to the vector
		DLLEXPORT const std::vector<RibbonBlock*>& getRibbonBlocks() const; ///< Gets a const ref to the vector
		DLLEXPORT const std::vector<BillboardSetBlock*>& getBillboardSetBlocks() const; ///< Gets a const ref to the vector
		DLLEXPORT 	const std::vector<SoundBlock*>& getSoundBlocks() const; ///< Gets a const ref to the vector
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
