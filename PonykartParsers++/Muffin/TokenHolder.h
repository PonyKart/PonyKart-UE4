#ifndef TOKENHOLDER_H_INCLUDED
#define TOKENHOLDER_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifndef DLLEXPORT
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif
#endif

#include <unordered_map>
#include <string>
#include <OgreQuaternion.h>
#include <OgreVector3.h>

#include "Thing/ThingEnum.h"

namespace PonykartParsers
{
/// Since both the .thing and its blocks can all have properties, they all use this abstract class to give them maps and a few helpful methods
class TokenHolder
{
public:
	//virtual void SetUpmaps()=0; ///< Constructs the std::unordered_maps
	virtual void finish() {};
	// Getters
	DLLEXPORT ThingEnum getEnumProperty(const std::string& propertyName) const; ///< Gets an enum property from the maps.
	DLLEXPORT ThingEnum getEnumProperty(const std::string& propertyName, const ThingEnum& defaultValue) const; ///< Gets an enum property from the maps.
	DLLEXPORT std::string getStringProperty(const std::string& propertyName) const; ///< Gets a string property from the maps.
	DLLEXPORT std::string getStringProperty(const std::string& propertyName, const std::string& defaultValue) const; ///< Gets a string property from the maps.
	DLLEXPORT float getFloatProperty(const std::string& propertyName) const; ///< Gets a float property from the maps.
	DLLEXPORT float getFloatProperty(const std::string& propertyName, const float defaultValue) const; ///< Gets a float property from the maps.
	DLLEXPORT bool getBoolProperty(const std::string& propertyName) const; ///< Gets a boolean property from the maps.
	DLLEXPORT bool getBoolProperty(const std::string& propertyName, const bool defaultValue) const; ///< Gets a boolean property from the maps.
	DLLEXPORT Ogre::Vector3 getVectorProperty(const std::string& propertyName) const; ///< Gets a vector property from the maps.
	DLLEXPORT Ogre::Vector3 getVectorProperty(const std::string& propertyName, const Ogre::Vector3& defaultValue) const; ///< Gets a vector property from the maps.
	DLLEXPORT Ogre::Quaternion getQuatProperty(const std::string& propertyName) const; ///< Gets a quaternion property from the maps.
	DLLEXPORT Ogre::Quaternion getQuatProperty(const std::string& propertyName, const Ogre::Quaternion& defaultValue) const; ///< Gets a quaternion property from the maps.
	// Setters
	DLLEXPORT void setEnumProperty(const std::string& propertyName, ThingEnum value); ///< Sets an enum property in the maps.
	DLLEXPORT void setStringProperty(const std::string& propertyName, const std::string& value); ///< Sets a string property in the maps.
	DLLEXPORT void setFloatProperty(const std::string& propertyName, float value); ///< Sets a float property in the maps.
	DLLEXPORT void setBoolProperty(const std::string& propertyName, bool value); ///< Sets a bool property in the maps.
	DLLEXPORT void setVectorProperty(const std::string& propertyName, const Ogre::Vector3& value); ///< Sets a vector property in the maps.
	DLLEXPORT void setQuatProperty(const std::string& propertyName, const Ogre::Quaternion& value); ///< Sets a quaternion property in the maps.
	// Getters
	DLLEXPORT const std::unordered_map<std::string, ThingEnum>& getEnumTokens() const;
	DLLEXPORT 	const std::unordered_map<std::string, std::string>& getStringTokens() const;
	DLLEXPORT 	const std::unordered_map<std::string, float>& getFloatTokens() const;
	DLLEXPORT const std::unordered_map<std::string, bool>& getBoolTokens() const;
	DLLEXPORT const std::unordered_map<std::string, Ogre::Vector3>& getVectorTokens() const;
	DLLEXPORT const std::unordered_map<std::string, Ogre::Quaternion>& getQuatTokens() const;
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
