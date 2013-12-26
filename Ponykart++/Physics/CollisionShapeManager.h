#ifndef COLLISIONSHAPEMANAGER_H_INCLUDED
#define COLLISIONSHAPEMANAGER_H_INCLUDED

#include <unordered_map>
#include <string>

class btCollisionShape;

namespace Ogre
{
	class Entity;
	class SceneNode;
} // Ogre

namespace PonykartParsers{class ThingDefinition;}

namespace Ponykart
{
namespace Actors
{
	class LThing;
	class ShapeComponent;
}
namespace Levels{class LevelChangedEventArgs;}

namespace Physics
{
	class CollisionShapeManager // TODO: Implement CollisionShapeManager
	{
	public:
		CollisionShapeManager();
		btCollisionShape* getShape(const std::string& name); // Gets a collision shape.
		btCollisionShape* getShapeFromFile(const std::string& filename, Ogre::Entity* ent, Ogre::SceneNode* node); // Gets a collision shape from a .bullet file.
		std::string getBulletFile(const std::string& filename); // Gets a bullet file's full path
		btCollisionShape* createAndRegisterShape(Actors::LThing* thing, PonykartParsers::ThingDefinition* def); // Creates a CollisionShape from the ShapeComponents of the given thing. If the shape already exists, we'll just return that instead.
		void registerShape(const std::string& name, btCollisionShape* shape); // If you've already created a shape, this registers it
		bool tryGetShape(const std::string& name, btCollisionShape* outShape); // Tries to get a shape from the shape map
		btCollisionShape* importCollisionShape(const std::string& bulletfile); // Imports a collision shape from a .bullet file.
		void serializeShape(btCollisionShape* shape, const std::string& name); // Serializes a collision shape and exports a .bullet file.
	private:
		void onLevelLoad(Levels::LevelChangedEventArgs* eventArgs); // Enumerates through our resource group paths and finds all of the .bullet files
		btCollisionShape* createShapeForComponent(Actors::ShapeComponent* component); // Creates a collision shape for a shape component
	public:
		std::unordered_map<std::string, btCollisionShape*> shapes;
		std::unordered_map<std::string, std::string> bulletFiles;
	};
} // Physics
} // Ponykart

#endif // COLLISIONSHAPEMANAGER_H_INCLUDED
