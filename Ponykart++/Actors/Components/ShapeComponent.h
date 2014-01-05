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
		PonykartParsers::ThingEnum getType() const;
		Ogre::Vector3 getDimensions() const;
		float getRadius() const;
		float getHeight() const;
		std::string getMesh() const;

		float getMinHeight() const;
		float getMaxHeight() const;
		int getWidth() const;
		int getLength() const;
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
