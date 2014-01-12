#ifndef INPUTMAIN_H_INCLUDED
#define INPUTMAIN_H_INCLUDED

#include <vector>
#include <functional>
#include <OIS.h>
#include <OgreFrameListener.h>
#include "Core/ControllerManager.h"

//#define PRINTINPUT ///< uncomment if you want all input to be printed

namespace Ponykart
{
template<typename T> using LymphInputEvent1 = typename std::vector<std::function<void (T eventArgs)>>;
template<typename T, typename U> using LymphInputEvent2 = typename std::vector<std::function<void (T eventArg1, U eventArg2)>>;
using AxisMovedEventHandler = std::vector<std::function<void (void* sender, Core::ControllerAxisArgument e )>>;

/// Class that handles all of the input.
/**This acts as a "layer" between the input library and the rest of the program.
// This makes it easier to change libraries and stuff, since you'd only need to change this class instead of everything
// that uses input.
//
// This class does not do anything with the input besides fire off more events.
//
// Other classes (mostly handlers) should only use events fired off from this class and not ones fired off from the
// input library.
**/
class InputMain : public OIS::KeyListener, public OIS::MouseListener, public Ogre::FrameListener // TODO: Finish implementing InputMain
{
public:
	InputMain();
	// Getters
	const OIS::InputManager* const getInputManager();
	const OIS::Keyboard* const getInputKeyboard();
	const OIS::Mouse* const getInputMouse();
	const Core::ControllerManager* const getInputController();
	// Event handlers
	bool keyPressed(const OIS::KeyEvent& ke) override; ///< Handles key pressing and fires appropriate events
	bool keyReleased(const OIS::KeyEvent& ke) override; ///< Handles key releasing and fires appropriate events
	bool mousePressed(const OIS::MouseEvent& me, OIS::MouseButtonID id) override; ///< Handles mouse pressing and fires appropriate events
	bool mouseReleased(const OIS::MouseEvent& me, OIS::MouseButtonID id) override; ///< Handles mouse releasing and fires appropriate events
	bool mouseMoved(const OIS::MouseEvent& me) override; ///< Handles mouse movement and fires appropriate events

private:
	void createEventHandlers(); ///< Hook up to OIS's event handlers
	template<typename T> void fireEvent(LymphInputEvent1<T> handler, T eventArgs); ///< Fires an event. Helper method so we don't have to check every single event for null.
	template<typename T, typename U> void fireEvent(LymphInputEvent2<T,U> handler, T eventArg1, U eventArg2); ///< Fires an event. Helper method so we don't have to check every single event for null.

private:
	bool frameStarted(const Ogre::FrameEvent& evt) override;

private:
	OIS::InputManager* inputManager;
	OIS::Keyboard* inputKeyboard;
	OIS::Mouse* inputMouse;
	Core::ControllerManager* inputController;

public: // Events
	std::vector<std::function<void (OIS::KeyEvent)>> onKeyboardPress_Anything; ///< When any keyboard button is pressed. This should eventually be removed once we know what all of the keys are.
	std::vector<std::function<void (OIS::KeyEvent)>> onKeyboardRelease_Anything;

	std::vector<std::function<void (OIS::KeyEvent)>> onKeyboardPress_Escape;

	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMousePress_Anything;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMouseRelease_Anything;

	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMousePress_Left;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMouseRelease_Left;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMousePress_Right;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMouseRelease_Right;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMousePress_Middle;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> onMouseRelease_Middle;

	std::vector<std::function<void (OIS::MouseEvent)>> onMouseMove;

	std::vector<std::function<void (void* sender, Core::ControllerAxisArgument e)>> onLeftXAxisMoved;
};

} // Ponykart

#endif // INPUTMAIN_H_INCLUDED
