#ifndef THINGIMPORTER_H_INCLUDED
#define THINGIMPORTER_H_INCLUDED

#include <string>
#include <unordered_map>

namespace PonykartParsers
{
	namespace ThingParser{class RuleInstance;}
	class ThingDefinition;

	class ThingImporter
	{
	public:
		ThingImporter();
		void parse(ThingDefinition* thingDef); ///< Parses right from the root
		ThingDefinition* parse(const std::string& nameOfThing);
		void parseProperty(TokenHolder* holder, ThingParser::RuleInstance* prop); ///< Takes a property and calls the appropriate parser method depending on its type
		void parseShape(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Shape blocks
		void parseModel(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Model blocks
		void parseRibbon(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Ribbon blocks
		void parseBillboardSet(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< BillboardSet blocks
		void parseBillboard(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Billboard blocks
		void parseSound(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Sound blocks
	private:
		static void prepareFileList();
	private:
		ThingParser::RuleInstance* root;
		static std::unordered_map<std::string, std::string> fileList;
		#if !DEBUG
			static bool hasPreparedFileList;
		#endif
	};
} // PonykartParsers

#endif // THINGIMPORTER_H_INCLUDED
