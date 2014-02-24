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
    /// Helper object that holds some data for CollisionObjects that don't have an LThing associated with them.
    class CollisionObjectDataHolder
    {
	public:
		CollisionObjectDataHolder(Actors::LThing* Thing); ///< Create a data holder using properties of an LThing object.
		CollisionObjectDataHolder(btCollisionObject* Owner, PonykartCollisionGroups CollisionGroup, const std::string& Name);
		int getHashCode();
		Actors::Kart* getThingAsKart(); ///< If this has a LThing associated with it and that LThing is a kart, this returns it as a kart. Otherwise it returns null.
	private:
		btCollisionObject* owner; ///< The collision object this data holder is attached to.
		PonykartCollisionGroups collisionGroup; ///< The object's collision group
		std::string name; ///< The object's name
		Actors::LThing* thing; ///< If this has a LThing associated with it, this points to it.
		int id; ///< Identificiation number. Is not the same as the Thing's ID!
    };
} // Physics
} // Ponykart

#endif // COLLISIONOBJECTDATAHOLDER_H_INCLUDED
