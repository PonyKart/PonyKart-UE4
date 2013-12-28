#ifndef TOKENHOLDER_H_INCLUDED
#define TOKENHOLDER_H_INCLUDED

#include <unordered_map>
#include <string>
#include <OgreQuaternion.h>
#include <OgreVector3.h>

#include "Thing/ThingEnum.h"

namespace PonykartParsers
{
// Since both the .thing and its blocks can all have properties, they all use this abstract class to give them maps and a few helpful methods
class TokenHolder // TODO: Implement properly
{
public:
	//virtual void SetUpmaps()=0; // Constructs the std::unordered_maps
	virtual void finish()=0;
	ThingEnum getEnumProperty(const std::string& propertyName) const; // Gets an enum property from the maps.
	ThingEnum getEnumProperty(const std::string& propertyName, const ThingEnum& defaultValue) const; // Gets an enum property from the maps.
	std::string getStringProperty(const std::string& propertyName) const; // Gets a string property from the maps.
	std::string getStringProperty(const std::string& propertyName, const std::string& defaultValue) const; // Gets a string property from the maps.
	float getFloatProperty(const std::string& propertyName) const; // Gets a float property from the maps.
	float getFloatProperty(const std::string& propertyName, const float defaultValue) const; // Gets a float property from the maps.
	bool getBoolProperty(const std::string& propertyName) const; // Gets a boolean property from the maps.
	bool getBoolProperty(const std::string& propertyName, const bool defaultValue) const; // Gets a boolean property from the maps.
	Ogre::Vector3 getVectorProperty(const std::string& propertyName) const; // Gets a vector property from the maps.
	Ogre::Vector3 getVectorProperty(const std::string& propertyName, const Ogre::Vector3& defaultValue) const; // Gets a vector property from the maps.
	Ogre::Quaternion getQuatProperty(const std::string& propertyName) const; // Gets a quaternion property from the maps.
	Ogre::Quaternion getQuatProperty(const std::string& propertyName, const Ogre::Quaternion& defaultValue) const; // Gets a quaternion property from the maps.
	// Getters
	const std::unordered_map<std::string, ThingEnum>& getEnumTokens() const;
	const std::unordered_map<std::string, std::string>& getStringTokens() const;
	const std::unordered_map<std::string, float>& getFloatTokens() const;
	const std::unordered_map<std::string, bool>& getBoolTokens() const;
	const std::unordered_map<std::string, Ogre::Vector3>& getVectorTokens() const;
	const std::unordered_map<std::string, Ogre::Quaternion>& getQuatTokens() const;

protected: // Set-protected public members
	std::unordered_map<std::string, ThingEnum> enumTokens;
	std::unordered_map<std::string, std::string> stringTokens;
	std::unordered_map<std::string, float> floatTokens;
	std::unordered_map<std::string, bool> boolTokens;
	std::unordered_map<std::string, Ogre::Vector3> vectorTokens;
	std::unordered_map<std::string, Ogre::Quaternion> quatTokens;

};
} // PonykartParsers

#endif // TOKENHOLDER_H_INCLUDED
