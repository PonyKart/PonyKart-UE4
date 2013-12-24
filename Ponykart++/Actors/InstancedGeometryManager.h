#ifndef INSTANCEDGEOMETRYMANAGER_H_INCLUDED
#define INSTANCEDGEOMETRYMANAGER_H_INCLUDED

#include <unordered_map>
#include <string>
#include <vector>
#include <Ogre.h>

namespace PonykartParsers
{
	class ThingBlock;
	class ModelBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{
	class InstancedGeometryManager
	{
	private:
		struct Transform
		{
			Ogre::Vector3 position;
			Ogre::Quaternion orientation;
			Ogre::Vector3 scale;
		};

	public:
		InstancedGeometryManager();
		void add(ModelComponent* mc, PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ModelBlock* block, PonykartParsers::ThingDefinition* def);
	public:
		std::unordered_map<std::string, Ogre::InstancedGeometry*> igeoms; // uses map group + mesh name as a key
		std::unordered_map<std::string, std::vector<Transform>> transforms; // uses map group + mesh name as a key
		std::unordered_map<std::string, Ogre::Entity*> ents; // uses map group + mesh name as a key
	};
} // Actors
} // Ponykart

#endif // INSTANCEDGEOMETRYMANAGER_H_INCLUDED
