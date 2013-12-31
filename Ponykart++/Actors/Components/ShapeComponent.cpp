#include "Actors/Components/ShapeComponent.h"
#include "Kernel/LKernel.h"
#include "Thing/Blocks/ShapeBlock.h"

using namespace PonykartParsers;
using namespace Ponykart::Actors;

ShapeComponent::ShapeComponent(LThing* lthing, PonykartParsers::ShapeBlock* block)
{
	auto sceneMgr = LKernel::gSceneManager;

	transform = block->getTransform();

	type = block->getEnumProperty("type");
	switch (type)
	{
		case ThingEnum::Box:
		case ThingEnum::Cylinder:
			dimensions = block->getVectorProperty("dimensions") / 2.0;
			break;
		case ThingEnum::Capsule:
		case ThingEnum::Cone:
			height = block->getFloatProperty("height");
			radius = block->getFloatProperty("radius");
			break;
		case ThingEnum::Sphere:
			radius = block->getFloatProperty("radius");
			break;
		case ThingEnum::Heightmap:
			minHeight = block->getFloatProperty("MinHeight");
			maxHeight = block->getFloatProperty("MaxHeight");
			width = (int) block->getFloatProperty("Width");
			length = (int) block->getFloatProperty("Length");
			mesh = block->getStringProperty("mesh");
			dimensions = block->getVectorProperty("dimensions") / 2.0;
			break;
		case ThingEnum::Hull:
		case ThingEnum::Mesh:
			mesh = block->getStringProperty("mesh");
			break;
		default: break;
	}
}
