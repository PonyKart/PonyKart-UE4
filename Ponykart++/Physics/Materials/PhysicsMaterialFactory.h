#ifndef PHYSICSMATERIALFACTORY_H_INCLUDED
#define PHYSICSMATERIALFACTORY_H_INCLUDED

#include <unordered_map>
#include <string>
#include <BulletDynamics/Dynamics/btRigidBody.h>
#include "Kernel/LKernelObject.h"

namespace Ponykart
{
namespace Physics
{
	struct PhysicsMaterial;

	class PhysicsMaterialFactory : public LKernel::LKernelObject
	{
	public:
		PhysicsMaterialFactory();
		PhysicsMaterial* getMaterial(const std::string& materialName);
		void readMaterialsFromFiles(); ///< Go through our media/physicsmaterials/ directory and find all of the material definitions we have, then make objects out of them and add them to our map
		void applyMaterial(btRigidBody* body, const std::string& material); ///< Only applies friction and bounciness. Use a RigidBodyConstructionInfo if you want to set the damping.
		void applyMaterial(btRigidBody::btRigidBodyConstructionInfo* info, const std::string& material); ///< Applies friction, bounciness, angular damping, and linear damping
	public:
		std::unordered_map<std::string, PhysicsMaterial*> materials;
		PhysicsMaterial* defaultMat;
	};
} // Physics
} // Ponykart

#endif // PHYSICSMATERIALFACTORY_H_INCLUDED
