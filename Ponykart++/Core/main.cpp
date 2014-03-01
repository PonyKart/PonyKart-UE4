#include "pch.h"
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
#include "Misc/sdl2Extensions.h"

using namespace Ponykart::Launch;
using namespace Ponykart::LKernel;
using namespace Extensions;
using Ponykart::Core::Options;
using Ponykart::Splash;


int main (int argc, char *argv[])
{
	try
	{
		initOgreRoot();
	}
	catch (SDL2Exception e)
	{
		std::ofstream logfile;
		auto logStr = std::string("Error initializing SDL2: ") + e.what();
		std::cerr << logStr << std::endl;
		logfile.open("Ponykart.log", std::ios::out);
		if (logfile.is_open())
		{
			logfile.clear();
			logfile << logStr;
			logfile.close();
		}
		abort();
	}
	catch (std::bad_alloc e)
	{
		std::ofstream logfile;
		auto logStr = "Error initializing ogre. Are you using the correct DLLs ?";
		std::cerr << logStr << std::endl;
		logfile.open("Ponykart.log", std::ios::out);
		if (logfile.is_open())
		{
			logfile.clear();
			logfile << logStr << std::endl;
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
		Options::initialize();
		initOgreGraphics();

		// Splash screen
		{
			Splash splash;
			loadInitialObjects(splash);
		}
		enterGameLoop();

		log("End of code. Shutting down...");
		shutdownOgre();
		std::printf("Shutdown complete.\n");
		return EXIT_SUCCESS;
	}
	catch (std::string e) // If you can't guarantee that someone will catch your exceptions, throw a string.
	{
		log("[EXCEPTION] " + e);
	}
	// TODO: Catch standard exceptions too. Log e.what()

	log ("Exception thrown! Shutting down...");
	shutdownOgre();
	std::printf("Post-exception shutdown complete.\n");
	return EXIT_FAILURE; // If we're here, we came from a catch
}


void Ponykart::Launch::enterGameLoop ()
{
	auto ogreRoot = getG<Ogre::Root>();
	auto sdlWindow = getG<SDL_Window>();
	auto ogreWindow = getG<Ogre::RenderWindow>();

	auto soundMain = getG<Ponykart::Sound::SoundMain>();
	auto music = soundMain->PlayMusic("./media/music/Sweet Apple Acres 128bpm.ogg");

	while (!ogreWindow->isClosed()) {
		ogreRoot->_fireFrameStarted();
		ogreWindow->update(false);
		ogreRoot->_fireFrameRenderingQueued();
		ogreRoot->_fireFrameEnded();
		SDL_GL_SwapWindow(sdlWindow);

		SDL_Event event;
		while (SDL_PollEvent(&event)) {
			switch (event.type) {
			case SDL_QUIT:
				return;
			}
		}

		for (auto &f : onEveryUnpausedTenthOfASecondEvent)
			f(nullptr);
	}
}
