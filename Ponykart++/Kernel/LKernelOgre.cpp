#include "pch.h"
#include <string>
#include <SDL.h>
#include <OgreConfigFile.h>
#include <OgreMeshManager.h>
#include <OgreSkeletonManager.h>
#include <OgreRenderWindow.h>
#include <OgreRoot.h>
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Core/Options.h"
#include "Misc/sdl2Extensions.h"
#include <SDL_syswm.h>

using namespace std;
using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ogre;
using namespace Extensions;

void LKernel::initOgreRoot()
{
	if (SDL_Init(SDL_INIT_TIMER | SDL_INIT_EVENTS) < 0)
		throw SDL2Exception();

#if defined(DEBUG) && defined(WIN32)
	gRoot = new Ogre::Root("media_debug/config/plugins.cfg", "", "Ponykart.log");
#else
	gRoot = new Ogre::Root("media/config/plugins.cfg", "", "Ponykart.log");
#endif
}

void LKernel::initOgreGraphics ()
{
	log("[Loading] Initializing Ogre Resources...");

	if (SDL_InitSubSystem(SDL_INIT_VIDEO) < 0)
		throw string(SDL_GetError());

	int width, height;
	// Parse video mode
	{
		String vm = Options::get("Video Mode");
		String::size_type xPos = vm.find('x');
		if (xPos != String::npos)
		{
			width = StringConverter::parseUnsignedInt(vm.substr(0, xPos));
			height = StringConverter::parseUnsignedInt(vm.substr(xPos + 1));
		}
	}

	bool fullscreen = StringConverter::parseBool(Options::get("Full Screen"));
	gSDLWindow = SDL_CreateWindow(
		"PonyKart",
		SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
		width, height,
		SDL_WINDOW_OPENGL | (fullscreen ? SDL_WINDOW_FULLSCREEN : 0)
	);

	SDL_GL_SetAttribute(SDL_GL_RED_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);

	auto fsaa = StringConverter::parseUnsignedInt(Options::get("FSAA"));
	if (fsaa > 0) {
		SDL_GL_SetAttribute(SDL_GL_MULTISAMPLEBUFFERS, 1);
		SDL_GL_SetAttribute(SDL_GL_MULTISAMPLESAMPLES, fsaa);
	}
	SDL_GL_SetAttribute(
		SDL_GL_FRAMEBUFFER_SRGB_CAPABLE,
		StringConverter::parseBool(Options::get("sRGB Gamma Conversion")) ? 1 : 0
	);

	auto context = SDL_GL_CreateContext(gSDLWindow);
	if (context == NULL)
		throw string(SDL_GetError());
	SDL_GL_MakeCurrent(gSDLWindow, context);

	if (StringConverter::parseBool(Options::get("VSync")))
		if (SDL_GL_SetSwapInterval(-1) < 0) // Try to enable late-swap tearing support...
			if (SDL_GL_SetSwapInterval(1)) // ...and fall back to standard VSync if not supported.
				log("[Loading] WARNING: Unable to activate VSync!");

	gRenderSystem = gRoot->getRenderSystemByName("OpenGL Rendering Subsystem");
	//gRenderSystem->setFixedPipelineEnabled(false); // Fixed-function pipeline is deprecated
	gRoot->setRenderSystem(gRenderSystem); // Add to global objects
//#if DEBUG
//	// print out the things we can support
//	auto renderList = gRoot->getAvailableRenderers();
//	for (auto renderSystem : renderList)
//	{
//		log("\n**** Available options for Render System: " + renderSystem->getName() + " ****");
//		for (auto option : renderSystem->getConfigOptions())
//		{
//			log("\t" + option.first);
//			for (auto p : option.second.possibleValues)
//				log("\t\t" + p);
//		}
//		log("***********************************");
//	}
//#endif

	gRoot->initialise(false);

	Ogre::NameValuePairList ogreWindowParams;
#if defined(SDL_VIDEO_DRIVER_WINDOWS)
	SDL_SysWMinfo wmInfo;
	SDL_VERSION(&wmInfo.version);
	if (SDL_GetWindowWMInfo(gSDLWindow, &wmInfo)) {
		ogreWindowParams["externalWindowHandle"] = StringConverter::toString(wmInfo.info.win.window);
		ogreWindowParams["externalGLContext"] = StringConverter::toString(reinterpret_cast<size_t>(context));
		ogreWindowParams["externalGLControl"] = String("true");
	} else
		throw string(SDL_GetError());
#else
	ogreWindowParams["currentGLContext"] = String("true");
	ogreWindowParams["externalGLControl"] = String("true");
#endif

	gOgreWindow = gRoot->createRenderWindow("OgreWindow", width, height, fullscreen, &ogreWindowParams);
	gOgreWindow->setVisible(true);

	//void LKernel::initOgreSceneManager()
	gSceneManager = gRoot->createSceneManager("OctreeSceneManager","sceneMgr");

	//void LKernel::initOgreViewportCam()
	gViewport = gOgreWindow->addViewport(gSceneManager->createCamera("tempCam"));

	log("[Loading] Ogre Graphics Initialized.");
}

void LKernel::details::initOgreResources()
{
	log("[Loading] Initializing Ogre Resources...");

	ConfigFile* file = new ConfigFile();
	file->load("media/config/ponykart.res", "\t:=", true);
	ConfigFile::SectionIterator sectionIterator = file->getSectionIterator();

	while(sectionIterator.hasMoreElements())
	{
		std::string currentKey = sectionIterator.peekNextKey();
		ConfigFile::SettingsMultiMap* settings=sectionIterator.getNext();
		for (auto curPair : *settings)
		{
			std::string key = curPair.first;
			std::string name = curPair.second;
			ResourceGroupManager::getSingleton().addResourceLocation(name, key, currentKey);
		}
	}

	delete file;

	log("[Loading] Ogre Resources Initialized.");
}

void LKernel::details::loadOgreResourceGroups()
{
			// knighty, this uncommented means we USE the mipmaps from the DDSes
			// try it - comment it out, then look at the detail on the roads and far-off tree billboards (yes you'll need to change your ModelDetail to see them)
			// then uncomment this and look at them again - notice how much sharper they are? That's because they're now using the DDS files' mipmaps,
			// which were created in photoshop to look a lot nicer than the blurry mess ogre creates on the fly.
			TextureManager::getSingleton().setDefaultNumMipmaps(8);

#if !DEBUG
			TextureManager::getSingleton().setVerbose(false);
			MeshManager::getSingleton().setVerbose(false);
			SkeletonManager::getSingleton().setVerbose(false);
#endif

#if DEBUG
			ResourceGroupManager::getSingleton().initialiseAllResourceGroups();
#else
			//ResourceGroupManager::getSingleton().initialiseResourceGroup("Bootstrap");
			ResourceGroupManager::getSingleton().initialiseResourceGroup("Main");
#endif
}

void LKernel::shutdownOgre ()
{
	for (auto obj : details::globalObjects)
		delete obj.second;
	details::globalObjects.clear();

	delete gRoot;
	gRoot = nullptr;

	auto context = SDL_GL_GetCurrentContext();
	SDL_GL_MakeCurrent(gSDLWindow, NULL);
	SDL_GL_DeleteContext(context);

	SDL_DestroyWindow(gSDLWindow);
	gSDLWindow = NULL;

	SDL_Quit();
}

void LKernel::log(const std::string& message)
{
	Ogre::LogManager::getSingleton().logMessage(message);
}
