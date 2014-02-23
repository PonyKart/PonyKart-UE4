#include <string>
#include <vector>
#include <unordered_set>
#include <functional>
#include "Physics/TriggerRegions/TriggerReportFlags.h"
#include "Physics/CollisionReports/CollisionReportInfo.h"

class btRigidBody;
class btGhostObject;
class btCollisionShape;
namespace Ogre
{
	class SceneNode;
	class Vector3;
	class Quaternion;
	class Entity;
}
namespace Ponykart
{
namespace Levels{ class LevelChangedEventArgs; }
namespace Physics
{
	class TriggerRegion;
	using TriggerReportEvent = std::vector<std::function<void(TriggerRegion& region, btRigidBody* otherBody, 
															TriggerReportFlags flags, CollisionReportInfo& info)>>;

	class TriggerRegion
	{
	public:
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		TriggerRegion(std::string& name, Ogre::Vector3& position, btCollisionShape* shape);
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// @param orientation a degree vector
		TriggerRegion(std::string& name, Ogre::Vector3& position, Ogre::Quaternion& orientation, btCollisionShape* shape);
		void invokeTrigger(btRigidBody* otherBody, TriggerReportFlags flags, CollisionReportInfo& info); ///< Run the enter event
		void cycleToNextColor(); ///< Changes the region's color to the next one in the cycle
		std::string toString();
		int getHashCode();

	public:
		std::unordered_set<btRigidBody*> currentlyCollidingWith;
		TriggerReportEvent onTrigger; ///< Invoked by TriggerReporter
	protected:
		btGhostObject* ghost;
		std::string name;
		Ogre::SceneNode* node;
		Ogre::Entity* entity;
	};
}
}