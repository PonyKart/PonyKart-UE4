#include <OgreFrameListener.h>
#include "Core/Pauser.h"
#include "Core/Animation/AnimationBlender.h"
#include "Core/Animation/AnimationManager.h"
#include "Kernel/LKernel.h"
#include "Levels/LevelChangedEventArgs.h"
#include "Levels/LevelManager.h"

using namespace std;
using namespace Ogre;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;

AnimationManager::AnimationManager()
{
	LevelManager::onLevelLoad.push_back(bind(&AnimationManager::onLevelLoad,this,placeholders::_1));
	LevelManager::onLevelUnload.push_back(bind(&AnimationManager::onLevelUnload,this,placeholders::_1));
}

void AnimationManager::add(Ogre::AnimationBlender* ab)
{
	blenders.push_back(ab);
}

void AnimationManager::add(Ogre::AnimationState* state)
{
	states.push_back(state);
}

void AnimationManager::remove(Ogre::AnimationBlender* ab)
{
	for (auto it=blenders.begin(); it!=blenders.end(); it++)
		if (*it == ab)
		{
			blenders.erase(it);
			break;
		}
}

void AnimationManager::remove(Ogre::AnimationState* state)
{
	for (auto it=states.begin(); it!=states.end(); it++)
		if (*it == state)
		{
			states.erase(it);
			break;
		}
}

void AnimationManager::onLevelLoad(LevelChangedEventArgs* eventArgs)
{
	LKernel::gRoot->addFrameListener(this);
}

void AnimationManager::onLevelUnload(LevelChangedEventArgs* eventArgs)
{
	blenders.clear();
	states.clear();
	skeletonEntities.clear();
	LKernel::gRoot->removeFrameListener(this);
}

bool AnimationManager::frameStarted(const Ogre::FrameEvent& evt)
{
	if (!Pauser::isPaused)
	{
		for (AnimationBlender* b : blenders)
			if (!b->getSource()->hasEnded())
				b->addTime(evt.timeSinceLastFrame);

		for (AnimationState* state : states)
			if (!state->hasEnded())
				state->addTime(evt.timeSinceLastFrame);
	}
	return true;
}
