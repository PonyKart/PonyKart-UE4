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
	void createEventHandlers();

private:
	class FrameStartedListener : public Ogre::FrameListener
	{
		bool frameStarted(const Ogre::FrameEvent& evt) override;
	};
	FrameStartedListener* frameStarted;

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
