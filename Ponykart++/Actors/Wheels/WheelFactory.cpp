#include "Actors/Wheels/WheelFactory.h"
#include "Misc/direntSearch.h"

using namespace std;
using namespace Ogre;
using namespace Ponykart::Actors;

WheelFactory::WheelFactory()
{
	readWheelsFromFiles();
}

void WheelFactory::readWheelsFromFiles()
{
	wheels.clear(); // since we can run this whenever (like when we're tweaking files), we want to clear our dictionary first

	std::vector<string> files = direntSearch("media/vehicles/", ".wheel"); // Recursively get all filenames matching the extension

	for (string filename : files)
	{
		ConfigFile cfile;
		cfile.load(filename, "=", true);

		// each .wheel file can have multiple [sections]. We'll use each section name as the wheel name
		ConfigFile::SectionIterator sectionIterator = cfile.getSectionIterator();
		while(sectionIterator.hasMoreElements())
		{
			string wheelname = sectionIterator.peekNextKey();
			unordered_map<string,float> wheeldict;
			for (auto curPair : *(sectionIterator.getNext())) // go over every property in the file and add it to the dictionary, parsing it as a float
				wheeldict.insert(pair<string,float>(curPair.first, atof(curPair.second.c_str())));

			wheels[wheelname] = wheeldict;
		}
	}
}

Wheel* WheelFactory::createWheel(string wheelName, WheelID ID, Kart* owner, Vector3 position, string meshName)
{
	unordered_map<string, float> dict = wheels[wheelName];

	Wheel* wheel = new Wheel(owner, position, ID, dict, meshName);

	return wheel;
}
