#include "Thing/ThingDefinition.h"
#include "Thing/Blocks/ShapeBlock.h"
#include "Thing/Blocks/ModelBlock.h"
#include "Thing/Blocks/RibbonBlock.h"
#include "Thing/Blocks/BillboardSetBlock.h"
#include "Thing/Blocks/SoundBlock.h"

using namespace std;
using namespace PonykartParsers;

ThingDefinition::ThingDefinition(const std::string& Name)
 : name(Name)
{
}

void ThingDefinition::finish()
{
	for (ShapeBlock* sb : shapeBlocks)
		sb->finish();
	for (ModelBlock* mb : modelBlocks)
		mb->finish();
	for (RibbonBlock* rb : ribbonBlocks)
		rb->finish();
	for (BillboardSetBlock* bb : billboardSetBlocks)
		bb->finish();
	for (SoundBlock* sb : soundBlocks)
		sb->finish();
}

const vector<ShapeBlock*>& ThingDefinition::getShapeBlocks() const
{
	return shapeBlocks;
}

const vector<ModelBlock*>& ThingDefinition::getModelBlocks() const
{
	return modelBlocks;
}

const vector<RibbonBlock*>& ThingDefinition::getRibbonBlocks() const
{
	return ribbonBlocks;
}

const vector<BillboardSetBlock*>& ThingDefinition::getBillboardSetBlocks() const
{
	return billboardSetBlocks;
}

const vector<SoundBlock*>& ThingDefinition::getSoundBlocks() const
{
	return soundBlocks;
}
