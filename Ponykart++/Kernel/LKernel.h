#ifndef LKERNEL_H_INCLUDED
#define LKERNEL_H_INCLUDED

#include <functional>
#include <vector>
#include <unordered_map>
#include <typeinfo>
#include <SDL.h>
#include <OgreTextAreaOverlayElement.h>
#include <OgreFontManager.h>

#include "LKernelObject.h"
#include "UI/Splash.h"

namespace Ponykart
{
namespace LKernel
{
	// Anyone can get those from ogre's interface, but accessing them throught LKernel is faster.
	extern Ogre::Root* gRoot;
	extern SDL_Window *gSDLWindow;
	extern Ogre::RenderWindow* gOgreWindow;
	extern Ogre::RenderSystem* gRenderSystem;
	extern Ogre::SceneManager* gSceneManager;
	extern Ogre::Viewport* gViewport;
	extern std::vector<std::function<void (void*)>> onEveryUnpausedTenthOfASecondEvent;

	/// Implementation details that are not part of the interface.
	namespace details
	{
		extern std::unordered_map<std::string, LKernelObject*> globalObjects;
		extern std::unordered_map<std::string, LKernelObject*> levelObjects;
	} // details

	// Interface
	void loadInitialObjects(Splash& splash);
	LKernelObject* addGlobalObject(LKernelObject* object, const std::string& typeName); ///< Add a singleton to LKernel's global map.
	template<typename T> inline T* addGlobalObject(T* object) {return (T*)addGlobalObject(object,typeid(T).name());} ///< Add a singleton to LKernel's global map.
	template<typename T> T* getG();
	template<> Ogre::Root *getG<Ogre::Root> ();
	template<> Ogre::RenderWindow *getG<Ogre::RenderWindow> ();
	template<> SDL_Window *getG<SDL_Window> ();
	template<> Ogre::RenderSystem *getG<Ogre::RenderSystem> ();
	template<> Ogre::SceneManager *getG<Ogre::SceneManager> ();
	template<> Ogre::Viewport *getG<Ogre::Viewport> ();
	template<typename T> T* getL();
	template<typename T> T* get();

	
} // LKernel
} // Ponykart

#endif // LKERNEL_H_INCLUDED
