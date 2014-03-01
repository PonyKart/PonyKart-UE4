#ifndef COLLISIONREPORTER_H_INCLUDED
#define COLLISIONREPORTER_H_INCLUDED

#include <functional>
#include <unordered_map>
#include <unordered_set>
#include <vector>
#include "Physics/CollisionGroups.h"
#include "Physics/CollisionReports/CollisionReportInfo.h"
#include "Kernel/LKernelObject.h"

class btCollisionObject;
struct btCollisionObjectWrapper;
class btDiscreteDynamicsWorld;
class btManifoldPoint;
namespace Ogre { struct FrameEvent; }
namespace Ponykart
{
namespace Physics
{
	struct CollisionReportInfo;

	/// Our delegate for collision reports
	using CollisionReportEvent = std::vector<std::function<void(CollisionReportInfo* info)>>;

	/// Our class for handling all collision reports, firing events when physics objects collide.
	/// 
	/// I'm sure there's a better way of dealing with these to speed it up, but hey how it is now is pretty efficient and isn't very high up on the
	/// profiling thing yet, so it's good enough for now.
	/// 
	/// Remember that things you want to be collided with need to have their CollisionFlags.CustomMaterialCallback flag set!
	class CollisionReporter : public LKernel::LKernelObject
	{
		static const unsigned char HIGHEST_BIT_IN_COLLISION_GROUPS = 64; ///< Remember to update this!

	public:
		explicit CollisionReporter(); ///< Constructor and stuff
		void preSimulate(btDiscreteDynamicsWorld* world, const Ogre::FrameEvent& evt);
		void postSimulate(btDiscreteDynamicsWorld* world, const Ogre::FrameEvent& evt);
		/// Fired every frame when an object is inside another object.
		bool contactAdded(btManifoldPoint& point, btCollisionObjectWrapper* objectA, int partId0, int index0, 
												btCollisionObjectWrapper* objectB, int partId1, int index1);
		void addEvent(PonykartCollisionGroups firstType, PonykartCollisionGroups secondType, CollisionReportEvent handler);
		void addEvent(int firstType, int secondType, CollisionReportEvent handler);
	private:
		/// Clear the dictionary whenever we unload a level
		void onLevelUnload(Levels::LevelChangedEventArgs* eventArgs);
		/// Sets up a contact event and then invokes it.
		/// @param position Optional, can be nullptr.
		/// @param normal Optional, can be nullptr.
		void setupAndFireEvent(const btCollisionObject* const objectA, const btCollisionObject* const objectB, 
								Ogre::Vector3* position, Ogre::Vector3* normal, ObjectTouchingFlags flags);
		std::unordered_set<const btCollisionObject*> getCollisionListForObject(const btCollisionObject* const colObj, 
									std::unordered_map<const btCollisionObject*, std::unordered_set<const btCollisionObject*>>& dict);
		/// invoke an event.
		/// note that it only invokes [a,b] and not [b,a] - if it invokes both, errors will happen
		void fireEvent(CollisionReportInfo* info);

	private:
		CollisionReportEvent** reporters; ///< our 2D array of contact report delegates
		/// Our map of collision objects, with a set containing the objects it collided with last frame.
		/// Why a hash set? They prevent having multiple identical objects, but also don't throw an error if they already contain it.
		/// They just silently ignore it (though .Add does return whether the adding was successful or not)
		std::unordered_map<const btCollisionObject*, std::unordered_set<const btCollisionObject*>> currentlyCollidingWith;
		/// This map contains objects that collided *this* frame.
		std::unordered_map<const btCollisionObject*, std::unordered_set<const btCollisionObject*>> newCollidingWith;
	};
}
}

#endif // COLLISIONREPORTER_H_INCLUDED
