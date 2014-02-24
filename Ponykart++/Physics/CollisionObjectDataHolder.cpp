#include "Actors/LThing.h"
#include "Core/IDs.h"
#include "Physics/CollisionObjectDataHolder.h"

using namespace Ponykart::Actors;
using namespace Ponykart::Physics;

CollisionObjectDataHolder::CollisionObjectDataHolder(LThing* Thing)
 : thing(Thing)
{
	owner = Thing->getBody();
	collisionGroup = *thing->getCollisionGroup();
	name = thing->getName();
}

/// Create a data holder without a LThing object.
/** @param CollisionGroup The collision group of the object
/// @param Name The name of the object, excluding an ID
/// @param Owner The collision object this data holder is attached to */
CollisionObjectDataHolder::CollisionObjectDataHolder(btCollisionObject* Owner, PonykartCollisionGroups CollisionGroup, const std::string& Name)
 : owner(Owner), collisionGroup(CollisionGroup), name(Name)
{
	id = IDs::random();
}
