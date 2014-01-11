#ifndef ANIMATIONMANAGER_H_INCLUDED
#define ANIMATIONMANAGER_H_INCLUDED

#include <string>
#include <vector>
#include <unordered_map>
#include <OgreFrameListener.h>

namespace Ogre
{
	class AnimationBlender;
	struct FrameEvent;
}

namespace Ponykart
{

namespace Levels
{
	class LevelChangedEventArgs;
} // Levels

namespace Core
{
	/// Instead of each ModelComponent hooking up to FrameStarted to update any animation it might have, we just give it
	/// to this class to update it for us. This means we only have to hook one FrameStarted method up instead of loads of them.
	class AnimationManager : public Ogre::FrameListener
	{
	public:
		AnimationManager();
		void add(Ogre::AnimationBlender* ab); ///< Add an animation to be automatically updated
		void add(Ogre::AnimationState* state); ///< Add an animation to be automatically updated
		void remove(Ogre::AnimationBlender* ab); ///< Remove an animation from being automatically updated
		void remove(Ogre::AnimationState* state); ///< Remove an animation from being automatically updated
	private:
		bool frameStarted(const Ogre::FrameEvent& evt) override; ///< update all of our animations, but only if we aren't paused
		void onLevelLoad(Levels::LevelChangedEventArgs* eventArgs); ///< hook up to the frame started event
		void onLevelUnload(Levels::LevelChangedEventArgs* eventArgs); ///< clear our states list and disconnect from the frame started event
	private:
		std::vector<Ogre::AnimationBlender*> blenders;
		std::vector<Ogre::AnimationState*> states;
		std::unordered_map<std::string, Ogre::Entity*> skeletonEntities;
	};
} // Core
} // Ponykart

#endif // ANIMATIONMANAGER_H_INCLUDED
