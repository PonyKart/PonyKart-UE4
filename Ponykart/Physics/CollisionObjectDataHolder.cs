using BulletSharp;
using Ponykart.Actors;

namespace Ponykart.Physics {
	/// <summary>
	/// A little helper object for CollisionObjects that don't have an LThing associated with them. This holds some data for them instead.
	/// </summary>
	public class CollisionObjectDataHolder {
		/// <summary>
		/// The collision object this data holder is attached to.
		/// </summary>
		public CollisionObject Owner { get; private set; }
		/// <summary>
		/// The object's collision group
		/// </summary>
		public PonykartCollisionGroups CollisionGroup { get; private set; }
		/// <summary>
		/// The object's name
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// If this has a LThing associated with it, this points to it.
		/// </summary>
		public LThing Thing { get; private set; }
		/// <summary>
		/// Identificiation number. Is not the same as the Thing's ID!
		/// </summary>
		public int ID { get; private set; }


		/// <summary>
		/// Create a data holder without a LThing object.
		/// </summary>
		/// <param name="collisionGroup">The collision group of the object</param>
		/// <param name="name">The name of the object, excluding an ID</param>
		/// <param name="owner">The collision object this data holder is attached to</param>
		public CollisionObjectDataHolder(CollisionObject owner, PonykartCollisionGroups collisionGroup, string name) {
			this.Owner = owner;
			this.CollisionGroup = collisionGroup;
			this.Name = name;

			this.ID = IDs.Random;
		}

		/// <summary>
		/// Create a data holder using properties of an LThing object.
		/// </summary>
		public CollisionObjectDataHolder(LThing thing) : this(thing.Body, thing.CollisionGroup, thing.Name) {
			this.Thing = thing;
		}

		public override int GetHashCode() {
			return ID;
		}

		/// <summary>
		/// If this has a LThing associated with it and that LThing is a kart, this returns it as a kart. Otherwise it returns null.
		/// </summary>
		public Kart GetThingAsKart() {
			if (Thing != null && Thing is Kart)
				return Thing as Kart;
			else
				return null;
		}
	}
}
