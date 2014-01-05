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

PonykartParsers::ThingEnum ShapeComponent::getType() const
{
    return type;
}

Ogre::Vector3 ShapeComponent::getDimensions() const
{
    return dimensions;
}

float ShapeComponent::getRadius() const
{
    return radius;
}

float ShapeComponent::getHeight() const
{
    return height;
}

std::string ShapeComponent::getMesh() const
{
    return mesh;
}

float ShapeComponent::getMinHeight() const
{
    return minHeight;
}

float ShapeComponent::getMaxHeight() const
{
    return maxHeight;
}

int ShapeComponent::getWidth() const
{
    return width;
}

int ShapeComponent::getLength() const
{
    return height;
}
