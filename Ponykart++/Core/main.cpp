#include "Core/main.h"

#include <string>
#include <fstream>
#include <cstdio>
#include <Ogre.h>
#include <OgreWindowEventUtilities.h>
#include "Core/Options.h"
#include "UI/Splash.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"

#include "Sound/SoundMain.h"
#include "Sound/Music/MusicSource.h"

using namespace Ponykart::Launch;
using namespace Ponykart::LKernel;
using Ponykart::Core::Options;
using Ponykart::Splash;


int main (int argc, char *argv[])
{
	try
	{
		initOgreRoot();
	}
	catch (std::bad_alloc e)
	{
		std::ofstream logfile;
		std::cerr << "Error initializing ogre. Are you using the correct DLLs ?";
		logfile.open("Ponykart.log", std::ios::out);
		if (logfile.is_open())
		{
			logfile.clear();
			logfile << "Error initializing ogre. Are you using the correct DLLs ?\n";
			logfile.close();
		}
		abort();
	}
	catch (Ogre::InternalErrorException e)
	{
		std::ofstream logfile;
		std::cerr << "Error initializing ogre : " << e.getDescription();
		logfile.open("Ponykart.log", std::ios::out);
		if (logfile.is_open())
		{
			logfile.clear();
			logfile  << "Error initializing ogre : " << e.getFullDescription();
			logfile.close();
		}
		abort();
	}
	
	try
	{
		log("[Loading] Loading configuration...");
		Options::initialize();

		log("[Loading] Loading the render system...");
		initOgreRenderSystem();

		log("[Loading] Creating the render window...");
		initOgreRenderWindow();

		log("Creating scene manager...");
		initOgreSceneManager();

		log("Creating player camera and viewport...");
		initOgreViewportCam();

		// Splash screen
		{
			Splash splash;
			loadInitialObjects(splash);
		}
		enterGameLoop();

		log("End of code. Shutting down...");
		shutdown();
		std::printf("Shutdown complete.\n");
		return EXIT_SUCCESS;
	}
	catch (std::string e) // If you can't guarantee that someone will catch your exceptions, throw a string.
	{
		log("[EXCEPTION] " + e);
	}
	// TODO: Catch standard exceptions too. Log e.what()

	log ("Exception thrown! Shutting down...");
	shutdown();
	std::printf("Post-exception shutdown complete.\n");
	return EXIT_FAILURE; // If we're here, we came from a catch
}


void Ponykart::Launch::enterGameLoop ()
{
	auto root = getG<Ogre::Root>();
	auto window = LKernel::getG<Ogre::RenderWindow>();

	auto soundMain = getG<Ponykart::Sound::SoundMain>();
	auto music = soundMain->PlayMusic("./media/music/Sweet Apple Acres 128bpm.ogg");

	while (!window->isClosed()) {
		root->renderOneFrame();

		Ogre::WindowEventUtilities::messagePump();

		for (auto &f : onEveryUnpausedTenthOfASecondEvent)
			f(nullptr);
	}
}
