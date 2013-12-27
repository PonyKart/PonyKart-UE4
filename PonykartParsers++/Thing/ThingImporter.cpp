#include <fstream>
#include <OgreLogManager.h>
#include "Misc/direntSearch.h"
#include "Muffin/MuffinParser.h"
#include "Thing/ThingDefinition.h"
#include "Thing/ThingImporter.h"

using namespace std;
using namespace PonykartParsers;

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

	MuffinParser::Parser p;
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
