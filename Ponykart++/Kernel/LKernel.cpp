#include <OgreLogManager.h>
#include "Kernel/LKernel.h"

using namespace Ponykart;
using namespace LKernel::details;

// Define the globals
Ogre::Root* LKernel::gRoot;
Ogre::RenderWindow* LKernel::gWindow;
Ogre::RenderSystem* LKernel::gRenderSystem;
Ogre::SceneManager* LKernel::gSceneManager;
Ogre::Viewport* LKernel::gViewport;
std::vector<std::function<void(void*)>> LKernel::onEveryUnpausedTenthOfASecondEvent;
std::unordered_map<std::string,void*> LKernel::details::globalObjects;
std::unordered_map<std::string,void*> LKernel::details::levelObjects;

void* LKernel::addGlobalObject(void* object, const std::string& type)
{
	if (globalObjects.find(type) != globalObjects.end())
		throw std::string(std::string("Global object already added ") + type);

	globalObjects.insert(std::pair<std::string,void*>(type,object));

	return object;
}
