#include "Kernel/LKernel.h"

using namespace Ponykart;
using namespace LKernel::details;

// Define the globals
Ogre::Root* LKernel::root;
Ogre::RenderWindow* LKernel::window;
Ogre::RenderSystem* LKernel::renderSystem;
Ogre::SceneManager* LKernel::sceneManager;
Ogre::Viewport* LKernel::viewport;
std::unordered_map<std::string,void*> LKernel::details::globalObjects;
std::unordered_map<std::string,void*> LKernel::details::levelObjects;

void* LKernel::addGlobalObject(void* object, std::string type)
{
	if (globalObjects.find(type) != globalObjects.end())
		throw std::string(std::string("Global object already added ") + type);

	globalObjects.insert(std::pair<std::string,void*>(type,object));

	return object;
}
