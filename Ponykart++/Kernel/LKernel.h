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
	extern Ogre::Root* gRoot;
	extern Ogre::RenderWindow* gWindow;
	extern Ogre::RenderSystem* gRenderSystem;
	extern Ogre::SceneManager* gSceneManager;
	extern Ogre::Viewport* gViewport;

	/// Implementation details that are not part of the interface.
	namespace details
	{
		extern std::unordered_map<std::string,void*> globalObjects;
		extern std::unordered_map<std::string,void*> levelObjects;
	} // details

	// Interface
	void log(const std::string& message); ///< Logs a message with Ogre. Ogre must be initialized.
	void loadInitialObjects(Splash& splash);
	void* addGlobalObject(void* object, const std::string& typeName); ///< Add a singleton to LKernel's map.
	template<typename T> static inline T* addGlobalObject(T* object) {return (T*)addGlobalObject(object,typeid(T).name());} ///< Add a singleton to LKernel's map.
	template<typename T> static T* getG() {return (T*)details::globalObjects[typeid(T).name()];} ///< Get a singleton from LKernel's map.
} // LKernel
} // Ponykart

#endif // LKERNEL_H_INCLUDED
