#include <fstream>
#include <OgreLogManager.h>
#include <OgreResourceGroupManager.h>
#include "Misc/direntSearch.h"
#include "Thing/ThingDefinition.h"
#include "Thing/ThingImporter.h"
#include "Thing/ThingParser.h"

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
    fileContents.resize(fileStream.tellg());
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
