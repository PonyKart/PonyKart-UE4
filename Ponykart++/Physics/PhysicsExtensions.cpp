#include "pch.h"
#include <BulletCollision/CollisionDispatch/btCollisionObject.h>
#include "Physics/PhysicsExtensions.h"
#include "Physics/CollisionObjectDataHolder.h"

using namespace Ponykart::Physics;
namespace Extensions
{
	/// Helper method for getting a collision group from a collision object
	/// @return The group of the collision object if it has one
	PonykartCollisionGroups getCollisionGroup(const btCollisionObject* const obj) 
	{
		// No RTTI, we assume that all user pointers of obj will be either CollisionObjectDataHolder* or nullptr_t
		CollisionObjectDataHolder* holder = static_cast<CollisionObjectDataHolder*>(obj->getUserPointer());
		if (holder != NULL)
			return holder->getCollisionGroup();
		else
			throw new std::string("This collision object does not have a CollisionObjectDataHolder associated with it!");
	}

	/// Helper method for getting a name out of a collision object.
	/// @return The name of the collision object if it has one
	std::string getName(btCollisionObject* obj) 
	{
		// No RTTI, we assume that all user pointers of obj will be either CollisionObjectDataHolder* or nullptr_t
		CollisionObjectDataHolder* holder = static_cast<CollisionObjectDataHolder*>(obj->getUserPointer());
		if (holder != NULL)
			return holder->getName();
		else
			throw new std::string("This collision object does not have a CollisionObjectDataHolder associated with it!");
	}
}