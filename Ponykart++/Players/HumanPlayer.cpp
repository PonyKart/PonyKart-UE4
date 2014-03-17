#include "pch.h"
#include "Core/KeyBindingManager.h"
#include "Kernel/LKernel.h"
#include "Players/HumanPlayer.h"
#include "UI/GameUIManager.h"

using namespace std;
using namespace Ponykart;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Players;

HumanPlayer::HumanPlayer(LevelChangedEventArgs* eventArgs, int id)
: Player(eventArgs, id, false)
{
	// hook up to input events
	bindings = LKernel::get<KeyBindingManager>();

	LKernel::getG<UI::GameUIManager>()->setItemLevel(0);

	// We're supposed to register to the events, not overwrite them. 
	// But I'm pretty sure we're the only one registering...right ?
	// Besides, that makes it possible to unregister : just assign nullptr.
	bindings->pressEventsDict[LKey::Accelerate] = bind(&HumanPlayer::onStartAccelerate, this);
	bindings->releaseEventsDict[LKey::Accelerate] = bind(&HumanPlayer::onStartAccelerate, this);
	bindings->pressEventsDict[LKey::Drift] = bind(&HumanPlayer::onStartDrift, this);
	bindings->releaseEventsDict[LKey::Drift] = bind(&HumanPlayer::onStopDrift, this);
	bindings->pressEventsDict[LKey::Reverse] = bind(&HumanPlayer::onStartReverse, this);
	bindings->releaseEventsDict[LKey::Reverse] = bind(&HumanPlayer::onStopReverse, this);
	bindings->pressEventsDict[LKey::TurnLeft] = bind(&HumanPlayer::onStartTurnLeft, this);
	bindings->releaseEventsDict[LKey::TurnLeft] = bind(&HumanPlayer::onStopTurnLeft, this);
	bindings->pressEventsDict[LKey::TurnRight] = bind(&HumanPlayer::onStartTurnRight, this);
	bindings->releaseEventsDict[LKey::TurnRight] = bind(&HumanPlayer::onStopTurnRight, this);
	bindings->pressEventsDict[LKey::Item] = bind(&HumanPlayer::useItem, this);
	//bindings->releaseEventsDict[LKey::Item] = ; Might need this later
	//bindings->axisEvents[LKey::SteeringAxis] = onSteeringAxisMoved;
	//bindings->axisEvents[LKey::AccelerateAxis] = onAccelerateAxisMoved;
}
