#ifndef LKERNEL_H_INCLUDED
#define LKERNEL_H_INCLUDED

#include <unordered_map>
#include <typeinfo>

#include <OgreTextAreaOverlayElement.h>
#include <OgreFontManager.h>

#include "UI/Splash.h"

namespace Ponykart
{
namespace LKernel
{
	// Anyone can get those from ogre's interface, but accessing them throught LKernel is faster.
	extern Ogre::Root* root;
	extern Ogre::RenderWindow* window;
	extern Ogre::RenderSystem* renderSystem;
	extern Ogre::SceneManager* sceneManager;
	extern Ogre::Viewport* viewport;

	// Implementation details that are not part of the interface.
	namespace details
	{
		void initOgreResources(); // Basically adds all of the resource locations but doesn't actually load anything.
		void loadOgreResourceGroups(); // This is where resources are actually loaded into memory.

		extern std::unordered_map<std::string,void*> globalObjects;
		extern std::unordered_map<std::string,void*> levelObjects;
	} // details

	// Interface
	inline void log(std::string message) {Ogre::LogManager::getSingleton().logMessage(message);}; // Ogre must be initialized.
	void initOgreRoot();
	void initOgreRenderSystem();
	void initOgreRenderWindow();
	void initOgreSceneManager();
	void initOgreViewportCam();
	void loadInitialObjects(Splash& splash);
	void* addGlobalObject(void* object, std::string typeName);
	template<typename T> static inline T* addGlobalObject(T* object) {return (T*)addGlobalObject(object,typeid(T).name());}
	template<typename T> static T* GetG() {return (T*)details::globalObjects[typeid(T).name()];}
} // LKernel
} // Ponykart

#endif // LKERNEL_H_INCLUDED
