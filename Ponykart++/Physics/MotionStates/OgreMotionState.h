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
	/// Handy thing that bullet has to keep graphics synched up with physics. It only updates the node when it's actually changed instead of every frame!
	class OgreMotionState : public btMotionState
	{
	public:
		OgreMotionState(btTransform& btTransform, Ogre::SceneNode* sceneNode, Actors::LThing* thing=nullptr); 
		OgreMotionState(const btVector3& position, const btQuaternion& orientation, Ogre::SceneNode* sceneNode, Actors::LThing* thing = nullptr);
		OgreMotionState(Ogre::Vector3& position, Ogre::Quaternion& orientation, Ogre::SceneNode* sceneNode, Actors::LThing* thing=nullptr); 
		OgreMotionState(Ogre::SceneNode* sceneNode, Actors::LThing* thing=nullptr);
		void getWorldTransform(btTransform& worldTrans) const override;
		void setWorldTransform(const btTransform& worldTrans) override; ///< Bullet only calls the update of worldtransform for active objects
	protected:
		btTransform transform;
		Ogre::SceneNode* node;
	private:
		Actors::LThing* owner;
	};
} // Physics
} // Ponykart

#endif // OGREMOTIONSTATE_H_INCLUDED
