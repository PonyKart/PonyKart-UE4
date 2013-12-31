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
		std::string getNameFromProperty(ThingParser::RuleInstance* prop); ///< Gets a name out of a property. It's in its own method because it's used all the time.
		void parse(ThingDefinition* thingDef); ///< Parses right from the root
		ThingDefinition* parse(const std::string& nameOfThing);
		void parseProperty(TokenHolder* holder, ThingParser::RuleInstance* prop); ///< Takes a property and calls the appropriate parser method depending on its type
		std::string parseStringProperty(ThingParser::RuleInstance* prop); ///< Parse a string property
		bool parseBoolProperty(ThingParser::RuleInstance* prop); ///< Parse a bool property
		ThingEnum parseEnumProperty(ThingParser::RuleInstance* prop); ///< Parse an enum property. The value must exist in ThingEnum, but it is not case sensitive.
		bool parseFloatProperty(ThingParser::RuleInstance* prop); ///< Parse a float property
		Ogre::Vector3 parseVectorProperty(ThingParser::RuleInstance* prop); ///< Parse a vector property, i.e. a triplet of floats separated by commas
		Ogre::Quaternion parseQuatProperty(ThingParser::RuleInstance* prop); ///< Parse a quaternion property, i.e. a quartet of floats separated by commas.
		void parseShape(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Shape blocks
		void parseModel(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Model blocks
		void parseRibbon(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< Ribbon blocks
		void parseBillboardSet(ThingDefinition* thingDef, ThingParser::RuleInstance* block); ///< BillboardSet blocks
		void parseBillboard(BillboardSetBlock* bbSet, ThingParser::RuleInstance* block); ///< Billboard blocks
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
