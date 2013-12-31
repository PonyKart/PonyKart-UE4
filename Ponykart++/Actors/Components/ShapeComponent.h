#ifndef SHAPECOMPONENT_H_INCLUDED
#define SHAPECOMPONENT_H_INCLUDED

#include <Ogre.h>
#include "Thing/ThingEnum.h"

namespace PonykartParsers
{
	class ShapeBlock;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{
	class LThing;

	/// Represents a physics collision shape
	class ShapeComponent
	{
	public:
		ShapeComponent(LThing* lthing, PonykartParsers::ShapeBlock* block); ///< For physics
		// Getters
		PonykartParsers::ThingEnum getType();
		Ogre::Vector3 getDimensions();
		float getRadius();
		float getHeight();
		std::string getMesh();

		float getMinHeight();
		float getMaxHeight();
		int getWidth();
		int getLength();
	public:
		Ogre::Matrix4 transform;
	private:
		PonykartParsers::ThingEnum type;
		Ogre::Vector3 dimensions;
		float radius;
		float height;
		std::string mesh;

		float minHeight;
		float maxHeight;
		int width;
		int length;
	};
} // Actors
} // Ponykart

#endif // SHAPECOMPONENT_H_INCLUDED
