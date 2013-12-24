#include "Core/Animation/AnimationBlender.h"
#include "Core/Animation/AnimationManager.h"
#include "Levels/LevelChangedEventArgs.h"
#include "Levels/LevelManager.h"

using namespace Ponykart::Core;
using namespace Ponykart::Levels;

AnimationManager::AnimationManager()
{
	LevelManager::onLevelLoad.push_back(onLevelLoad);
	LevelManager::onLevelUnload.push_back(onLevelUnload);
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
