#ifndef INPUTMAIN_H_INCLUDED
#define INPUTMAIN_H_INCLUDED

#include <vector>
#include <functional>
#include <OIS.h>
#include "Core/ControllerManager.h"

//#define PRINTINPUT // uncomment if you want all input to be printed

namespace Ponykart
{
/* Class that handles all of the input. This acts as a "layer" between the input library and the rest of the program.
// This makes it easier to change libraries and stuff, since you'd only need to change this class instead of everything
// that uses input.
//
// This class does not do anything with the input besides fire off more events.
//
// Other classes (mostly handlers) should only use events fired off from this class and not ones fired off from the
// input library.
*/
class InputMain // TODO: Finish implementing InputMain
{
public:
	InputMain();
	// Getters
	const OIS::InputManager* const getInputManager();
	const OIS::Keyboard* const getInputKeyboard();
	const OIS::Mouse* const getInputMouse();
	const Core::ControllerManager* const getInputController();

private:
	void CreateEventHandlers();

private:
	OIS::InputManager* InputManager;
	OIS::Keyboard* InputKeyboard;
	OIS::Mouse* InputMouse;
	Core::ControllerManager* InputController;

public: // Events
	std::vector<std::function<void (OIS::KeyEvent)>> OnKeyboardPress_Anything; // When any keyboard button is pressed. This should eventually be removed once we know what all of the keys are.
	std::vector<std::function<void (OIS::KeyEvent)>> OnKeyboardRelease_Anything;

	std::vector<std::function<void (OIS::KeyEvent)>> OnKeyboardPress_Escape;

	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMousePress_Anything;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMouseRelease_Anything;

	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMousePress_Left;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMouseRelease_Left;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMousePress_Right;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMouseRelease_Right;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMousePress_Middle;
	std::vector<std::function<void (OIS::MouseEvent, OIS::MouseButtonID)>> OnMouseRelease_Middle;

	std::vector<std::function<void (OIS::MouseEvent)>> OnMouseMove;

	std::vector<std::function<void (void* sender, Core::ControllerAxisArgument e)>> OnLeftXAxisMoved;
};

} // Ponykart

#endif // INPUTMAIN_H_INCLUDED
