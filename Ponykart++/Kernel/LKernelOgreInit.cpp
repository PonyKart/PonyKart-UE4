#include <string>

#include "Kernel/LKernel.h"
#include "Core/Options.h"

using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ogre;

void LKernel::initOgreRoot()
{
	root = new Ogre::Root("media/config/plugins.cfg", "", "Ponykart.log");
	addGlobalObject(root);
}

void LKernel::initOgreRenderSystem()
{
	//renderSystem = root->getRenderSystemByName("Direct3D9 Rendering Subsystem");
	renderSystem = root->getRenderSystemByName("OpenGL Rendering Subsystem");
	renderSystem->setConfigOption("Full Screen", Options::get("Full Screen"));
	renderSystem->setConfigOption("VSync", Options::get("VSync"));
	renderSystem->setConfigOption("VSync Interval", Options::get("VSync Interval"));
	renderSystem->setConfigOption("FSAA", Options::get("FSAA"));
	renderSystem->setConfigOption("Video Mode", Options::get("Video Mode"));
	renderSystem->setConfigOption("sRGB Gamma Conversion", Options::get("sRGB Gamma Conversion"));
	root->setRenderSystem(renderSystem); // Add to global objects
	addGlobalObject(renderSystem);
#if DEBUG
	// print out the things we can support
	auto renderList = root->getAvailableRenderers();
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
	window = root->initialise(true, "Ponykart");
	window->setDeactivateOnFocusChange(false);
	addGlobalObject(window);
}

void LKernel::initOgreSceneManager()
{
	sceneManager = root->createSceneManager("OctreeSceneManager","sceneMgr");
	addGlobalObject(sceneManager);
}

void LKernel::initOgreViewportCam()
{
	viewport = window->addViewport(sceneManager->createCamera("tempCam"));
	addGlobalObject(viewport);
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
