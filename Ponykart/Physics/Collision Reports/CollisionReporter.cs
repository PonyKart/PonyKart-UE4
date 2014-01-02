using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.Physics {

	/// <summary>
	/// Our delegate for collision reports
	/// </summary>
	public delegate void CollisionReportEvent(CollisionReportInfo info);

	/// <summary>
	/// Our class for handling all collision reports, firing events when physics objects collide.
	/// 
	/// I'm sure there's a better way of dealing with these to speed it up, but hey how it is now is pretty efficient and isn't very high up on the
	/// profiling thing yet, so it's good enough for now.
	/// 
	/// Remember that things you want to be collided with need to have their CollisionFlags.CustomMaterialCallback flag set!
	/// </summary>
	public class CollisionReporter {
		/// <summary>
		/// our 2D array of contact report delegates
		/// </summary>
		private CollisionReportEvent[,] reporters;
		/// <summary>
		/// Our dictionary of collision objects, with a set containing the objects it collided with last frame.
		/// Why a hash set? They prevent having multiple identical objects, but also don't throw an error if they already contain it.
		/// They just silently ignore it (though .Add does return whether the adding was successful or not)
		/// </summary>
		private IDictionary<CollisionObject, HashSet<CollisionObject>> CurrentlyCollidingWith;
		/// <summary>
		/// This dictionary contains objects that collided *this* frame.
		/// </summary>
		private IDictionary<CollisionObject, HashSet<CollisionObject>> NewCollidingWith;


		#region -------------------- HEY THIS IS IMPORTANT ---------------------
		/// <summary>
		/// remember to update this!
		/// </summary>
		static readonly byte HIGHEST_BIT_IN_COLLISION_GROUPS = 64;

		#endregion -------------------- HEY THIS IS IMPORTANT ---------------------


		/// <summary>
		/// Constructor and stuff
		/// </summary>
		public CollisionReporter() {
			reporters = new CollisionReportEvent[HIGHEST_BIT_IN_COLLISION_GROUPS + 1, HIGHEST_BIT_IN_COLLISION_GROUPS + 1];
			CurrentlyCollidingWith = new Dictionary<CollisionObject, HashSet<CollisionObject>>();
			NewCollidingWith = new Dictionary<CollisionObject, HashSet<CollisionObject>>();

			PhysicsMain.PreSimulate += new PhysicsSimulateEvent(PreSimulate);
			PhysicsMain.PostSimulate += new PhysicsSimulateEvent(PostSimulate);
			PhysicsMain.ContactAdded += new ContactAdded(ContactAdded);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {

			foreach (var oldPair in CurrentlyCollidingWith) {
				// does the new dict have the old key?
				if (NewCollidingWith.ContainsKey(oldPair.Key)) {
					// find all of the objects that were in the old list but weren't in the new one
					IEnumerable<CollisionObject> oldSet = oldPair.Value.Except(NewCollidingWith[oldPair.Key]);

					foreach (CollisionObject obj in oldSet) {
						// fire events for each of them
						// this stops us from firing events twice
						if (oldPair.Key.GetCollisionGroup() < obj.GetCollisionGroup())
							SetupAndFireEvent(oldPair.Key, obj, null, null, ObjectTouchingFlags.StoppedTouching);
					}
				}
				// if it doesn't, that means two things stopped touching, and the new dict only had one object for that key.
				else if (oldPair.Value.Count > 0) {
					// this stops us from firing events twice
					CollisionObject toStopObjectA = oldPair.Key;
					CollisionObject toStopObjectB = oldPair.Value.First();

					if (toStopObjectA.GetCollisionGroup() < toStopObjectB.GetCollisionGroup())
						SetupAndFireEvent(toStopObjectA, toStopObjectB, null, null, ObjectTouchingFlags.StoppedTouching);
				}
			}

			CurrentlyCollidingWith = NewCollidingWith;
		}

		/// <summary>
		/// Fired every frame when an object is inside another object.
		/// </summary>
		bool ContactAdded(ManifoldPoint point, CollisionObject objectA, int partId0, int index0, CollisionObject objectB, int partId1, int index1) {
			// if one of the two objects is deactivated, we don't care
			if (!objectA.IsActive && !objectB.IsActive)
				return false;

			int objectACollisionGroup = (int) objectA.GetCollisionGroup();
			int objectBCollisionGroup = (int) objectB.GetCollisionGroup();

			// do we have any events that care about these groups? if not, then skip this collision pair
			if (reporters[objectACollisionGroup, objectBCollisionGroup] == null
					&& reporters[objectBCollisionGroup, objectACollisionGroup] == null)
				return false;

			// when the actual bodies are touching and not just their AABB's
			if (point.Distance <= 0.05) {
				// get the lists
				HashSet<CollisionObject> objectAList = GetCollisionListForObject(objectA, CurrentlyCollidingWith),
										 newObjectAList = GetCollisionListForObject(objectA, NewCollidingWith),
										 objectBList = GetCollisionListForObject(objectB, CurrentlyCollidingWith),
										 newObjectBList = GetCollisionListForObject(objectB, NewCollidingWith);

				// see if the other object is in there
				if (!objectAList.Contains(objectB) || !objectBList.Contains(objectA)) {
					/*
					 * if it isn't, add it! this means we have a new collision and need to fire off an event!
					 * okay now we need to get the point where it contacted!
					 * Limitation with this system: if we're already colliding with B and then collide with it in a different place without
					 * leaving the original place, we won't get another event. Why? Well because what if something's sliding along?
					 * Don't need loads of events for that
					 */
					// make sure we add it to our collections! The hashset means we don't have to worry about duplicates
					objectAList.Add(objectB);
					objectBList.Add(objectA);
					newObjectAList.Add(objectB);
					newObjectBList.Add(objectA);

					// update the dictionaries (is this necessary?)
					CurrentlyCollidingWith[objectA] = objectAList;
					CurrentlyCollidingWith[objectB] = objectBList;
					NewCollidingWith[objectA] = newObjectAList;
					NewCollidingWith[objectB] = newObjectBList;


					Vector3 pos = point.PositionWorldOnA.MidPoint(point.PositionWorldOnB);
					Vector3 normal = point.NormalWorldOnB;

					// woop woop they started touching, so we fire off an event!
					SetupAndFireEvent(objectA, objectB, pos, normal, ObjectTouchingFlags.StartedTouching);
				}
				else {
					// already in the dictionary, no new collisions. Add it to the new dictionary anyway though, because if we don't then it thinks
					// they stopped colliding. Which we don't want!
					newObjectAList.Add(objectB);
					newObjectBList.Add(objectA);

					NewCollidingWith[objectA] = newObjectAList;
					NewCollidingWith[objectB] = newObjectBList;
				}
			}
			// This means they're still inside each other's AABB's, but they aren't actually touching
			//else {

			//}

			return false;
		}

		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {

			// we start with an empty dict and then gradually build it up. After the frame, we replace the old dict with this one,
			// and then find which pairs did not exist in the new one and fire stoppedtouching events for them

			NewCollidingWith = new Dictionary<CollisionObject, HashSet<CollisionObject>>(CurrentlyCollidingWith.Count);
		}

		/// <summary>
		/// Sets up a contact event and then invokes it.
		/// </summary>
		private void SetupAndFireEvent(CollisionObject objectA, CollisionObject objectB, Vector3? position, Vector3? normal, ObjectTouchingFlags flags) {
			PonykartCollisionGroups groupA = objectA.GetCollisionGroup();
			PonykartCollisionGroups groupB = objectB.GetCollisionGroup();
			//int groupIDA = (int) groupA;
			//int groupIDB = (int) groupB;

			CollisionReportInfo info = new CollisionReportInfo {
				FirstGroup = groupA,
				SecondGroup = groupB,
				FirstObject = objectA,
				SecondObject = objectB,
				Position = position,
				Normal = normal,
				Flags = flags
			};

			FireEvent(info);
		}

		/// <summary>
		/// Gets the collision set associated with this CollisionObject in the CurrentlyCollidingWith dictionary.
		/// If the object doesn't have a set in the dictionary, this method makes one and adds it to the dictionary.
		/// It does not add anything to the new set, but it initialises it.
		/// </summary>
		/// <param name="colObj">The CollisionObject to use as the "key" in the dictionary.</param>
		/// <param name="dict">The dictionary to check against</param>
		/// <returns>The collision set associated with the given CollisionObject.</returns>
		private HashSet<CollisionObject> GetCollisionListForObject(CollisionObject colObj, IDictionary<CollisionObject, HashSet<CollisionObject>> dict) {
			HashSet<CollisionObject> colSet;
			if (!dict.TryGetValue(colObj, out colSet)) {
				colSet = new HashSet<CollisionObject>();
				dict.Add(colObj, colSet);
			}
			return colSet;
		}

		/// <summary>
		/// Hooks up an event handler to a collision event.
		/// 
		/// The order of the two groups do not matter, as it will add them to both [a,b] and [b,a].
		/// 
		/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void AddEvent(PonykartCollisionGroups firstType, PonykartCollisionGroups secondType, CollisionReportEvent handler) {
			AddEvent((int) firstType, (int) secondType, handler);
		}

		/// <summary>
		/// Hooks up an event handler to a collision event.
		/// 
		/// The order of the two groups do not matter, as it will add them to both [a,b] and [b,a].
		/// 
		/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void AddEvent(int firstType, int secondType, CollisionReportEvent handler) {
			reporters[firstType, secondType] += handler;
			reporters[secondType, firstType] += handler;
		}

		/// <summary>
		/// Removes a handler from a collision event. 
		/// 
		/// The order of the two group IDs does not matter, as it will remove from both [a,b] and [b,a].
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void RemoveEvent(PonykartCollisionGroups firstType, PonykartCollisionGroups secondType, CollisionReportEvent handler) {
			RemoveEvent((int) firstType, (int) secondType, handler);
		}

		/// <summary>
		/// Removes a handler from a collision event. 
		/// 
		/// The order of the two group IDs does not matter, as it will remove from both [a,b] and [b,a].
		/// </summary>
		/// <param name="handler">The method that will run when the event is fired</param>
		public void RemoveEvent(int firstType, int secondType, CollisionReportEvent handler) {
			reporters[firstType, secondType] -= handler;
			reporters[secondType, firstType] -= handler;
		}

		/// <summary>
		/// invoke an event.
		/// 
		/// note that it only invokes [a,b] and not [b,a] - if it invokes both, errors will happen
		/// </summary>
		private void FireEvent(CollisionReportInfo info) {
			var e = reporters[(int) info.FirstGroup, (int) info.SecondGroup];
			if (e != null) {
				e(info);
			}
		}

		/// <summary>
		/// Clear the dictionary whenever we unload a level
		/// </summary>
		private void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			CurrentlyCollidingWith.Clear();
		}
	}
}
