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

/// Get a singleton from LKernel's global map.
/// @return The requested singleton or nullptr on error.
template<typename T> T* LKernel::getG()
try
{
	return (T*)details::globalObjects[typeid(T).name()];
}
catch (...)
{
	return nullptr;
}
template<> Ogre::Root *LKernel::getG<Ogre::Root> () { return gRoot; }
template<> SDL_Window *LKernel::getG<SDL_Window>() { return gSDLWindow; }
template<> Ogre::RenderWindow *LKernel::getG<Ogre::RenderWindow>() { return gOgreWindow; }
template<> Ogre::RenderSystem *LKernel::getG<Ogre::RenderSystem>() { return gRenderSystem; }
template<> Ogre::SceneManager *LKernel::getG<Ogre::SceneManager>() { return gSceneManager; }
template<> Ogre::Viewport *LKernel::getG<Ogre::Viewport>() { return gViewport; }

/// Get a singleton from LKernel's level map.
/// @return The requested singleton or nullptr on error.
template<typename T> T* LKernel::getL()
try
{
	return (T*)details::levelObjects[typeid(T).name()];
}
catch (...)
{
	return nullptr;
}

/// Get a singleton from LKernel's level map, or LKernel's global map.
/// @return The requested singleton or throws on error.
template<typename T> T* LKernel::get()
{
	T* obj;
	obj = getL<T>();
	if (obj == nullptr)
		obj = getG<T>();
	if (obj == nullptr)
		throw string("The class \"" + typeid(T).name() + "\" is not registered to the kernel.");
	return obj;
}