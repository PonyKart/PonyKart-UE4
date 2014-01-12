#include <string>
#include <sstream>
#include <OgreRenderWindow.h>
#include <OgreRoot.h>
#include <OgreViewport.h>
#include "Core/InputMain.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"

using namespace std;
using namespace OIS;
using namespace Ponykart;
using namespace Ponykart::Levels;
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
	const OIS::MouseState& mouseState = inputMouse->getMouseState(); // TODO: Find the OIS equivalent of mouseState.height and width
	mouseState.width = LKernel::getG<Ogre::Viewport>()->getActualWidth();
	mouseState.height = LKernel::getG<Ogre::Viewport>()->getActualHeight();

	LKernel::gRoot->addFrameListener(this);  // TODO: ASAP! Implement this frame listener! It's crashing without it.

	createEventHandlers(); // TODO: Implement

	log("[Loading] OIS input system loaded!");
}

void InputMain::createEventHandlers()
{
	if (inputKeyboard)
	{
		log("[Loading] Setting up keyboard listeners");
		inputKeyboard->setEventCallback(this);
	}
	if (inputMouse)
	{
		log("[Loading] Setting up mouse listeners");
		inputMouse->setEventCallback(this);
	}
	if (inputController)
	{
		log( "[Loading] Setting up joypad listeners" );
		// TODO: Implement joypad listeners. Those weren't implemented in Ponykart.
	}
}

template<typename T> void InputMain::fireEvent(LymphInputEvent1<T> handler, T eventArgs)
{
	if (handler.size())
		for (auto fun : handler)
			fun(eventArgs);
}

template<typename T, typename U> void InputMain::fireEvent(LymphInputEvent2<T,U> handler, T eventArg1, U eventArg2)
{
	if (handler.size())
		for (auto fun : handler)
			fun(eventArg1, eventArg2);
}

bool InputMain::keyPressed(const OIS::KeyEvent& ke)
{
#if PRINTINPUT
	cout << "Pressed: " << ke.key;
#endif
	switch (ke.key)
	{
		case KeyCode::KC_ESCAPE:
			fireEvent<KeyEvent>(onKeyboardPress_Escape, ke); break;
		default:
			break;
	}
	fireEvent<KeyEvent>(onKeyboardPress_Anything, ke);
	return true;
}

bool InputMain::keyReleased(const OIS::KeyEvent& ke)
{
#if PRINTINPUT
	cout << "Released: " << ke.key;
#endif
	fireEvent<KeyEvent>(onKeyboardRelease_Anything, ke);
	return true;
}

bool InputMain::mousePressed(const OIS::MouseEvent& me, OIS::MouseButtonID id)
{
#if PRINTINPUT
	cout << "Mouse " << id << " pressed";
#endif
	fireEvent<MouseEvent, MouseButtonID>(onMousePress_Anything, me, id);

	switch (id)
	{
		case MouseButtonID::MB_Left:
			fireEvent<MouseEvent, MouseButtonID>(onMousePress_Left, me, id); break;
		case MouseButtonID::MB_Right:
			fireEvent<MouseEvent, MouseButtonID>(onMousePress_Right, me, id); break;
		case MouseButtonID::MB_Middle:
			fireEvent<MouseEvent, MouseButtonID>(onMousePress_Middle, me, id); break;
		default:
			break;
	}
	return true;
}

bool InputMain::mouseReleased(const OIS::MouseEvent& me, OIS::MouseButtonID id)
{
#if PRINTINPUT
	cout << "Mouse " << id << " released";
#endif
	fireEvent<MouseEvent, MouseButtonID>(onMouseRelease_Anything, me, id);

	switch (id)
	{
		case MouseButtonID::MB_Left:
			fireEvent<MouseEvent, MouseButtonID>(onMouseRelease_Left, me, id); break;
		case MouseButtonID::MB_Right:
			fireEvent<MouseEvent, MouseButtonID>(onMouseRelease_Right, me, id); break;
		case MouseButtonID::MB_Middle:
			fireEvent<MouseEvent, MouseButtonID>(onMouseRelease_Middle, me, id); break;
		default:
			break;
	}
	return true;
}

/** scroll wheel counts as a movement rather than a press **/
bool InputMain::mouseMoved(const MouseEvent& me)
{
	// you can use handler.state.Y.rel for relative position, and handler.state.Y.abs for absolute
#if PRINTINPUT
	cout << "Mouse moved: x " << me.state.X.rel << " | y " << me.state.Y.rel);
#endif
	fireEvent<MouseEvent>(onMouseMove, me);
	return true;
}

bool InputMain::frameStarted(const Ogre::FrameEvent& evt)
{
	if (!LKernel::getG<LevelManager>()->getIsValidLevel())
		return true;
	
	//timeSinceLastFrame += e.timeSinceLastFrame;
	//if (timeSinceLastFrame >= _inputCaptureRate) {
	// Capture all key presses since last check.
	inputKeyboard->capture();
	// Capture all mouse movements and button presses since last check.
	inputMouse->capture();
	//	timeSinceLastFrame -= _inputCaptureRate;
	//}

	return true;
}