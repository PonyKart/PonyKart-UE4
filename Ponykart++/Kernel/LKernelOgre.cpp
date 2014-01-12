#include <string>
#include <OgreConfigFile.h>
#include <OgreMeshManager.h>
#include <OgreSkeletonManager.h>
#include <OgreRenderWindow.h>
#include <OgreRoot.h>
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Core/Options.h"

using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ogre;

void LKernel::initOgreRoot()
{
#ifdef DEBUG
	gRoot = new Ogre::Root("media_debug/config/plugins.cfg", "", "Ponykart.log");
#else
	gRoot = new Ogre::Root("media/config/plugins.cfg", "", "Ponykart.log");
#endif
	addGlobalObject(gRoot);
}

void LKernel::initOgreRenderSystem()
{
	//gRenderSystem = gRoot->getRenderSystemByName("Direct3D9 Rendering Subsystem");
	gRenderSystem = gRoot->getRenderSystemByName("OpenGL Rendering Subsystem");
	gRenderSystem->setConfigOption("Full Screen", Options::get("Full Screen"));
	gRenderSystem->setConfigOption("VSync", Options::get("VSync"));
	gRenderSystem->setConfigOption("VSync Interval", Options::get("VSync Interval"));
	gRenderSystem->setConfigOption("FSAA", Options::get("FSAA"));
	gRenderSystem->setConfigOption("Video Mode", Options::get("Video Mode"));
	gRenderSystem->setConfigOption("sRGB Gamma Conversion", Options::get("sRGB Gamma Conversion"));
	gRoot->setRenderSystem(gRenderSystem); // Add to global objects
	addGlobalObject(gRenderSystem);
#if DEBUG
	// print out the things we can support
	auto renderList = gRoot->getAvailableRenderers();
	for (auto renderSystem : renderList)
	{
		log("\n**** Available options for Render System: " + renderSystem->getName() + " ****");
		for (auto option : renderSystem->getConfigOptions())
		{
			log("\t" + option.first);
			for (auto p : option.second.possibleValues)
				log("\t\t" + p);
		}
		log("***********************************");
	}
#endif
}

void LKernel::initOgreRenderWindow()
{
	gWindow = gRoot->initialise(true, "Ponykart");
	gWindow->setDeactivateOnFocusChange(false);
	addGlobalObject(gWindow);
}

void LKernel::initOgreSceneManager()
{
	gSceneManager = gRoot->createSceneManager("OctreeSceneManager","sceneMgr");
	addGlobalObject(gSceneManager);
}

void LKernel::initOgreViewportCam()
{
	gViewport = gWindow->addViewport(gSceneManager->createCamera("tempCam"));
	addGlobalObject(gViewport);
}

void LKernel::details::initOgreResources()
{
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

void LKernel::log(const std::string& message)
{
	Ogre::LogManager::getSingleton().logMessage(message);
}