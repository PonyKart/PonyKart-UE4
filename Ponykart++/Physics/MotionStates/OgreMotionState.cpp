#include <OgreQuaternion.h>
#include <OgreSceneNode.h>
#include <OgreVector3.h>
#include "Actors/LThing.h"
#include "Misc/ogreExtensions.h"
#include "Misc/bulletExtensions.h"
#include "Physics/MotionStates/OgreMotionState.h"

using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Actors;
using namespace Ponykart::Physics;

/// @param thing The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
OgreMotionState::OgreMotionState(btTransform& btTransform, SceneNode* sceneNode, LThing* thing)
 : transform(btTransform), node(sceneNode), owner(thing)
{
}

/// @param thing The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
OgreMotionState::OgreMotionState(Vector3& position, Quaternion& orientation, SceneNode* sceneNode, LThing* thing)
 : transform(toBtTransform(orientation, position)), node(sceneNode), owner(thing)
{
}

/// @param thing The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
OgreMotionState::OgreMotionState(const btVector3& position, const btQuaternion& orientation, SceneNode* sceneNode, LThing* thing)
 : transform(orientation, position), node(sceneNode), owner(thing)
{
}

/// Uses the scene node's orientation and position to construct the btTransform
/// @param thing The connected lthing, used for updating sounds. You can pass null to skip updating sounds.
OgreMotionState::OgreMotionState(SceneNode* sceneNode, LThing* thing)
 : transform(toBtTransform(sceneNode->getOrientation(), sceneNode->getPosition())), node(sceneNode), owner(thing)
{
}

void OgreMotionState::getWorldTransform(btTransform& worldTrans) const
{
	worldTrans = transform;
}

void OgreMotionState::setWorldTransform(const btTransform& worldTrans)
{
	node->setOrientation(toOgreQuaternion(worldTrans.getRotation()));
	node->setPosition(toOgreVector3(worldTrans.getOrigin()));

	// update the sounds
	if (owner != nullptr)
		owner->setSoundsNeedUpdate(true);

	transform = worldTrans;
}