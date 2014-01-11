#include <LinearMath/btTransform.h>
#include "Actors/Kart.h"
#include "Misc/bulletExtensions.h"
#include "Misc/ogreExtensions.h"
#include "Physics/MotionStates/KartMotionState.h"

using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Actors;
using namespace Ponykart::Physics;

KartMotionState::KartMotionState(const btVector3& position, const btQuaternion& orientation, SceneNode* Node, Kart* Kart)
: OgreMotionState(position, orientation, Node, (LThing*)Kart)
{
	transform = btTransform( orientation, position );
	node = Node;
	kart = Kart;

	lastPosition = position;
	lastOrientation = orientation;
}

void KartMotionState::getWorldTransform(btTransform& worldTrans) const
{
	worldTrans = transform;
}

void KartMotionState::setWorldTransform(const btTransform& worldTrans)
{
	// interpolate the karts' movement to remove jittery-ness
	btVector3 newPos = worldTrans.getOrigin();
	btQuaternion newOrient = worldTrans.getRotation();

	// bias the newer orientations
	btVector3 avgPos = (lastPosition * INV_BIAS) + (newPos * BIAS);
	btQuaternion avgOrient = toBtQuaternion(Quaternion::Slerp(BIAS, toOgreQuaternion(lastOrientation), toOgreQuaternion(newOrient), true));

	node->setPosition(toOgreVector3(avgPos));
	node->setOrientation(toOgreQuaternion(avgOrient));

	lastPosition = avgPos;
	lastOrientation = avgOrient;
	actualPosition = newPos;
	actualOrientation = newOrient;

	// update the sounds
	if (kart != nullptr)
		kart->setSoundsNeedUpdate(true);

	transform = worldTrans;
}