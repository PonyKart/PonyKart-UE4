#ifndef STATICGEOMETRYMANAGER_H_INCLUDED
#define STATICGEOMETRYMANAGER_H_INCLUDED

#include <vector>
#include <unordered_map>
#include <Ogre.h>

namespace PonykartParsers
{
	class ModelBlock;
	class ThingBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{
	class ModelComponent;

	class StaticGeometryManager
	{
	public:
		StaticGeometryManager();
		void add(ModelComponent* mc, PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ModelBlock* block, PonykartParsers::ThingDefinition* def); // Adds all of the geometry used by a model component to the static geometry. This is used by the ModelComponent.
	public:
		std::unordered_map<std::string, Ogre::StaticGeometry*> sgeoms;
		std::unordered_map<std::string, Ogre::Entity*> ents;
	private:
		const float _staticRegionSize = 4;
		const Ogre::Vector3 regionDimensions;
	};
} // Actors
} // Ponykart

#endif // STATICGEOMETRYMANAGER_H_INCLUDED
