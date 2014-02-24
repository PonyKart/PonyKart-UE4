#ifndef TRIGGERREGION_H_INCLUDED
#define TRIGGERREGION_H_INCLUDED

#include <functional>
#include <string>
#include <unordered_set>
#include <vector>
#include "Core/GlowColours.h"
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
	class CollisionObjectDataHolder;
	class TriggerRegion;
	using TriggerReportEvent = std::vector<std::function<void(TriggerRegion& region, btRigidBody* otherBody, 
															TriggerReportFlags flags, CollisionReportInfo& info)>>;

	class TriggerRegion
	{
	public:
		TriggerRegion(const std::string& Name, const Ogre::Vector3& position, btCollisionShape* shape);
		TriggerRegion(const std::string& Name, const Ogre::Vector3& position, const Ogre::Quaternion& orientation, btCollisionShape* shape);
		void invokeTrigger(btRigidBody* otherBody, TriggerReportFlags flags, CollisionReportInfo& info); ///< Run the enter event
		void cycleToNextColor(); ///< Changes the region's color to the next one in the cycle
		std::string toString();
		int getHashCode();
		BalloonGlowColour getGlowColor();
		void setGlowColor(BalloonGlowColour color);

	public:
		std::unordered_set<btRigidBody*> currentlyCollidingWith;
		TriggerReportEvent onTrigger; ///< Invoked by TriggerReporter
	protected:
		std::pair<btGhostObject*, CollisionObjectDataHolder*> ghost;
		std::string name;
		Ogre::SceneNode* node;
		Ogre::Entity* entity;
	private:
		/// Must be one of: red, blue, yellow, green, orange, magenta, purple, cyan, white
		BalloonGlowColour _balloonColor = BalloonGlowColour::red;
	};
}
}
#endif TRIGGERREGION_H_INCLUDED