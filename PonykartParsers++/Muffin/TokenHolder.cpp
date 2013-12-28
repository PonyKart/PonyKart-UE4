#include "TokenHolder.h"

using namespace std;
using namespace Ogre;
using namespace PonykartParsers;

ThingEnum TokenHolder::getEnumProperty(const string& propertyName) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto it = enumTokens.find(propertyName);
	if (it != enumTokens.end())
		return it->second;
	else
		throw string("That property was not found in the .thing file!"+propertyName);
}

ThingEnum TokenHolder::getEnumProperty(const string& propertyName, const ThingEnum& defaultValue) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto it = enumTokens.find(propertyName);
	if (it != enumTokens.end())
		return it->second;
	else
		return defaultValue;
}

Vector3 TokenHolder::getVectorProperty(const string& propertyName) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto vecIt = vectorTokens.find(propertyName);
	if (vecIt != vectorTokens.end())
		return vecIt->second;
	else
		throw string("That property was not found in the .thing file!"+propertyName);
}

Vector3 TokenHolder::getVectorProperty(const string& propertyName, const Vector3& defaultValue) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto vecIt = vectorTokens.find(propertyName);
	if (vecIt != vectorTokens.end())
		return vecIt->second;
	else
		return defaultValue;
}

Quaternion TokenHolder::getQuatProperty(const string& propertyName) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto it = quatTokens.find(propertyName);
	if (it != quatTokens.end())
		return it->second;
	else
		throw string("That property was not found in the .thing file!"+propertyName);
}

Quaternion TokenHolder::getQuatProperty(const string& propertyName, const Quaternion& defaultValue) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto it = quatTokens.find(propertyName);
	if (it != quatTokens.end())
		return it->second;
	else
		return defaultValue;
}

string TokenHolder::getStringProperty(const string& propertyName) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto sIt=stringTokens.find(propertyName);
	if (sIt != stringTokens.end())
		return sIt->second;
	else
		throw string("That property was not found in the .thing file:"+propertyName);
}

string TokenHolder::getStringProperty(const string& propertyName, const string& defaultValue) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto sIt=stringTokens.find(propertyName);
	if (sIt != stringTokens.end())
		return sIt->second;
	else
		return defaultValue;
}

float TokenHolder::getFloatProperty(const string& propertyName) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto fIt=floatTokens.find(propertyName);
	if (fIt != floatTokens.end())
		return fIt->second;
	else
		throw string("That property was not found in the .thing file:"+propertyName);
}

float TokenHolder::getFloatProperty(const string& propertyName, const float defaultValue) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto fIt=floatTokens.find(propertyName);
	if (fIt != floatTokens.end())
		return fIt->second;
	else
		return defaultValue;
}

bool TokenHolder::getBoolProperty(const string& propertyName) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto it=boolTokens.find(propertyName);
	if (it != boolTokens.end())
		return it->second;
	else
		throw string("That property was not found in the .thing file:"+propertyName);
}

bool TokenHolder::getBoolProperty(const string& propertyName, const bool defaultValue) const
{
	// TODO: We should convert propertyName to lowercase before using it
	auto it=boolTokens.find(propertyName);
	if (it != boolTokens.end())
		return it->second;
	else
		return defaultValue;
}

const unordered_map<string, Ogre::Vector3>& TokenHolder::getVectorTokens() const
{
	return vectorTokens;
}

const unordered_map<string, Ogre::Quaternion>& TokenHolder::getQuatTokens() const
{
	return quatTokens;
}

const unordered_map<string, float>& TokenHolder::getFloatTokens() const
{
	return floatTokens;
}

const unordered_map<string, string>& TokenHolder::getStringTokens() const
{
	return stringTokens;
}
