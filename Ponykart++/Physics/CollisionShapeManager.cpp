#include <BulletCollision/CollisionShapes/btCompoundShape.h>
#include "Actors/LThing.h"
#include "Actors/Components/ShapeComponent.h"
#include "Levels/LevelManager.h"
#include "Misc/direntSearch.h"
#include "Misc/bulletExtensions.h"
#include "Physics/CollisionShapeManager.h"
#include "Thing/ThingDefinition.h"

using namespace std;
using namespace Ogre;
using namespace PonykartParsers;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::Levels;
using namespace Ponykart::Physics;

CollisionShapeManager::CollisionShapeManager()
{
	LevelManager::onLevelLoad.push_back(function<void (LevelChangedEventArgs*)>(bind(&CollisionShapeManager::onLevelLoad,this,placeholders::_1)));
}

void CollisionShapeManager::onLevelLoad(LevelChangedEventArgs* eventArgs)
{
#if !DEBUG
	ResourceGroupManager& resGM = ResourceGroupManager::getSingleton();
	for (string group : resGM.getResourceGroups())
	{
		if (!resGM.isResourceGroupInitialised(group) || group == "Bootstrap")
				continue;


		auto resourceLocations = *(resGM.listResourceLocations(group));
		for (string loc : resourceLocations)
		{
			auto scripts = direntSearch(loc, ".bullet");

			for (string file : scripts)
				bulletFiles[getFilenameWithoutExtension(file)] = file;
		}
	}
#else
	std::vector<string> files = direntSearchRec("media/", ".bullet");
	for (string file : files)
		bulletFiles[getFilenameWithoutExtension(file)] = file;
#endif
}

btCollisionShape* CollisionShapeManager::createAndRegisterShape(LThing* thing, ThingDefinition* def)
{
	btCollisionShape* shape;
	auto shapeIt=shapes.find(thing->getName());
	if (shapeIt == shapes.end())
	{
		// create the shape
		bool forceCompound = def->getBoolProperty("forcecompound", false);

		// if we only have one shape we don't have to do as much
		if (thing->getShapeComponents().size() == 1)
		{
			// force us to use a compound shape?
			if (forceCompound)
			{
				btCompoundShape* comp = new btCompoundShape();
				comp->addChildShape(toBtTransform(thing->getShapeComponents().at(0)->transform), createShapeForComponent(thing->getShapeComponents().at(0)));

				shape = comp;
			}
			// one component, no compound is the easiest
			else
				shape = createShapeForComponent(thing->getShapeComponents().at(0));
		}
		// otherwise, make all of our shapes and stick them in a compound shape
		else
		{
			shape = shapeIt->second;
			btCompoundShape* comp = new btCompoundShape();

			for (ShapeComponent* component : thing->getShapeComponents())
				comp->addChildShape(toBtTransform(component->transform), createShapeForComponent(component));

			shape = comp;
		}

		// then put the shape in our dictionary
		shapes.insert(pair<string, btCollisionShape*>(thing->getName(), shape));
	}
	return shape;
}
