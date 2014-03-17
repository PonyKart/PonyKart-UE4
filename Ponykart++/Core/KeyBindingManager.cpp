#include "pch.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Core/KeyBindingManager.h"
#include "Core/InputMain.h"
#include "Core/Options.h"
#include "Core/InputSwallowerManager.h"

using namespace std;
using namespace Ponykart::Core;
using namespace Ponykart::LKernel;

// Define our static members
std::map<LKey, SDL_Keycode> KeyBindingManager::lKeysMap;
std::map<SDL_Keycode, LKey> KeyBindingManager::oisKeysMap;
std::map<ControllerButtons, LKey> KeyBindingManager::lButtonsMap;
std::map<ControllerAxis, LKey> KeyBindingManager::lAxisMap;
std::map<LKey, std::function<void ()>> KeyBindingManager::pressEventsMap;
std::map<LKey, std::function<void ()>> KeyBindingManager::releaseEventsMap;
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
	lKeysMap[LKey::Accelerate] = SDLK_w;
	lKeysMap[LKey::TurnLeft] = SDLK_a;
	lKeysMap[LKey::TurnRight] = SDLK_d;
	lKeysMap[LKey::Drift] = SDLK_SPACE;
	lKeysMap[LKey::Reverse] = SDLK_s;
	lKeysMap[LKey::Item] = SDLK_LSHIFT;
	oisKeysMap[SDLK_w] = LKey::Accelerate;
	oisKeysMap[SDLK_a] = LKey::TurnLeft;
	oisKeysMap[SDLK_d] = LKey::TurnRight;
	oisKeysMap[SDLK_SPACE] = LKey::Drift;
	oisKeysMap[SDLK_s] = LKey::Reverse;
	oisKeysMap[SDLK_LSHIFT] = LKey::Item;
	lButtonsMap[ControllerButtons::A] = LKey::Drift;
	lAxisMap[ControllerAxis::LeftX] = LKey::SteeringAxis;

	pressEventsMap.insert(pair<LKey,function<void()>>(LKey::Accelerate, function<void ()>()));
	pressEventsMap.insert(pair<LKey,function<void()>>(LKey::TurnLeft, function<void ()>()));
	pressEventsMap.insert(pair<LKey,function<void()>>(LKey::TurnRight, function<void ()>()));
	pressEventsMap.insert(pair<LKey,function<void()>>(LKey::Drift, function<void ()>()));
	pressEventsMap.insert(pair<LKey,function<void()>>(LKey::Reverse, function<void ()>()));
	pressEventsMap.insert(pair<LKey,function<void()>>(LKey::Item, function<void ()>()));

	releaseEventsMap.insert(pair<LKey,function<void()>>(LKey::Accelerate, function<void ()>()));
	releaseEventsMap.insert(pair<LKey,function<void()>>(LKey::TurnLeft, function<void ()>()));
	releaseEventsMap.insert(pair<LKey,function<void()>>(LKey::TurnRight, function<void ()>()));
	releaseEventsMap.insert(pair<LKey,function<void()>>(LKey::Drift, function<void ()>()));
	releaseEventsMap.insert(pair<LKey,function<void()>>(LKey::Reverse, function<void ()>()));
	releaseEventsMap.insert(pair<LKey,function<void()>>(LKey::Item, function<void ()>()));
}

void KeyBindingManager::onKeyboardPressAnything(const SDL_KeyboardEvent &ke)
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	LKey key;
	if (oisKeysMap.find(ke.keysym.scancode) != oisKeysMap.end()) // TODO: Check if this is correct. I'm not sure of the translation.
		invoke(pressEventsMap[key]);
}

void KeyBindingManager::onKeyboardReleaseAnything(const SDL_KeyboardEvent &ke)
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	LKey key;
	if (oisKeysMap.find(ke.keysym.scancode) != oisKeysMap.end()) // TODO: Check if this is correct. I'm not sure of the translation.
		invoke(releaseEventsMap[key]);
}

void KeyBindingManager::input_OnLeftXAxisMoved(void* sender, ControllerAxisArgument e)
{
	if ( LKernel::getG<InputSwallowerManager>()->isSwallowed( ) )
		return;

	invoke(axisEvents[lAxisMap[e.Axis]] );
}

void KeyBindingManager::onMousePress_Anything(const SDL_MouseButtonEvent &mbe)
{
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return;

	switch (mbe.button)
	{
		case SDL_BUTTON_LEFT:
			invoke(pressEventsMap[LKey::Accelerate]); break;
		case SDL_BUTTON_X1:
			invoke(pressEventsMap[LKey::TurnLeft]); break;
		case SDL_BUTTON_X2:
			invoke(pressEventsMap[LKey::TurnRight]); break;
		case SDL_BUTTON_MIDDLE:
			invoke(pressEventsMap[LKey::Drift]); break;
		default:;
	}
}

void KeyBindingManager::onMouseRelease_Anything(const SDL_MouseButtonEvent &mbe)
{
	switch (mbe.button)
	{
	case SDL_BUTTON_LEFT:
		invoke(releaseEventsMap[LKey::Accelerate]); break;
	case SDL_BUTTON_X1:
		invoke(releaseEventsMap[LKey::TurnLeft]); break;
	case SDL_BUTTON_X2:
		invoke(releaseEventsMap[LKey::TurnRight]); break;
	case SDL_BUTTON_MIDDLE:
		invoke(releaseEventsMap[LKey::Drift]); break;
	default:;
	}
}

void KeyBindingManager::invoke(std::function<void ()> e)
{
	if (e)
		e();
}

bool KeyBindingManager::isKeyPressed(LKey key) 
{
	// don't do anything if it's swallowed
	if (LKernel::getG<InputSwallowerManager>()->isSwallowed())
		return false;

	// TODO: BUG: This will fail horribly since the lKeysMap uses SDL keycodes and InputMain uses OIS keycodes
	return LKernel::getG<InputMain>()->getInputKeyboard()->isKeyDown((OIS::KeyCode)lKeysMap[key]);
}
