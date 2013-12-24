#include "Core/Animation/AnimationBlender.h"

using namespace std;
using namespace Ogre;

AnimationBlender::AnimationBlender(Entity* entity) : mEntity(entity)
{
}

void AnimationBlender::blend(const string& animation, AnimationBlendingTransition transition, float duration, bool looping)
{
	// return if we're already playing this animation or if that animation doesn't exist
	if (mCurrentAnim == animation || !mEntity->getAllAnimationStates()->hasAnimationState(animation))
		return;
	mCurrentAnim = animation;

	mLoop = looping;

	if (transition == AnimationBlendingTransition::BlendSwitch)
	{
		if (mSource != nullptr)
			mSource->setEnabled(false);

		mSource = mEntity->getAnimationState(animation);
		mSource->setEnabled(true);
		mSource->setWeight(1);
		mSource->setTimePosition(0);
		mSource->setLoop(looping);

		mComplete = true;

		mTimeleft = 0;
	}
	else
	{
		AnimationState* newTarget = mEntity->getAnimationState(animation);

		if (mTimeleft > 0)
		{
			// oops, weren't finished yet
			if (*newTarget == *mSource)
			{
				// going back to the source state, so let's switch
				mSource = mTarget;
				mTarget = newTarget;
				mTimeleft = mDuration - mTimeleft; // i'm ignoring the new duration here

				mComplete = false;
			}
			else if (*newTarget != *mTarget)
			{
				// ok, newTarget is really new, so either we simply replace the target with this one, or
				// we make the target the new source
				if (mTimeleft < mDuration * 0.5f)
				{
					// simply replace the target with this one
					mTarget->setEnabled(false);
					mTarget->setWeight(0);
				}
				else
				{
					// old target becomes new source
					mSource->setEnabled(false);
					mSource->setWeight(0);
					mSource = mTarget;

				}
				mTarget = newTarget;
				mTarget->setEnabled(true);
				mTarget->setWeight(1.0 - mTimeleft / mDuration);
				mTarget->setTimePosition(0);
				mTarget->setLoop(looping);
				mComplete = false;
			}
			// else -> nothing to do! (ignoring duration here)
		}
		else {
			// assert( target == 0, "target should be 0 when not blending" )
			// mSource->setEnabled(true);
			// mSource->setWeight(1);
			mTransition = transition;
			mTimeleft = mDuration = duration;
			mTarget = newTarget;
			mTarget->setEnabled(true);
			mTarget->setWeight(0);
			mTarget->setTimePosition(0);
			mComplete = false;
		}
	}
}

void AnimationBlender::addTime(float time)
{
	if (mSource != nullptr)
	{
		if (!mComplete)
		{
			if (mTimeleft > 0)
			{
				mTimeleft -= time;

				if (mTimeleft < 0)
				{
					// finish blending
					mSource->setEnabled(false);
					mSource->setWeight(0);
					mSource = mTarget;
					mSource->setEnabled(true);
					mSource->setWeight(1);
					mTarget = nullptr;
				}
				else
				{
					// still blending, advance weights
					mSource->setWeight(mTimeleft / mDuration);
					mTarget->setWeight(1.0 - mTimeleft / mDuration);

					if (mTransition == AnimationBlendingTransition::BlendWhileAnimating)
						mTarget->addTime(time);
				}
			}

			if (mSource->getTimePosition() >= mSource->getLength())
				mComplete = true;
			else
				mComplete = false;
		}

		mSource->addTime(time);
		mSource->setLoop(mLoop);
	}
}

void AnimationBlender::init(const string& animation, bool looping)
{
	AnimationStateSet* set = mEntity->getAllAnimationStates();
	if (set == nullptr)
		throw string("AnimationBlender::init:That mesh does not have any animations!");

	for (auto animPair : set->getAnimationStateIterator())
	{
		animPair.second->setEnabled(false);
		animPair.second->setWeight(0);
		animPair.second->setTimePosition(0);
	}

	mCurrentAnim = animation;
	mSource = mEntity->getAnimationState(animation);
	mSource->setEnabled(true);
	mSource->setWeight(1);
	mTimeleft = 0;
	mDuration = 1;
	mTarget = nullptr;
	mComplete = true;
	mLoop = looping;
}
