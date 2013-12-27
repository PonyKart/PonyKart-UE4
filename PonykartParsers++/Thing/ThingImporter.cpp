#include <fstream>
#include <OgreLogManager.h>
#include "Muffin/MuffinParser.h"
#include "Thing/ThingDefinition.h"
#include "Thing/ThingImporter.h"


using namespace std;
using namespace PonykartParsers;

bool ThingImporter::hasPreparedFileList(false);

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
