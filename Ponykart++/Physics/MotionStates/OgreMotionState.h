#ifndef OGREMOTIONSTATE_H_INCLUDED
#define OGREMOTIONSTATE_H_INCLUDED

#include <LinearMath/btMotionState.h>
#include <LinearMath/btTransform.h>

namespace Ogre{class SceneNode;}

namespace Ponykart
{
namespace Actors{class LThing;}
namespace Physics
{
	// Handy thing that bullet has to keep graphics synched up with physics. It only updates the node when it's actually changed instead of every frame!
	class OgreMotionState : public btMotionState
	{
	public:
		OgreMotionState(btTransform& transform, Ogre::SceneNode* node, Actors::LThing* thing=nullptr); // thing:The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
		OgreMotionState(Ogre::Vector3& position, Ogre::Quaternion& orientation, Ogre::SceneNode* node, Actors::LThing* thing=nullptr); // thing:The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
		OgreMotionState(btVector3& position, btQuaternion& orientation, Ogre::SceneNode* node, Actors::LThing* thing=nullptr); // thing:The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
		OgreMotionState(Ogre::SceneNode* node, Actors::LThing* thing=nullptr); // thing:The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
		void getWorldTransform(btTransform& worldTrans) const override;
		void setWorldTransform(const btTransform& worldTrans) override; // Bullet only calls the update of worldtransform for active objects
	protected:
		Ogre::SceneNode* node;
		btTransform transform;
	private:
		Actors::LThing* owner;
	};
} // Physics
} // Ponykart

#endif // OGREMOTIONSTATE_H_INCLUDED
