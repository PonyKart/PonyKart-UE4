#include "pch.h"
#include <vector>
#include <OgreConfigFile.h>
#include "Kernel/LKernelOgre.h"
#include "Misc/direntSearch.h"
#include "Physics/Materials/PhysicsMaterial.h"
#include "Physics/Materials/PhysicsMaterialFactory.h"

using namespace std;
using namespace Extensions;
using namespace Ponykart::Physics;

PhysicsMaterialFactory::PhysicsMaterialFactory()
{
	defaultMat = new PhysicsMaterial();

	readMaterialsFromFiles();
}

void PhysicsMaterialFactory::readMaterialsFromFiles()
{
	// since we can run this whenever (like when we're tweaking files), we want to clear this first
	materials.clear();

	// get all of the filenames of the files in media/physicsmaterials
	std::string path = "media/physicsmaterials/";
	vector<string> files = direntSearch(path, ".physmat");

	for (string filename : files)
	{
		// rev up those files
		Ogre::ConfigFile cfile;
		cfile.load(path+filename, "=", true);

		auto sectionIterator = cfile.getSectionIterator();
		while (sectionIterator.hasMoreElements())
		{
			string matname = sectionIterator.peekNextKey();
			sectionIterator.moveNext();
			//if (matname.empty()) continue;

			ostringstream ssF,ssB,ssAD,ssLD;
			ssF << PhysicsMaterial::DEFAULT_FRICTION;
			ssB << PhysicsMaterial::DEFAULT_BOUNCINESS;
			ssAD << PhysicsMaterial::DEFAULT_ANGULAR_DAMPING;
			ssLD << PhysicsMaterial::DEFAULT_LINEAR_DAMPING;

			static const string strF = "Friction";
			static const string strB = "Bounciness";
			static const string strAD = "AngularDamping";
			static const string strLD = "LinearDamping";
			string strSsF = ssF.str();
			string strSsB = ssB.str();
			string strSsAD = ssAD.str();
			string strSsLD = ssLD.str();
			string a = cfile.getSetting(strF, matname, strSsF);
			string b = cfile.getSetting(strB, matname, strSsB);
			string c = cfile.getSetting(strAD, matname, strSsAD);
			string d = cfile.getSetting(strLD, matname, strSsLD);

			PhysicsMaterial *mat = new PhysicsMaterial(
				(float)atof(a.c_str()),
				(float)atof(b.c_str()),
				(float)atof(c.c_str()),
				(float)atof(d.c_str())
			);

			materials[matname] = mat;
		}
	}
}

/// Gets a material from the map, or a default value if not found
PhysicsMaterial* PhysicsMaterialFactory::getMaterial(const string& materialName)
{
	auto matIt = materials.find(materialName);
	if (matIt != materials.end())
		return matIt->second;
	else if (materialName == "Default")
		return defaultMat;
	else
	{
		LKernel::log("[PhysicsMaterialFactory] Material \"" + materialName + "\" did not exist! Applying default...");
		return defaultMat;
	}
}

/// Only applies friction and bounciness. Use a RigidBodyConstructionInfo if you want to set the damping.
void PhysicsMaterialFactory::applyMaterial(btRigidBody* body, const string& material)
{
	PhysicsMaterial* mat = getMaterial(material);

	body->setFriction(mat->friction);
	body->setRestitution(mat->bounciness);
}

/// Applies friction, bounciness, angular damping, and linear damping
void PhysicsMaterialFactory::applyMaterial(btRigidBody::btRigidBodyConstructionInfo* info, const string& material)
{
	PhysicsMaterial* mat = getMaterial(material);

	info->m_friction = mat->friction;
	info->m_restitution = mat->bounciness;
	info->m_angularDamping = mat->angularDamping;
	info->m_linearDamping = mat->linearDamping;
}
