#ifndef PHYSICSEXTENSIONS_H_INCLUDED
#define PHYSICSEXTENSIONS_H_INCLUDED

#include <string>
#include "Physics/CollisionGroups.h"

class btCollisionObject;
namespace Extensions
{
	Ponykart::Physics::PonykartCollisionGroups getCollisionGroup(const btCollisionObject* const obj);
	std::string getName(btCollisionObject* obj);
}

#endif // PHYSICSEXTENSIONS_H_INCLUDED