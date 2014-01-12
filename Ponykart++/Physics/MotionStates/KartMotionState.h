#include <LinearMath/btVector3.h>
#include <LinearMath/btQuaternion.h>
#include "Physics/MotionStates/OgreMotionState.h"

namespace Ponykart
{
namespace Actors{ class Kart; }
namespace Physics
{
	class KartMotionState : OgreMotionState
	{
	public:
		/// @param kart The connected kart, used for updating sounds. You can pass nullptr to skip updating sounds.
		KartMotionState(const btVector3& position, const btQuaternion& orientation, Ogre::SceneNode* Node, Actors::Kart* Kart=nullptr);
		void getWorldTransform(btTransform& worldTrans) const override;
		void setWorldTransform(const btTransform& worldTrans) override;
	public:
		btVector3 lastPosition;
		btQuaternion lastOrientation;
		const float BIAS = 0.6f; ///< how much "weight" the new transform has
		const float INV_BIAS = 1 - BIAS; ///< how much weight the old transform has

		btVector3 actualPosition;
		btQuaternion actualOrientation;
	private:
		Actors::Kart* kart;
	};
}
}