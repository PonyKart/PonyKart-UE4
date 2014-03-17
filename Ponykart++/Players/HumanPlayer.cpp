#include "pch.h"
#include "Actors/Wheels/DriftState.h"
#include "Core/KeyBindingManager.h"
#include "Items/ItemManager.h"
#include "Kernel/LKernel.h"
#include "Players/HumanPlayer.h"
#include "UI/GameUIManager.h"

using namespace std;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Items;
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
	bindings->pressEventsMap[LKey::Accelerate] = bind(&HumanPlayer::onStartAccelerate, this);
	bindings->releaseEventsMap[LKey::Accelerate] = bind(&HumanPlayer::onStartAccelerate, this);
	bindings->pressEventsMap[LKey::Drift] = bind(&HumanPlayer::onStartDrift, this);
	bindings->releaseEventsMap[LKey::Drift] = bind(&HumanPlayer::onStopDrift, this);
	bindings->pressEventsMap[LKey::Reverse] = bind(&HumanPlayer::onStartReverse, this);
	bindings->releaseEventsMap[LKey::Reverse] = bind(&HumanPlayer::onStopReverse, this);
	bindings->pressEventsMap[LKey::TurnLeft] = bind(&HumanPlayer::onStartTurnLeft, this);
	bindings->releaseEventsMap[LKey::TurnLeft] = bind(&HumanPlayer::onStopTurnLeft, this);
	bindings->pressEventsMap[LKey::TurnRight] = bind(&HumanPlayer::onStartTurnRight, this);
	bindings->releaseEventsMap[LKey::TurnRight] = bind(&HumanPlayer::onStopTurnRight, this);
	bindings->pressEventsMap[LKey::Item] = bind(&HumanPlayer::useItem, this);
	//bindings->releaseEventsMap[LKey::Item] = ; Might need this later
	//bindings->axisEvents[LKey::SteeringAxis] = onSteeringAxisMoved;
	//bindings->axisEvents[LKey::AccelerateAxis] = onAccelerateAxisMoved;
}

void HumanPlayer::useItem()
{
	if (hasItem)
	{
		LKernel::getG<UI::GameUIManager>()->setItemLevel(0);
		LKernel::getG<UI::GameUIManager>()->setItemImage("none");
		LKernel::getG<ItemManager>()->spawnItem(this, heldItem);
	}
	else
	{
		//Ogre::Vector3 pos = kart->getActualPosition();
		//pos.y += 5;
		//LKernel::getG<ItemManager>()->requestBox(pos);
	}
	hasItem = false;
}

void HumanPlayer::onStartAccelerate() 
{
	Player::onStartAccelerate();

	if (isControlEnabled) 
	{
		// if we have both forward and reverse pressed at the same time, do nothing
		if (bindings->isKeyPressed(LKey::Reverse))
			kart->setAcceleration(0.f);
		// otherwise go forwards as normal
		else
			kart->setAcceleration(1.f);
	}
}

void HumanPlayer::onStopTurnRight()
{
	Player::onStopTurnRight();

	if (isControlEnabled) 
	{
		// if left is still pressed, turn left
		if (bindings->isKeyPressed(LKey::TurnLeft))
			kart->setTurnMultiplier(1.f);
		// otherwise go straight
		else
			kart->setTurnMultiplier(0.f);
	}
}

void HumanPlayer::onStopTurnLeft()
{
	Player::onStopTurnLeft();

	if (isControlEnabled) 
	{
		// if right is still pressed, turn right
		if (bindings->isKeyPressed(LKey::TurnRight))
			kart->setTurnMultiplier(-1.f);
		// otherwise go straight
		else
			kart->setTurnMultiplier(0.f);
	}
}

void HumanPlayer::onStopReverse() 
{
	Player::onStopReverse();

	if (isControlEnabled) 
	{
		// if forward is still held down, then we start going forwards
		if (bindings->isKeyPressed(LKey::Accelerate))
			kart->setAcceleration(1.f);
		// otherwise we just stop accelerating
		else
			kart->setAcceleration(0.f);
	}
}

/// Cancel the drift
void HumanPlayer::onStopDrift() {
	Player::onStopDrift();

#ifdef DRIFTING_ENABLED
	if (isControlEnabled) 
	{
		// if we were drifting left
		if (kart->driftState == KartDriftState::FullLeft || kart->driftState == KartDriftState::StartLeft)
			kart->stopDrifting();
		// if we were drifting right
		else if (kart->driftState == KartDriftState::FullRight || kart->driftState == KartDriftState::StartRight)
			kart->stopDrifting();
		// if we had the drift button down but weren't actually drifting
		else if (kart->driftState == KartDriftState::WantsDriftingButNotTurning)
			kart->driftState = KartDriftState::None;
	}
#endif
}

void HumanPlayer::onStopAccelerate() 
{
	Player::onStopAccelerate();

	if (isControlEnabled) 
	{
		// if reverse is still held down, then we start reversing
		if (bindings->isKeyPressed(LKey::Reverse))
			kart->setAcceleration(-1.f);
		// otherwise we just stop accelerating
		else
			kart->setAcceleration(0.f);
	}
}

void HumanPlayer::onStartTurnRight() 
{
	Player::onStartTurnRight();

	if (isControlEnabled) 
	{
#ifdef DRIFTING_ENABLED
		if (kart->driftState == KartDriftState::WantsDriftingButNotTurning)
			kart->startDrifting(KartDriftState::StartLeft);
		// normal steering
		else
#endif
		{
			// if both turns are pressed, we go straight
			if (bindings->isKeyPressed(LKey::TurnLeft))
				kart->setTurnMultiplier(0.f);
			// otherwise go right
			else
				kart->setTurnMultiplier(-1.f);
		}
	}
}

void HumanPlayer::onStartTurnLeft() {
	Player::onStartTurnLeft();

	if (isControlEnabled) 
	{
#ifdef DRIFTING_ENABLED
		// if we're waiting to drift
		if (kart->driftState == KartDriftState::WantsDriftingButNotTurning)
			kart->startDrifting(KartDriftState::StartRight);
		// normal steering
		else
#endif
		{
			// if both turns are pressed, we go straight
			if (bindings->isKeyPressed(LKey::TurnRight))
				kart->setTurnMultiplier(0.f);
			// otherwise go left
			else
				kart->setTurnMultiplier(1.f);
		}
	}
}

void HumanPlayer::onStartReverse() 
{
	Player::onStartReverse();

	if (isControlEnabled) 
	{
		// if we have both forward and reverse pressed at the same time, do nothing
		if (bindings->isKeyPressed(LKey::Accelerate))
			kart->setAcceleration(0.f);
		// otherwise go forwards as normal
		else
			kart->setAcceleration(-1.f);
	}
}

void HumanPlayer::onStartDrift() 
{
	Player::onStartDrift();

#ifdef DRIFTING_ENABLED
	if (isControlEnabled) 
	{
		// if left is pressed and right isn't, start drifting left
		if (bindings->isKeyPressed(LKey::TurnLeft) && !bindings->isKeyPressed(LKey::TurnRight))
			kart->startDrifting(KartDriftState::StartRight);
		// otherwise if right is pressed and left isn't, start drifting right
		else if (bindings->isKeyPressed(LKey::TurnRight) && !bindings->isKeyPressed(LKey::TurnLeft))
			kart->startDrifting(KartDriftState::StartLeft);
		// otherwise it wants to drift but we don't have a direction yet
		else if (kart->getVehicleSpeed() > 20.f)
			kart->driftState = KartDriftState::WantsDriftingButNotTurning;
	}
#endif
}
