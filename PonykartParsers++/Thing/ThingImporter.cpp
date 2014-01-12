#include <fstream>
#include <OgreLogManager.h>
#include <OgreResourceGroupManager.h>
#include "Misc/direntSearch.h"
#include "Thing/ThingDefinition.h"
#include "Thing/ThingImporter.h"
#include "Thing/ThingParser.h"
#include "Thing/Blocks/ShapeBlock.h"
#include "Thing/Blocks/ModelBlock.h"
#include "Thing/Blocks/RibbonBlock.h"
#include "Thing/Blocks/BillboardSetBlock.h"
#include "Thing/Blocks/SoundBlock.h"

using namespace std;
using namespace Extensions;
using namespace PonykartParsers;
using namespace PonykartParsers::ThingParser;

std::unordered_map<std::string, std::string> ThingImporter::fileList;
#if !DEBUG
	bool ThingImporter::hasPreparedFileList(false);
#endif

ThingImporter::ThingImporter()
{
}

ThingDefinition* ThingImporter::parse(const std::string& nameOfThing)
{
	prepareFileList();

	// make the file path
	// this just searches for "media/things/foo.thing"
	string filePath;
	auto filePathIt = fileList.find(nameOfThing);
	// this searches subfolders for .things
	if (filePathIt == fileList.end())
		throw new string(nameOfThing + ".thing does not exist!");
	else
		filePath = filePathIt->second;

	Ogre::LogManager::getSingleton().logMessage("[ThingImporter] Importing and parsing thing: " + filePath);

	// read stuff
	ifstream fileStream(filePath, ios::in);
	if (!fileStream)
		throw string("ThingImporter: Can't open "+filePath);
	
	string fileContents;
    fileStream.seekg(0, std::ios::end);
    fileContents.resize((size_t)fileStream.tellg()); // We're assuming that the .thing is smaller than 4GB
    fileStream.seekg(0, std::ios::beg);
    fileStream.read(&fileContents[0], fileContents.size());
    fileStream.close();

	ThingParser::Parser p;
	root = p.parse(fileContents);

	ThingDefinition* thingDef = new ThingDefinition(nameOfThing);

	parse(thingDef);

	thingDef->finish();

	return thingDef;
}

string ThingImporter::getNameFromProperty(RuleInstance* prop)
{
	RuleInstance* nameRule = (RuleInstance*)prop->children[0];
	Token* nameTok = (Token*)nameRule->children[0];
	return nameTok->image;
}

void ThingImporter::prepareFileList()
{
#if !DEBUG
	if (!hasPreparedFileList)
	{
#endif
		Ogre::ResourceGroupManager& rgm = Ogre::ResourceGroupManager::getSingleton();
		for (string group : rgm.getResourceGroups())
		{
			if (!rgm.isResourceGroupInitialised(group) || group == "Bootstrap")
				continue;

			auto resourceLocations = *(rgm.listResourceLocations(group));

			for (string loc : resourceLocations)
			{
				auto scripts = direntSearch(loc, ".thing");

				for (string file : scripts)
					fileList[getFilenameWithoutExtension(file)] = file;
			}
		}
#if !DEBUG
		hasPreparedFileList = true;
	}
#endif
}

void ThingImporter::parse(ThingDefinition* thingDef)
{
	for (unsigned a = 0; a < root->children.size(); a++)
	{
		Node* prop = root->children[a];
		switch (prop->type)
		{
			case NodeType::Rule_Property:
				parseProperty(thingDef, (RuleInstance*)((RuleInstance*)prop)->children[0]);
				break;
			case NodeType::Rule_Shape:
				parseShape(thingDef, (RuleInstance*)prop);
				break;
			case NodeType::Rule_Model:
				parseModel(thingDef, (RuleInstance*)prop);
				break;
			case NodeType::Rule_Ribbon:
				parseRibbon(thingDef, (RuleInstance*)prop);
				break;
			case NodeType::Rule_BillboardSet:
				parseBillboardSet(thingDef, (RuleInstance*)prop);
				break;
			case NodeType::Rule_Sound:
				parseSound(thingDef, (RuleInstance*)prop);
				break;
			default:
				break;
		}
	}
}

void ThingImporter::parseProperty(TokenHolder* holder, ThingParser::RuleInstance* prop)
{
	string propName = getNameFromProperty(prop);
	transform(propName.begin(), propName.end(), propName.begin(), ::tolower);
	switch (prop->type)
	{
		case NodeType::Rule_StringProperty:
			holder->setStringProperty(propName, parseStringProperty(prop));
			break;
		case NodeType::Rule_BoolProperty:
			holder->setBoolProperty(propName, parseBoolProperty(prop));
			break;
		case NodeType::Rule_EnumProperty:
			holder->setEnumProperty(propName, parseEnumProperty(prop));
			break;
		case NodeType::Rule_NumericProperty:
			holder->setFloatProperty(propName, parseFloatProperty(prop));
			break;
		case NodeType::Rule_Vec3Property:
			holder->setVectorProperty(propName, parseVectorProperty(prop));
			break;
		case NodeType::Rule_QuatProperty:
			holder->setQuatProperty(propName, parseQuatProperty(prop));
			break;
		default:
			break;
	}
}

void ThingImporter::parseShape(ThingDefinition* thingDef, RuleInstance* block)
{
	ShapeBlock* shapeBlock = new ShapeBlock(thingDef);

	for (unsigned a = 2; a < block->children.size() - 1; a++)
	{
		RuleInstance* rule = (RuleInstance*)block->children[a];
		if (rule->type == NodeType::Rule_Property)
			parseProperty(shapeBlock, (RuleInstance*)rule->children[0]);
	}

	thingDef->addShapeBlock(shapeBlock);
}

