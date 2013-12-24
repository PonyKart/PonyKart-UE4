#ifndef COLLISIONOBJECTDATAHOLDER_H_INCLUDED
#define COLLISIONOBJECTDATAHOLDER_H_INCLUDED

#include <string>
#include "Physics/CollisionGroups.h"

class btCollisionObject;

namespace Ponykart
{
namespace Actors
{
	class LThing;
	class Kart;
} // Actors
namespace Physics
{
    // A little helper object for CollisionObjects that don't have an LThing associated with them. This holds some data for them instead.
    class CollisionObjectDataHolder
    {
	public:
		CollisionObjectDataHolder(btCollisionObject* owner, PonykartCollisionGroups collisionGroup, const std::string& name); // Create a data holder without a LThing object.
		CollisionObjectDataHolder(Actors::LThing* thing); // Create a data holder using properties of an LThing object.
		int getHashCode();
		Actors::Kart* getThingAsKart(); // If this has a LThing associated with it and that LThing is a kart, this returns it as a kart. Otherwise it returns null.
	private:
		std::string name; // The object's name
		btCollisionObject* owner; // The collision object this data holder is attached to.
		PonykartCollisionGroups collisionGroup; // The object's collision group
		Actors::LThing* thing; // If this has a LThing associated with it, this points to it.
		int id; // Identificiation number. Is not the same as the Thing's ID!
    };
} // Physics
} // Ponykart

#endif // COLLISIONOBJECTDATAHOLDER_H_INCLUDED
