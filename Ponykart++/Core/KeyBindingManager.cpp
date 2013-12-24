#include "Kernel/LKernel.h"
#include "Core/KeyBindingManager.h"
#include "Core/InputMain.h"
#include "Core/Options.h"
#include "Core/InputSwallowerManager.h"

using namespace std;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;

// Define our static members
std::map<LKey, OIS::KeyCode> KeyBindingManager::lKeysDict;
std::map<OIS::KeyCode, LKey> KeyBindingManager::oisKeysDict;
std::map<ControllerButtons, LKey> KeyBindingManager::lButtonsDict;
std::map<ControllerAxis, LKey> KeyBindingManager::lAxisDict;
std::map<LKey, std::function<void ()>> KeyBindingManager::pressEventsDict;
std::map<LKey, std::function<void ()>> KeyBindingManager::releaseEventsDict;
std::map<LKey, std::function<void ()>> KeyBindingManager::axisEvents;

KeyBindingManager::KeyBindingManager()
{
	log("[Loading] Creating KeyBindingManager...");

	setupInitialBindings();

	// TODO: Replace the LymphInputEvents with std::functions

	auto input = LKernel::getG<InputMain>();
	input->onKeyboardPress_Anything.push_back(onKeyboardPressAnything);
	input->onKeyboardRelease_Anything.push_back(onKeyboardReleaseAnything);
	input->onLeftXAxisMoved.push_back(input_OnLeftXAxisMoved);

	if (Options::getBool("Twh"))
	{
		input->onMousePress_Anything.push_back(onMousePress_Anything);
		input->onMouseRelease_Anything.push_back(onMouseRelease_Anything);
	}
}

// TODO read these from a file
void KeyBindingManager::setupInitialBindings()
{
	lKeysDict[LKey::Accelerate] = OIS::KeyCode::KC_W;
	lKeysDict[LKey::TurnLeft] = OIS::KeyCode::KC_A;
	lKeysDict[LKey::TurnRight] = OIS::KeyCode::KC_D;
	lKeysDict[LKey::Drift] = OIS::KeyCode::KC_SPACE;
	lKeysDict[LKey::Reverse] = OIS::KeyCode::KC_S;
	lKeysDict[LKey::Item] = OIS::KeyCode::KC_LSHIFT;
	oisKeysDict[OIS::KeyCode::KC_W] = LKey::Accelerate;
	oisKeysDict[OIS::KeyCode::KC_A] = LKey::TurnLeft;
	oisKeysDict[OIS::KeyCode::KC_D] = LKey::TurnRight;
	oisKeysDict[OIS::KeyCode::KC_SPACE] = LKey::Drift;
	oisKeysDict[OIS::KeyCode::KC_S] = LKey::Reverse;
	oisKeysDict[OIS::KeyCode::KC_LSHIFT] = LKey::Item;
	lButtonsDict[ControllerButtons::A] = LKey::Drift;
	lAxisDict[ControllerAxis::LeftX] = LKey::SteeringAxis;

	pressEventsDict.insert(pair<LKey,function<void()>>(LKey::Accelerate, function<void ()>()));
	pressEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnLeft, function<void ()>()));
	pressEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnRight, function<void ()>()));
	pressEventsDict.insert(pair<LKey,function<void()>>(LKey::Drift, function<void ()>()));
	pressEventsDict.insert(pair<LKey,function<void()>>(LKey::Reverse, function<void ()>()));
	pressEventsDict.insert(pair<LKey,function<void()>>(LKey::Item, function<void ()>()));

	releaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Accelerate, function<void ()>()));
	releaseEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnLeft, function<void ()>()));
	releaseEventsDict.insert(pair<LKey,function<void()>>(LKey::TurnRight, function<void ()>()));
	releaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Drift, function<void ()>()));
	releaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Reverse, function<void ()>()));
	releaseEventsDict.insert(pair<LKey,function<void()>>(LKey::Item, function<void ()>()));
}

void KeyBindingManager::onKeyboardPressAnything(OIS::KeyEvent ke)
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	LKey key;
	if (oisKeysDict.find(ke.key) != oisKeysDict.end()) // TODO: Check if this is correct. I'm not sure of the translation.
		invoke(pressEventsDict[key]);
}

void KeyBindingManager::onKeyboardReleaseAnything(OIS::KeyEvent ke)
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	LKey key;
	if (oisKeysDict.find(ke.key) != oisKeysDict.end()) // TODO: Check if this is correct. I'm not sure of the translation.
		invoke(releaseEventsDict[key]);
}

void KeyBindingManager::input_OnLeftXAxisMoved(void* sender, Core::ControllerAxisArgument e)
{
	if ( LKernel::getG<InputSwallowerManager>()->isSwallowed( ) )
		return;

	invoke(axisEvents[lAxisDict[e.Axis]] );
}

void KeyBindingManager::onMousePress_Anything(OIS::MouseEvent e, OIS::MouseButtonID id)
{
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	switch (id)
	{
		case OIS::MouseButtonID::MB_Left:
			invoke(pressEventsDict[LKey::Accelerate]); break;
		case OIS::MouseButtonID::MB_Button3:
			invoke(pressEventsDict[LKey::TurnLeft]); break;
		case OIS::MouseButtonID::MB_Button4:
			invoke(pressEventsDict[LKey::TurnRight]); break;
		case OIS::MouseButtonID::MB_Middle:
			invoke(pressEventsDict[LKey::Drift]); break;
		default:;
	}
}

void KeyBindingManager::onMouseRelease_Anything(OIS::MouseEvent e, OIS::MouseButtonID id)
{
	switch (id)
	{
	case OIS::MouseButtonID::MB_Left:
		invoke(releaseEventsDict[LKey::Accelerate]); break;
	case OIS::MouseButtonID::MB_Button3:
		invoke(releaseEventsDict[LKey::TurnLeft]); break;
	case OIS::MouseButtonID::MB_Button4:
		invoke(releaseEventsDict[LKey::TurnRight]); break;
	case OIS::MouseButtonID::MB_Middle:
		invoke(releaseEventsDict[LKey::Drift]); break;
	default:;
	}
}

void KeyBindingManager::invoke(std::function<void ()> e)
{
	if (e)
		e();
}