void ThingImporter::parseModel(ThingDefinition* thingDef, RuleInstance* block)
{
	ModelBlock* modelBlock = new ModelBlock(thingDef);

	for (unsigned a = 2; a < block->children.size() - 1; a++)
	{
		RuleInstance* rule = (RuleInstance*)block->children[a];
		if (rule->type == NodeType::Rule_Property)
			parseProperty(modelBlock, (RuleInstance*)rule->children[0]);
	}

	thingDef->addModelBlock(modelBlock);
}

void ThingImporter::parseRibbon(ThingDefinition* thingDef, RuleInstance* block)
{
	RibbonBlock* ribbonBlock = new RibbonBlock(thingDef);

	for (unsigned a = 2; a < block->children.size() - 1; a++)
	{
		RuleInstance* rule = (RuleInstance*)block->children[a];
		if (rule->type == NodeType::Rule_Property)
			parseProperty(ribbonBlock, (RuleInstance*)rule->children[0]);
	}

	thingDef->addRibbonBlock(ribbonBlock);
}

void ThingImporter::parseBillboardSet(ThingDefinition* thingDef, RuleInstance* block)
{
	BillboardSetBlock* billboardSetBlock = new BillboardSetBlock(thingDef);

	for (unsigned a = 2; a < block->children.size() - 1; a++)
	{
		RuleInstance* rule = (RuleInstance*)block->children[a];
		if (rule->type == NodeType::Rule_Property)
			parseProperty(billboardSetBlock, (RuleInstance*)rule->children[0]);
		else if (rule->type == NodeType::Rule_Billboard)
			parseBillboard(billboardSetBlock, rule);
	}

	thingDef->addBillboardSetBlock(billboardSetBlock);
}

/** @param bbSet We pass the bbset block, not the whole thingDefinition **/
void ThingImporter::parseBillboard(BillboardSetBlock* bbSet, RuleInstance* block)
{
	BillboardBlock* billboardBlock = new BillboardBlock(bbSet);

	for (unsigned a = 2; a < block->children.size() - 1; a++)
	{
		RuleInstance* rule = (RuleInstance*)block->children[a];
		if (rule->type == NodeType::Rule_Property)
			parseProperty(billboardBlock, (RuleInstance*)rule->children[0]);
	}

	bbSet->addBillboardBlock(billboardBlock);
}

void ThingImporter::parseSound(ThingDefinition* thingDef, RuleInstance* block)
{
	SoundBlock* soundBlock = new SoundBlock(thingDef);

	for (unsigned a = 2; a < block->children.size() - 1; a++)
	{
		RuleInstance* rule = (RuleInstance*)block->children[a];
		if (rule->type == NodeType::Rule_Property)
			parseProperty(soundBlock, (RuleInstance*)rule->children[0]);
	}

	thingDef->addSoundBlock(soundBlock);
}

string ThingImporter::parseStringProperty(RuleInstance* prop)
{
	Token* valTok = (Token*)prop->children[2];
	string val = valTok->image;

	// need to substring because we get "\"foo\"" from the file, and we don't want extra quotes
	return val.substr(1, val.size() - 2);
}

bool ThingImporter::parseBoolProperty(RuleInstance* prop)
{
	Token* valTok = (Token*)prop->children[2];
	if (valTok->type == NodeType::Tok_KeyTrue)
		return true;
	else if (valTok->type == NodeType::Tok_KeyFalse)
		return false;
	else
		throw string("ThingImporter::parseBoolProperty: Boolean property is not true or false! (How did we even get to this point?)");
}

ThingEnum ThingImporter::parseEnumProperty(RuleInstance* prop)
{
	RuleInstance* valRule = (RuleInstance*)prop->children[2];
	Token* valTok = (Token*)valRule->children[0];

	int result=-1;
	for (unsigned i = 0; i < ThingEnumMap.size(); i++) // Parse valTok->image into a ThingEnum
	{
		string s1{ThingEnumMap[i]};
		string s2{valTok->image};
		transform(s1.begin(), s1.end(), s1.begin(), ::tolower);
		transform(s2.begin(), s2.end(), s2.begin(), ::tolower);
		if (s1 == s2)
			result = i;
	}

	if (result != -1)
		return ThingEnum(result);
	else
		throw string("ThingImporter::parseEnumProperty: Unable to parse Enum property!");
}

float ThingImporter::parseFloatProperty(RuleInstance* prop)
{
	Token* tok = (Token*)prop->children[2];
	return (float)atof(tok->image.c_str());
}

Ogre::Vector3 ThingImporter::parseVectorProperty(RuleInstance* prop)
{
	Token* tok1 = (Token*)prop->children[2];
	Token* tok2 = (Token*)prop->children[4];
	Token* tok3 = (Token*)prop->children[6];

	float x = (float)atof(tok1->image.c_str());
	float y = (float)atof(tok2->image.c_str());
	float z = (float)atof(tok3->image.c_str());

	return Ogre::Vector3(x, y, z);
}

/** Note that the .thing format uses xyzw but ogre uses wxyz! **/
Ogre::Quaternion ThingImporter::parseQuatProperty(ThingParser::RuleInstance* prop)
{
	Token* tok1 = (Token*)prop->children[2];
	Token* tok2 = (Token*)prop->children[4];
	Token* tok3 = (Token*)prop->children[6];
	Token* tok4 = (Token*)prop->children[8];

	float x = (float)atof(tok1->image.c_str());
	float y = (float)atof(tok2->image.c_str());
	float z = (float)atof(tok3->image.c_str());
	float w = (float)atof(tok4->image.c_str());

	return Ogre::Quaternion(w, x, y, z);
}
