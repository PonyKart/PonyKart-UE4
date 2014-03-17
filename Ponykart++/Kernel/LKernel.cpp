#include "pch.h"
#include "Kernel/LKernel.h"
#include <OgreRoot.h>
#include <OgreLogManager.h>

using namespace Ponykart;
using namespace LKernel;
using namespace LKernel::details;

// Define the globals
Ogre::Root* LKernel::gRoot;
SDL_Window *LKernel::gSDLWindow;
Ogre::RenderWindow* LKernel::gOgreWindow;
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

template<> Ogre::Root *LKernel::getG<Ogre::Root> () { return gRoot; }
template<> SDL_Window *LKernel::getG<SDL_Window>() { return gSDLWindow; }
template<> Ogre::RenderWindow *LKernel::getG<Ogre::RenderWindow>() { return gOgreWindow; }
template<> Ogre::RenderSystem *LKernel::getG<Ogre::RenderSystem>() { return gRenderSystem; }
template<> Ogre::SceneManager *LKernel::getG<Ogre::SceneManager>() { return gSceneManager; }
template<> Ogre::Viewport *LKernel::getG<Ogre::Viewport>() { return gViewport; }
