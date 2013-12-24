#include "Kernel/LKernel.h"
#include "Core/KeyBindingManager.h"
#include "Core/InputMain.h"
#include "Core/Options.h"
#include "Core/InputSwallowerManager.h"

using namespace std;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;

// Define our static members
std::map<LKey, OIS::KeyCode> KeyBindingManager::LKeysDict;
std::map<OIS::KeyCode, LKey> KeyBindingManager::OISKeysDict;
std::map<ControllerButtons, LKey> KeyBindingManager::LButtonsDict;
std::map<ControllerAxis, LKey> KeyBindingManager::LAxisDict;
std::map<LKey, std::function<void ()>> KeyBindingManager::PressEventsDict;
std::map<LKey, std::function<void ()>> KeyBindingManager::ReleaseEventsDict;
std::map<LKey, std::function<void ()>> KeyBindingManager::AxisEvents;

KeyBindingManager::KeyBindingManager()
{
	log("[Loading] Creating KeyBindingManager...");

	setupInitialBindings();

	// TODO: Replace the LymphInputEvents with std::functions

	auto input = LKernel::getG<InputMain>();
	input->OnKeyboardPress_Anything.push_back(onKeyboardPressAnything);
	input->OnKeyboardRelease_Anything.push_back(onKeyboardReleaseAnything);
	input->OnLeftXAxisMoved.push_back(input_OnLeftXAxisMoved);

	if (Options::getBool("Twh"))
	{
		input->OnMousePress_Anything.push_back(onMousePress_Anything);
		input->OnMouseRelease_Anything.push_back(onMouseRelease_Anything);
	}
}

// TODO read these from a file
void KeyBindingManager::setupInitialBindings()
{
	LKeysDict[LKey::Accelerate] = OIS::KeyCode::KC_W;
	LKeysDict[LKey::TurnLeft] = OIS::KeyCode::KC_A;
	LKeysDict[LKey::TurnRight] = OIS::KeyCode::KC_D;
	LKeysDict[LKey::Drift] = OIS::KeyCode::KC_SPACE;
	LKeysDict[LKey::Reverse] = OIS::KeyCode::KC_S;
	LKeysDict[LKey::Item] = OIS::KeyCode::KC_LSHIFT;
	OISKeysDict[OIS::KeyCode::KC_W] = LKey::Accelerate;
	OISKeysDict[OIS::KeyCode::KC_A] = LKey::TurnLeft;
	OISKeysDict[OIS::KeyCode::KC_D] = LKey::TurnRight;
	OISKeysDict[OIS::KeyCode::KC_SPACE] = LKey::Drift;
	OISKeysDict[OIS::KeyCode::KC_S] = LKey::Reverse;
	OISKeysDict[OIS::KeyCode::KC_LSHIFT] = LKey::Item;
	LButtonsDict[ControllerButtons::A] = LKey::Drift;
	LAxisDict[ControllerAxis::LeftX] = LKey::SteeringAxis;

	PressEventsDict.insert(pair<LKey,function<void()>>(LKey::Accelerate, function<void ()>()));
	PressEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnLeft, function<void ()>()));
	PressEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnRight, function<void ()>()));
	PressEventsDict.insert(pair<LKey,function<void()>>(LKey::Drift, function<void ()>()));
	PressEventsDict.insert(pair<LKey,function<void()>>(LKey::Reverse, function<void ()>()));
	PressEventsDict.insert(pair<LKey,function<void()>>(LKey::Item, function<void ()>()));

	ReleaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Accelerate, function<void ()>()));
	ReleaseEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnLeft, function<void ()>()));
	ReleaseEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnRight, function<void ()>()));
	ReleaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Drift, function<void ()>()));
	ReleaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Reverse, function<void ()>()));
	ReleaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Item, function<void ()>()));
}

void KeyBindingManager::onKeyboardPressAnything(OIS::KeyEvent ke)
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	LKey key;
	if (OISKeysDict.find(ke.key) != OISKeysDict.end()) // TODO: Check if this is correct. I'm not sure of the translation.
		invoke(PressEventsDict[key]);
}

void KeyBindingManager::onKeyboardReleaseAnything(OIS::KeyEvent ke)
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	LKey key;
	if (OISKeysDict.find(ke.key) != OISKeysDict.end()) // TODO: Check if this is correct. I'm not sure of the translation.
		invoke(ReleaseEventsDict[key]);
}

void KeyBindingManager::input_OnLeftXAxisMoved(void* sender, Core::ControllerAxisArgument e)
{
	if ( LKernel::getG<InputSwallowerManager>()->isSwallowed( ) )
		return;

	invoke( AxisEvents[LAxisDict[e.Axis]] );
}

void KeyBindingManager::onMousePress_Anything(OIS::MouseEvent e, OIS::MouseButtonID id)
{
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	switch (id)
	{
		case OIS::MouseButtonID::MB_Left:
			invoke(PressEventsDict[LKey::Accelerate]); break;
		case OIS::MouseButtonID::MB_Button3:
			invoke(PressEventsDict[LKey::TurnLeft]); break;
		case OIS::MouseButtonID::MB_Button4:
			invoke(PressEventsDict[LKey::TurnRight]); break;
		case OIS::MouseButtonID::MB_Middle:
			invoke(PressEventsDict[LKey::Drift]); break;
		default:;
	}
}

void KeyBindingManager::onMouseRelease_Anything(OIS::MouseEvent e, OIS::MouseButtonID id)
{
	switch (id)
	{
	case OIS::MouseButtonID::MB_Left:
		invoke(ReleaseEventsDict[LKey::Accelerate]); break;
	case OIS::MouseButtonID::MB_Button3:
		invoke(ReleaseEventsDict[LKey::TurnLeft]); break;
	case OIS::MouseButtonID::MB_Button4:
		invoke(ReleaseEventsDict[LKey::TurnRight]); break;
	case OIS::MouseButtonID::MB_Middle:
		invoke(ReleaseEventsDict[LKey::Drift]); break;
	default:;
	}
}

void KeyBindingManager::invoke(std::function<void ()> e)
{
	if (e)
		e();
}
