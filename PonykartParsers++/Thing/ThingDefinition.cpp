#include "Thing/ThingDefinition.h"

using namespace std;
using namespace PonykartParsers;

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
