#include "pch.h"
#include <OgreSceneNode.h>
#include "Actors/Components/ModelComponent.h"
#include "Actors/LThing.h"
#include "Core/Spawner.h"
#include "Kernel/LKernel.h"
#include "Players/ComputerPlayer.h"

using namespace Ogre;
using namespace Ponykart::Actors;
using namespace Ponykart::Levels;
using namespace Ponykart::Players;

ComputerPlayer::ComputerPlayer(LevelChangedEventArgs* eventArgs, int id)
: Player(eventArgs,id,true)
{
	//LKernel.GetG<CollisionReporter>().AddEvent(PonykartCollisionGroups.Karts, PonykartCollisionGroups.Environment, OnCol);
#if DEBUG
	axis = LKernel::getG<Core::Spawner>()->spawn("Axis", kart->getRootNode()->getPosition());
	axis->getModelComponents()[0]->getNode()->setScale(0.1f, 0.1f, 0.1f);
	axis2 = LKernel::getG<Core::Spawner>()->spawn("Axis", kart->getRootNode()->getPosition());
	axis2->getModelComponents()[0]->getNode()->setScale(0.1f, 0.1f, 0.1f);
#endif

	LKernel::onEveryUnpausedTenthOfASecondEvent.push_back(std::bind(&ComputerPlayer::everyTenth,this,std::placeholders::_1));
}

void ComputerPlayer::everyTenth(void* o)
{
	Vector3 vecToTar = nextWaypoint - kart->getActualPosition();

	// not using Y so set it to 0
	vecToTar.y = 0;

	if (isControlEnabled) 
	{
		if (reverseCooldown <= 0)
		{
			direction = 1;
		}
		else
		{
			reverseCooldown--;
		}
		float steerFactor = steerTowards(vecToTar);
		kart->setTurnMultiplier(steerFactor * direction);
		kart->setAcceleration((1.0f - abs(steerFactor) + 0.15f));
		if (kart->getAcceleration() < 0.15f)
			kart->setAcceleration(kart->getAcceleration() + 0.25f);
		kart->setMaxReverseSpeed(kart->getAcceleration() * direction);
		//Enable this for hilarity
		//if (Kart.Acceleration > 0.5f && Kart.VehicleSpeed < 0.1f)
		//{
		//    Kart.Acceleration = -0.3f;
		//    Kart.TurnMultiplier = -1;
		//}

		//if (Kart.VehicleSpeed < 0.1f)
		//Kart.Acceleration = -1;e
	}
}

float ComputerPlayer::steerTowards(Ogre::Vector3 vecToTar)
{
	Vector3& xaxis = kart->getActualOrientation().xAxis();

	xaxis.normalise();
	vecToTar.normalise();

	float result = xaxis.dotProduct(vecToTar);

	if (result < -1)
		return -1;
	else if (result > 1)
		return 1;
	else
		return result;
}

void ComputerPlayer::useItem() 
{
	throw std::string("ComputerPlayer::useItem(): Not implemented. Wasn't in the C# either");
}
