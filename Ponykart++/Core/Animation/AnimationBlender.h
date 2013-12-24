#ifndef ANIMATIONBLENDER_H_INCLUDED
#define ANIMATIONBLENDER_H_INCLUDED

/**
 * Porting notes:
 * Originally ported to Mogre by Ernesto Gutierrez (smernesto) - 02-july-2007
 *
 * http://www.ogre3d.org/tikiwiki/MOGRE+AnimationBlender
 * http://www.ogre3d.org/tikiwiki/AnimationBlender#Utilisation
**/

#include <string>
#include <Ogre.h>

namespace Ogre
{
	enum AnimationBlendingTransition
	{
		BlendSwitch, // immediately switch - stop source and start dest
		BlendWhileAnimating, // cross fade, blend source animation out while blending destination animation in
		BlendThenAnimate // blend source to first frame of dest, when done, start dest anim
	};

	class AnimationBlender
	{
	public:
		AnimationBlender(Entity* entity);
		void addTime(float time); // Adds time to the animation, similar to AnimationState.AddTime()
		void blend(const std::string& animation, AnimationBlendingTransition transition, float duration, bool looping); // Fade between two states
		void init(const std::string& animation, bool looping); // Initialise the animation blender with an initial animation
		// Getters
        inline float getProgress() const {return mTimeleft / mDuration;};
        inline AnimationState* getSource() {return mSource;};
        inline AnimationState* getTarget() {return mTarget;};
        inline Entity* getEntity() {return mEntity;};
        inline bool isComplete() const {return mTimeleft < mDuration;};
        inline float getTimeLeft() const {return mTimeleft;};
        inline float getDuration() const {return mDuration;};
        inline const std::string& getCurrentAnimation() const {return mCurrentAnim;};
	private:
		Entity* mEntity;
		AnimationState* mSource;
		AnimationState* mTarget;
		AnimationBlendingTransition mTransition;
		bool mLoop;
		float mTimeleft, mDuration;
		bool mComplete;
		std::string mCurrentAnim;
	};

} // Ogre

#endif // ANIMATIONBLENDER_H_INCLUDED
