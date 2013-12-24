#include <string>
#include <sstream>
#include "Kernel/LKernel.h"
#include "Core/InputMain.h"

using namespace std;
using namespace Ponykart;
using namespace Ponykart::LKernel;

InputMain::InputMain()
{
	log("[Loading] Initialising OIS input system");

	OIS::ParamList* pl = new OIS::ParamList();
	int* windowHnd=new int;
	LKernel::getG<Ogre::RenderWindow>()->getCustomAttribute("WINDOW", windowHnd); // window is your RenderWindow!
	ostringstream oss; oss << *windowHnd;
	pl->insert(std::pair<std::string,std::string> ("WINDOW", oss.str())); // TODO: Find another way to give the window to OIS. It needs to be portable.

	//#if DEBUG
	// this stops OIS from swallowing the mouse
	pl->insert(std::pair<std::string,std::string> ("w32_mouse", "DISCL_NONEXCLUSIVE"));
	pl->insert(std::pair<std::string,std::string> ("w32_mouse", "DISCL_FOREGROUND"));
	//#endif

	inputManager = OIS::InputManager::createInputSystem(*pl);

	// Create all devices (except joystick, as most people have Keyboard/Mouse) using buffered input.
	inputKeyboard = (OIS::Keyboard*) inputManager->createInputObject(OIS::Type::OISKeyboard, true);
	inputMouse = (OIS::Mouse*) inputManager->createInputObject(OIS::Type::OISMouse, true);
	inputController = new Core::ControllerManager();

	// sets the mouseState initial width and height (default is too low)
	OIS::MouseState mouseState = inputMouse->getMouseState(); // TODO: Find the OIS equivalent of mouseState.height and width
	mouseState.width = LKernel::getG<Ogre::Viewport>()->getActualWidth();
	mouseState.height = LKernel::getG<Ogre::Viewport>()->getActualHeight();

	LKernel::gRoot->addFrameListener(frameStarted);  // TODO: Implement this frame listener

	createEventHandlers(); // TODO: Implement

	log("[Loading] OIS input system loaded!");
}

/*
void InputMain::CreateEventHandlers()
{
	if (InputKeyboard)
	{
		log("[Loading] Setting up keyboard listeners");
		InputKeyboard.KeyPressed += new KeyListener.KeyPressedHandler(KeyPressed);
		InputKeyboard.KeyReleased += new KeyListener.KeyReleasedHandler(KeyReleased);
	}
	if (InputMouse)
	{
		log("[Loading] Setting up mouse listeners");
		InputMouse.MousePressed += new MouseListener.MousePressedHandler(MousePressed);
		InputMouse.MouseReleased += new MouseListener.MouseReleasedHandler(MouseReleased);
		InputMouse.MouseMoved += new MouseListener.MouseMovedHandler(MouseMotion);
	}
	if (InputController)
	{
		log( "[Loading] Setting up joypad listeners" );
		// TODO: Implement joypad listeners. Those weren't implemented in Ponykart.
	}
}
*/
