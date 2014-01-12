#include <string>
#include <fstream>
#include "Core/Options.h"
#include "UI/Splash.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"

using namespace Ponykart::LKernel;
using Ponykart::Core::Options;
using Ponykart::Splash;

int main()
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

		Splash splash;

		loadInitialObjects(splash);

		//startRendering();

		log("End of code. Exited successfully.");
		return EXIT_SUCCESS;
	}
	catch (std::string e) // If you can't guarantee that someone will catch your exceptions, throw a string.
	{
		log("[EXCEPTION] " + e);
	}
	// TODO: Catch standard exceptions too. Log e.what()

	return EXIT_FAILURE; // If we're here, we came from a catch
}
