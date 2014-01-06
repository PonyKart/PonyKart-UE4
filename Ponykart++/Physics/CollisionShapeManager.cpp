#include <BulletCollision/CollisionShapes/btCompoundShape.h>
#include <BulletCollision/CollisionShapes/btBoxShape.h>
#include <BulletCollision/CollisionShapes/btCapsuleShape.h>
#include <BulletCollision/CollisionShapes/btConeShape.h>
#include <BulletCollision/CollisionShapes/btCylinderShape.h>
#include <BulletCollision/CollisionShapes/btHeightfieldTerrainShape.h>
#include <BulletCollision/CollisionShapes/btSphereShape.h>
#include "Actors/LThing.h"
#include "Actors/Components/ShapeComponent.h"
#include "Misc/direntSearch.h"
#include "Levels/LevelManager.h"
#include "Misc/direntSearch.h"
#include "Misc/bulletExtensions.h"
#include "Physics/CollisionShapeManager.h"
#include "Thing/ThingDefinition.h"

using namespace std;
using namespace Ogre;
using namespace Extensions;
using namespace PonykartParsers;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::Levels;
using namespace Ponykart::Physics;

CollisionShapeManager::CollisionShapeManager()
{
	LevelManager::onLevelLoad.push_back(function<void (LevelChangedEventArgs*)>(bind(&CollisionShapeManager::onLevelLoad,this,placeholders::_1)));
}

/// Gets a bullet file's full path
/// @param filename The filename, without any path or extension
/// @return The bullet file's full path if it was found, or an empty string if it wasn't
string CollisionShapeManager::getBulletFile(const string& filename) 
{
	string result;
	auto it = bulletFiles.find(getFilenameWithoutExtension(filename));
	if (it != end(bulletFiles))
		result = it->second;
	return result;
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

btCollisionShape* CollisionShapeManager::createShapeForComponent(ShapeComponent* component)
{
	switch (component->getType())
	{
	case ThingEnum::Box:
		return new btBoxShape(toBtVector3(component->getDimensions()));
	case ThingEnum::Cylinder:
		return new btCylinderShape(toBtVector3(component->getDimensions()));
	case ThingEnum::Cone:
	{
		btConeShape* cone = new btConeShape(component->getRadius(), component->getHeight());
		cone->setConeUpIndex(1);
		return cone;
	}
	case ThingEnum::Capsule:
		return new btCapsuleShape(component->getRadius(), component->getHeight());
	case ThingEnum::Sphere:
		return new btSphereShape(component->getRadius());
	case ThingEnum::Hull:
	{
		btCollisionShape* shape;

		string name = getFilenameWithoutExtension(component->getMesh());
		string bulletFilePath = getBulletFile(name);

		if (!bulletFilePath.empty())
			shape = importCollisionShape(name); // so it has a file
		else
		{
			/*var sceneMgr = LKernel.GetG<SceneManager>();
			// get our entity if we have one, create it if we don't
			Entity ent = sceneMgr.HasEntity(component.Mesh) ? sceneMgr.GetEntity(component.Mesh) : sceneMgr.CreateEntity(component.Mesh, component.Mesh);

			ConvexHullShape hull = OgreToBulletMesh.ConvertToHull(
				ent.GetMesh(),
				component.Transform.GetTrans(),
				component.Transform.ExtractQuaternion(),
				Vector3.UNIT_SCALE);
			shape = hull;

			// TODO: figure out how to deal with concave triangle mesh shapes since apparently they aren't being exported
			SerializeShape(shape, name);*/
			throw string("Your \"Mesh\" property did not point to an existing .bullet file!" + component->getMesh());
		}
		return shape;
	}
	case ThingEnum::Mesh:
	{
		btCollisionShape* shape;
		// example
		// physics/example.bullet
		string name = getFilenameWithoutExtension(component->getMesh());
		string bulletFilePath = getBulletFile(name);

		// right, so what we do is test to see if this shape has a .bullet file, and if it doesn't, create one
		if (!bulletFilePath.empty())
			shape = importCollisionShape(name); // so it has a file
		else
		{
			/*Launch.Log("[CollisionShapeManager] " + bulletFilePath + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");

			// it does not have a file, so we need to convert our ogre mesh
			var sceneMgr = LKernel.GetG<SceneManager>();
			Entity ent = sceneMgr.HasEntity(component.Mesh) ? sceneMgr.GetEntity(component.Mesh) : sceneMgr.CreateEntity(component.Mesh, component.Mesh);

			shape = new BvhTriangleMeshShape(
				OgreToBulletMesh.Convert(
					ent.GetMesh(),
					component.Transform.GetTrans(),
					component.Transform.ExtractQuaternion(),
					Vector3.UNIT_SCALE),
				true,
				true);

			(shape as BvhTriangleMeshShape).BuildOptimizedBvh();

			// and then export it as a .bullet file
			SerializeShape(shape, name);*/
			throw string("Your \"Mesh\" property did not point to an existing .bullet file!" + component->getMesh());
		}
		return shape;
	}
	/// NOTE: TODO: Implement this ASAP. A C# pixel is a 32 bit ARGB value
	/// EasyBMP can read a BMP and give the ARGB values, and it looks portable.
	/*
	case ThingEnum::Heightmap:
	{
		string filename = "media/" + component->getMesh();
		//FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
		//Bitmap bitmap = new Bitmap(filename);

		int width = 256, length = 256;

		unsigned char* terr = new unsigned char[width * length * 4];
		MemoryStream file = new MemoryStream(terr);
		BinaryWriter writer = new BinaryWriter(file);
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < length; j++) {
				writer.Write(bitmap.GetPixel((int) (((float) i / width) * bitmap.Width), (int) (((float) j / length) * bitmap.Height)).R / 255.0);
				//writer.Write(bitmap.GetPixel(i, j).R / 255f);
			}
		}
		writer.Flush();
		file.Position = 0;

		float heightScale = component->getMaxHeight() - component->getMinHeight() / 255.0;
		Vector3 scale = component->getDimensions();

		btHeightfieldTerrainShape* heightfield = new btHeightfieldTerrainShape(width, length, file, heightScale,
			component->getMinHeight(), component->getMaxHeight(), 1, PHY_ScalarType::PHY_FLOAT, false);

		//heightfield.SetUseDiamondSubdivision(true);
		//heightfield.LocalScaling = new Vector3(scale.x / width, scale.y, scale.z / length);

		//Matrix4 trans = new Matrix4();
		//trans.MakeTransform(new Vector3(-scale.x / 2f, scale.y / 2f, -scale.z / 2f), new Vector3(scale.x, 1, scale.z), Quaternion.IDENTITY);
		//component.Transform = trans;

		return heightfield;
	}
	*/
	default:
		throw string("ShapeComponent's Type was invalid!");
	}
}
