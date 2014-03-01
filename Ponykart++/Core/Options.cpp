#include "pch.h"
#include <fstream>
#include <OgreConfigFile.h>
#include "Core/Options.h"
#include "Kernel/LKernelOgre.h"

using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;
using namespace Ogre;
using namespace std;

// Define the static members
unordered_map<string, string> Options::dict;
unordered_map<string, string> Options::defaults;
ModelDetailOption Options::modelDetail;
ShadowDetailOption Options::shadowDetail;

void Options::setupDictionaries()
{
	defaults=unordered_map<string,string>({
		{"FSAA","0"},
		{"Floating-point mode","Fastest"}, // Fastest or Accurate
		{"Full Screen","No"}, // Yes or No
		{"VSync","Yes"}, // Yes or No
//		{"VSync Interval","1"}, // 1, 2, 3, or 4
		{"Video Mode","1024 x 768 @ 32-bit colour"},
		{"sRGB Gamma Conversion","No"}, // Yes or No
		{"Music","No"}, // Yes or No
		{"Sounds","Yes"},
		{"Ribbons","Yes"},
		{"ModelDetail","Medium"}, // Low, Medium or High
		{"ShadowDetail","Some"}, // None, Some or Many
		{"ShadowDistance","40"},
		{"Twh","No"},
		{"Controller", "Keyboard"},
	});
	dict = defaults; // copy it into the regular dictionary
}

void Options::initialize()
{
	log("[Loading] Loading configuration...");

	setupDictionaries();

	static const char optionsPath[] = "media/config/ponykart.cfg";

	fstream file;
	file.open(optionsPath,ios::in);
	if (!file.is_open()) // create it if the file doesn't exist, and write out some defaults
	{
		file.open(optionsPath,ios::out);
		if (!file.is_open()) throw string("Cannot initialize media/config/ponykart.cfg");
		for (auto setting : defaults)
			file << setting.first << "=" << setting.second << endl;
		file.close();
		modelDetail = ModelDetailOption::Medium;
		shadowDetail = ShadowDetailOption::Some;
	}
	else // otherwise we just read from it
	{
		file.close(); // We're going to open it with cfile
		ConfigFile cfile;
		cfile.load(optionsPath, "=", true);
		auto sectionIterator = cfile.getSectionIterator();
		ConfigFile::SettingsMultiMap* settings=sectionIterator.getNext();
		for (auto curPair : *settings)
			dict[curPair.first] = curPair.second;
		// Convert the settings for ModelDetail and ShadowDetail (string) into enum values (int)
		string enumStr = dict["ModelDetail"];
		modelDetail = enumStr=="High"?ModelDetailOption::High: (enumStr=="Low"?ModelDetailOption::Low: (ModelDetailOption::Medium));
		enumStr = dict["ShadowDetail"];
		shadowDetail = enumStr=="Many"?ShadowDetailOption::Many: (enumStr=="None"?ShadowDetailOption::None: (ShadowDetailOption::Some));
	}

#ifdef DEBUG
	// since we sometimes add new options, we want to make sure the .ini file has all of them
	save();
#endif

	log("[Loading] Configuration loaded.");
}

void Options::save()
{
	static const char optionsPath[] = "media/config/ponykart.cfg";

	dict["ModelDetail"] = modelDetail==ModelDetailOption::High?"High": (modelDetail==ModelDetailOption::Low?"Low": ("Medium"));
	dict["ShadowDetail"] = shadowDetail==ShadowDetailOption::Many?"Many": (shadowDetail==ShadowDetailOption::None?"None": ("Some"));

	fstream file;
	file.open(optionsPath,ios::out);
	if (!file.is_open()) throw string("Cannot save media/config/ponykart.cfg");

	for (auto setting : dict)
		file << setting.first << "=" << setting.second << endl;
}

string Options::get(const string keyName)
{
	// TODO: The string comparisons should be case-insensitive, like in the C# code
	if (keyName == "ModelDetail")
		throw new string("Options::getBool: Use the Options.ModelDetail enum instead of this method!");
	else if (keyName == "ShadowDetail")
		throw new string("Use the Options.ShadowDetail enum instead of this method!");

	return dict[keyName];
}

bool Options::getBool(string keyName)
{
	string value = dict[keyName];
	if (value == "Yes")
		return true;
	else if (value == "No")
		return false;
	else
		throw string("Options::getBool: The key '"+keyName+"' does not represent a boolean option!");
}
