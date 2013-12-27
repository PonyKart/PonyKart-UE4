#include <sstream>
#include "Actors/LThing.h"
#include "Actors/Components/BillboardSetComponent.h"
#include "Actors/Components/ModelComponent.h"
#include "Core/IDs.h"
#include "Kernel/LKernel.h"
#include "Misc/ogreExtensions.h"
#include "Muffin/ThingBlock.h"
#include "Thing/BillboardSetBlock.h"

using namespace std;
using namespace Ogre;
using namespace Ponykart::Actors;
using namespace PonykartParsers;

BillboardSetComponent::BillboardSetComponent(LThing* lthing, ThingBlock* thingTemplate, BillboardSetBlock* block)
{
	id = IDs::incremental();
	ostringstream ssId; ssId<<id;
	name = block->getStringProperty("name", thingTemplate->getThingName());

	// set it up
	billboardSet = LKernel::gSceneManager->createBillboardSet(name + ssId.str() + "BillboardSet", (unsigned) block->getBillboardBlocks().size());
	billboardSet->setMaterialName(block->getStringProperty("material"));
	billboardSet->setCastShadows(false); //block->getBoolProperty("CastsShadows", false);
	billboardSet->setDefaultDimensions(block->getFloatProperty("width", 1), block->getFloatProperty("height", 1));

	// billboard type
	ThingEnum type = block->getEnumProperty("Type", ThingEnum::Point);
	switch (type)
	{
		case ThingEnum::OrientedCommon:
			billboardSet->setBillboardType(BillboardType::BBT_ORIENTED_COMMON); break;
		case ThingEnum::OrientedSelf:
			billboardSet->setBillboardType(BillboardType::BBT_ORIENTED_SELF); break;
		case ThingEnum::PerpendicularCommon:
			billboardSet->setBillboardType(BillboardType::BBT_PERPENDICULAR_COMMON); break;
		case ThingEnum::PerpendicularSelf:
			billboardSet->setBillboardType(BillboardType::BBT_PERPENDICULAR_SELF); break;
		case ThingEnum::Point:
		default: // To silence -Wswitch
			billboardSet->setBillboardType(BillboardType::BBT_POINT);
	}

	auto vecIt = block->getVectorTokens().find("upvector");
	if (vecIt != block->getVectorTokens().end())
		billboardSet->setCommonUpVector(vecIt->second);

	billboardSet->setSortingEnabled(block->getBoolProperty("Sort", true)); // sort transparent stuff
	billboardSet->setCommonDirection(block->getVectorProperty("Direction", Vector3::UNIT_Y)); // make them point the right way
	billboardSet->setBillboardRotationType(block->getBoolProperty("UseVertexRotation", false)
		? BillboardRotationType::BBR_VERTEX
		: BillboardRotationType::BBR_TEXCOORD);

	// origin
	ThingEnum originToken = block->getEnumProperty("Origin", ThingEnum::Center);
	switch (originToken)
	{
		case ThingEnum::TopLeft:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_TOP_LEFT); break;
		case ThingEnum::TopCenter:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_TOP_CENTER); break;
		case ThingEnum::TopRight:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_TOP_RIGHT); break;
		case ThingEnum::CenterLeft:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_CENTER_LEFT); break;
		case ThingEnum::CenterRight:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_CENTER_RIGHT); break;
		case ThingEnum::BottomLeft:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_BOTTOM_LEFT); break;
		case ThingEnum::BottomCenter:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_BOTTOM_CENTER); break;
		case ThingEnum::BottomRight:
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_BOTTOM_RIGHT); break;
		case ThingEnum::Center:
		default: // To silence -Wswitch
			billboardSet->setBillboardOrigin(BillboardOrigin::BBO_CENTER);
	}

	billboardSet->setRenderingDistance(block->getFloatProperty("RenderingDistance", 120));

	// texture coordinates
	auto rectQIt = block->getQuatTokens().find("texturecoords");
	if (rectQIt != block->getQuatTokens().end())
	{
		auto rect = new FloatRect(rectQIt->second.x, rectQIt->second.y, rectQIt->second.z, rectQIt->second.w);
		billboardSet->setTextureCoords(rect, 1);
	}

	// stacks/slices
	auto floatTokens = block->getFloatTokens();
	if (floatTokens.find("texturestacks")!=floatTokens.end() && floatTokens.find("textureslices")!=floatTokens.end())
		billboardSet->setTextureStacksAndSlices((uint8_t) block->getFloatProperty("TextureStacks", 1), (uint8_t) block->getFloatProperty("TextureSlices", 1));

	// and then go through each billboard block and create a billboard from it
	for (auto bbblock : block->getBillboardBlocks())
		createBillboard(bbblock);

	// setup attachment, if it needs one
	if (block->getBoolProperty("Attached", false))
	{
		string boneName = block->getStringProperty("AttachBone");
		int modelComponentID = (int) block->getFloatProperty("AttachComponentID");
		Quaternion offsetQuat = block->getQuatProperty("AttachOffsetOrientation", Quaternion::IDENTITY);
		Vector3 offsetVec = block->getVectorProperty("AttachOffsetPosition", Vector3::ZERO);
		lthing->getModelComponents()[modelComponentID]->getEntity()->attachObjectToBone(boneName, billboardSet, offsetQuat, offsetVec);
	}
	// if not, just attach it to the root node
	else
	{
		Vector3 pos = block->getVectorProperty("Position", Vector3::ZERO);

		SceneNode* attachNode;
		if (pos != Vector3::ZERO)
		{
			localNode = lthing->getRootNode()->createChildSceneNode(pos);
			attachNode = localNode;
		}
		else
			attachNode = lthing->getRootNode();

		attachNode->attachObject(billboardSet);
	}
}

void BillboardSetComponent::createBillboard(const PonykartParsers::BillboardBlock* const block)
{
	Billboard* bb = billboardSet->createBillboard(block->getVectorProperty("Position")); // make our billboard
	// set its color if it has one, and a rotation
	auto quatIt=block->getQuatTokens().find("colour");
	if (quatIt != block->getQuatTokens().end())
		bb->setColour(toColourValue(quatIt->second));
	bb->setRotation(Degree(block->getFloatProperty("Rotation", 0)));

	auto rectQIt = block->getQuatTokens().find("texturecoords");
	if (rectQIt != block->getQuatTokens().end())
		bb->setTexcoordRect(rectQIt->second.x, rectQIt->second.y, rectQIt->second.z, rectQIt->second.w);

	// It's best to not do this unless we really need to since it makes it less efficient
	auto fTokens = block->getFloatTokens();
	auto heightIt=fTokens.find("height"), widthIt=fTokens.find("width");
	if (heightIt!=fTokens.end() && widthIt!=fTokens.end())
		bb->setDimensions(widthIt->second, heightIt->second);
}
