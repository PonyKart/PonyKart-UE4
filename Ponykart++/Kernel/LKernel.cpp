#include <OgreRoot.h>
#include <OgreLogManager.h>
#include "Kernel/LKernel.h"

using namespace Ponykart;
using namespace LKernel;
using namespace LKernel::details;

// Define the globals
Ogre::Root* LKernel::gRoot;
Ogre::RenderWindow* LKernel::gWindow;
Ogre::RenderSystem* LKernel::gRenderSystem;
Ogre::SceneManager* LKernel::gSceneManager;
Ogre::Viewport* LKernel::gViewport;
std::vector<std::function<void (void*)>> LKernel::onEveryUnpausedTenthOfASecondEvent;
std::unordered_map<std::string, LKernelObject*> LKernel::details::globalObjects;
std::unordered_map<std::string, LKernelObject*> LKernel::details::levelObjects;


LKernelObject* Ponykart::LKernel::addGlobalObject(LKernelObject* object, const std::string& type)
{
	if (globalObjects.find(type) != globalObjects.end())
		throw std::string(std::string("Global object already added ") + type);

	globalObjects.insert(std::pair<std::string, LKernelObject*>(type,object));

	return object;
}

template<> Ogre::Root *getG<Ogre::Root> () { return gRoot; }
template<> Ogre::RenderWindow *getG<Ogre::RenderWindow> () { return gWindow; }
template<> Ogre::RenderSystem *getG<Ogre::RenderSystem> () { return gRenderSystem; }
template<> Ogre::SceneManager *getG<Ogre::SceneManager> () { return gSceneManager; }
template<> Ogre::Viewport *getG<Ogre::Viewport> () { return gViewport; }

void Ponykart::LKernel::shutdown ()
{
	for (auto obj : globalObjects)
		delete obj.second;
	globalObjects.clear();

	delete gRoot;
}
