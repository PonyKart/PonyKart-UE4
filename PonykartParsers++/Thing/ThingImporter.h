#ifndef THINGIMPORTER_H_INCLUDED
#define THINGIMPORTER_H_INCLUDED

#include <string>
#include <unordered_map>

namespace PonykartParsers
{
	namespace MuffinParser{class RuleInstance;}
	class ThingDefinition;

	class ThingImporter
	{
	public:
		ThingImporter();
		void parse(ThingDefinition* thingDef); ///< Parses right from the root
		ThingDefinition* parse(const std::string& nameOfThing);
	private:
		static void prepareFileList();
	private:
		MuffinParser::RuleInstance* root;
		static bool hasPreparedFileList;
		static std::unordered_map<std::string, std::string> fileList;
	};
} // PonykartParsers

#endif // THINGIMPORTER_H_INCLUDED
