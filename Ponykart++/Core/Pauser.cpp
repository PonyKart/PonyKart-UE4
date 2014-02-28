#include "pch.h"
#include "Core/Pauser.h"
#include "Core/InputMain.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Core/InputSwallowerManager.h"

using namespace Ponykart::Core;
using namespace Ponykart::LKernel;
using namespace Ponykart::Levels;

// Define static members
bool Pauser::isPaused = false;
std::vector<std::function<void (PausingState state)>> Pauser::pauseEvent;

Pauser::Pauser()
{
	log("[Loading] Creating Pauser");

	typedef void(*ftype)(OIS::KeyEvent); // For disanbiguanting the overloads of invokePauseEvent

	// if we press `, then pause
	LKernel::getG<InputMain>()->onKeyboardPress_Anything.push_back(static_cast<ftype>(invokePauseEvent));
	LKernel::getG<InputSwallowerManager>()->addSwallower(&isPaused, this);
}

void Pauser::invokePauseEvent(OIS::KeyEvent ke)
{
	if (ke.key == OIS::KeyCode::KC_GRAVE)
		invokePauseEvent();
}

void Pauser::invokePauseEvent()
{
	if (LKernel::getG<LevelManager>()->isPlayableLevel())
		pauseWithEvent();
}

void Pauser::pauseWithEvent()
{
	log("Pause!");
	isPaused = !isPaused;
	if (pauseEvent.size())
	{
		if (isPaused)
			for (auto event : pauseEvent)
				event(PausingState::Pausing);
		else
			for (auto event : pauseEvent)
				event(PausingState::Unpausing);
	}
}
