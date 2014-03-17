#include "pch.h"
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
using namespace Ponykart;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;

InputMain::InputMain()
{
	log("[Loading] Initialising SDL2 input system");

//	OIS::ParamList pl = OIS::ParamList();
//	int windowHnd = 0;
//	LKernel::getG<Ogre::RenderWindow>()->getCustomAttribute("WINDOW", &windowHnd); // window is your RenderWindow!
//	ostringstream oss; oss << windowHnd;
//	pl.insert(std::pair<std::string,std::string> ("WINDOW", oss.str())); // TODO: Find another way to give the window to OIS. It needs to be portable.

//	// this stops OIS from swallowing the mouse
//#if defined OIS_LINUX_PLATFORM
//	pl.insert(make_pair(string("x11_mouse_grab"), std::string("false")));
//	pl.insert(make_pair(string("x11_mouse_hide"), std::string("false")));
//#elif defined OIS_WIN32_PLATFORM
//	pl.insert(make_pair(string("w32_mouse"), string("DISCL_NONEXCLUSIVE")));
//	pl.insert(make_pair(string("w32_mouse"), string("DISCL_FOREGROUND")));
//#endif

//	inputManager = OIS::InputManager::createInputSystem(pl);

//	// Create all devices (except joystick, as most people have Keyboard/Mouse) using buffered input.
//	inputKeyboard = (OIS::Keyboard*) inputManager->createInputObject(OIS::Type::OISKeyboard, true);
//	inputMouse = (OIS::Mouse*) inputManager->createInputObject(OIS::Type::OISMouse, true);
//	inputController = new Core::ControllerManager();

//	// sets the mouseState initial width and height (default is too low)
//	const OIS::MouseState& mouseState = inputMouse->getMouseState(); // TODO: Find the OIS equivalent of mouseState.height and width
//	mouseState.width = LKernel::getG<Ogre::Viewport>()->getActualWidth();
//	mouseState.height = LKernel::getG<Ogre::Viewport>()->getActualHeight();

	LKernel::gRoot->addFrameListener(this);  // TODO: ASAP! Implement this frame listener! It's crashing without it.

	createEventHandlers(); // TODO: Implement

	log("[Loading] SDL2 input system loaded.");
}

void InputMain::createEventHandlers()
{
}

template<typename T> void InputMain::fireEvent(LymphInputEvent1<T> handler, const T &eventArgs)
{
	if (handler.size())
		for (auto fun : handler)
			fun(eventArgs);
}

//template<typename T, typename U> void InputMain::fireEvent(LymphInputEvent2<T,U> handler, const T &eventArg1, const U &eventArg2)
//{
//	if (handler.size())
//		for (auto fun : handler)
//			fun(eventArg1, eventArg2);
//}

bool InputMain::keyPressed(const SDL_KeyboardEvent &ke)
{
#if PRINTINPUT
	cout << "Pressed: " << ke.key;
#endif
	switch (ke.keysym.sym)
	{
		case SDLK_ESCAPE:
			fireEvent<SDL_KeyboardEvent>(onKeyboardPress_Escape, ke); break;
		default:
			break;
	}
	fireEvent<SDL_KeyboardEvent>(onKeyboardPress_Anything, ke);
	return true;
}

bool InputMain::keyReleased(const SDL_KeyboardEvent &ke)
{
#if PRINTINPUT
	cout << "Released: " << ke.key;
#endif
	fireEvent<SDL_KeyboardEvent>(onKeyboardRelease_Anything, ke);
	return true;
}

bool InputMain::mousePressed(const SDL_MouseButtonEvent &mbe)
{
#if PRINTINPUT
	cout << "Mouse " << id << " pressed";
#endif
	fireEvent<SDL_MouseButtonEvent>(onMousePress_Anything, mbe);

	switch (mbe.button)
	{
		case SDL_BUTTON_LEFT:
			fireEvent<SDL_MouseButtonEvent>(onMousePress_Left, mbe); break;
		case SDL_BUTTON_RIGHT:
			fireEvent<SDL_MouseButtonEvent>(onMousePress_Right, mbe); break;
		case SDL_BUTTON_MIDDLE:
			fireEvent<SDL_MouseButtonEvent>(onMousePress_Middle, mbe); break;
		default:
			break;
	}
	return true;
}

bool InputMain::mouseReleased(const SDL_MouseButtonEvent &mbe)
{
#if PRINTINPUT
	cout << "Mouse " << id << " released";
#endif
	fireEvent<SDL_MouseButtonEvent>(onMouseRelease_Anything, mbe);

	switch (mbe.button)
	{
		case SDL_BUTTON_LEFT:
			fireEvent<SDL_MouseButtonEvent>(onMouseRelease_Left, mbe); break;
		case SDL_BUTTON_RIGHT:
			fireEvent<SDL_MouseButtonEvent>(onMouseRelease_Right, mbe); break;
		case SDL_BUTTON_MIDDLE:
			fireEvent<SDL_MouseButtonEvent>(onMouseRelease_Middle, mbe); break;
		default:
			break;
	}
	return true;
}

/** scroll wheel counts as a movement rather than a press **/
bool InputMain::mouseMoved(const SDL_MouseMotionEvent &mme)
{
	// you can use handler.state.Y.rel for relative position, and handler.state.Y.abs for absolute
#if PRINTINPUT
	cout << "Mouse moved: x " << me.state.X.rel << " | y " << me.state.Y.rel);
#endif
	fireEvent<SDL_MouseMotionEvent>(onMouseMove, mme);
	return true;
}

bool InputMain::frameStarted(const Ogre::FrameEvent &evt)
{
	if (!LKernel::getG<LevelManager>()->getIsValidLevel())
		return true;
	
	//timeSinceLastFrame += e.timeSinceLastFrame;
	//if (timeSinceLastFrame >= _inputCaptureRate) {
	// Capture all key presses since last check.
	//inputKeyboard->capture();
	// Capture all mouse movements and button presses since last check.
	//inputMouse->capture();
	//	timeSinceLastFrame -= _inputCaptureRate;
	//}

	return true;
}

OIS::Keyboard* InputMain::getInputKeyboard()
{
	return inputKeyboard;
}
