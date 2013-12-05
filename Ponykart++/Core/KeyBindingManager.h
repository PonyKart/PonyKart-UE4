#ifndef KEYBINDINGMANAGER_H_INCLUDED
#define KEYBINDINGMANAGER_H_INCLUDED

#include <map>
#include <functional>
#include <OIS.h>
#include "Core/ControllerManager.h"

namespace Ponykart
{
namespace Core
{

// Our key commands - these are for things that need to be polled. If you want to just respond to events, use the ones in InputMain.
enum LKey
{
	Accelerate,
	TurnLeft,
	TurnRight,
	Drift,
	Reverse,
	SteeringAxis,
	AccelerateAxis,
	BrakeAxis,
	Item
};

// This class provides an interface between game commands (accelerate, etc) and key presses (WASD, etc).
// This way we can change which keys do things at runtime
class KeyBindingManager
{
public:
	KeyBindingManager();
	void setupInitialBindings(); // Set up some initial key bindings
	static void onKeyboardPressAnything(OIS::KeyEvent ke);
	static void onKeyboardReleaseAnything(OIS::KeyEvent ke);
	static void input_OnLeftXAxisMoved(void* sender, Core::ControllerAxisArgument e);
	static void onMousePress_Anything(OIS::MouseEvent e, OIS::MouseButtonID id);
	static void onMouseRelease_Anything(OIS::MouseEvent e, OIS::MouseButtonID id);
	static void invoke(std::function<void ()> e);
	// Getters
	const std::map<LKey, std::function<void ()>>& getPressEventsDict();
	const std::map<LKey, std::function<void ()>>& getReleaseEventsDict();
	const std::map<LKey, std::function<void ()>>& getAxisEvents();

private:
	// The maps that converts our key commands into OIS keys
	static std::map<LKey, OIS::KeyCode> LKeysDict;
	static std::map<OIS::KeyCode, LKey> OISKeysDict;
	static std::map<ControllerButtons, LKey> LButtonsDict;
	static std::map<ControllerAxis, LKey> LAxisDict;

	static std::map<LKey, std::function<void ()>> PressEventsDict;
	static std::map<LKey, std::function<void ()>> ReleaseEventsDict;
	static std::map<LKey, std::function<void ()>> AxisEvents;
};

} // Core
} // Ponykart

#endif // KEYBINDINGMANAGER_H_INCLUDED
