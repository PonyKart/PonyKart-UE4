#include <OgreQuaternion.h>
#include <OgreSceneNode.h>
#include <OgreVector3.h>
#include "Actors/LThing.h"
#include "Misc/bulletExtensions.h"
#include "Physics/MotionStates/OgreMotionState.h"

using namespace Ogre;
using namespace Ponykart::Actors;
using namespace Ponykart::Physics;

OgreMotionState::OgreMotionState(btTransform& btTransform, SceneNode* sceneNode, LThing* thing)
 : transform(btTransform), node(sceneNode), owner(thing)
{
}

OgreMotionState::OgreMotionState(Vector3& position, Quaternion& orientation, SceneNode* sceneNode, LThing* thing)
 : transform(toBtTransform(orientation, position)), node(sceneNode), owner(thing)
{
}

OgreMotionState::OgreMotionState(btVector3& position, btQuaternion& orientation, SceneNode* sceneNode, LThing* thing)
 : transform(orientation, position), node(sceneNode), owner(thing)
{
}

/// Uses the scene node's orientation and position to construct the btTransform
OgreMotionState::OgreMotionState(SceneNode* sceneNode, LThing* thing)
 : transform(toBtTransform(sceneNode->getOrientation(), sceneNode->getPosition())), node(sceneNode), owner(thing)
{
}
