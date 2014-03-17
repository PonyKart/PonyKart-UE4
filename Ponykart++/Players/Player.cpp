#include "pch.h"
#include "Core/Settings.h"
#include "Core/Spawner.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Players/Player.h"
#include "Levels/LevelChangedEventArgs.h"
#include "Muffin/MuffinDefinition.h"
#include "Thing/Blocks/ThingBlock.h"

using namespace std;
using namespace Ogre;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Players;
using namespace PonykartParsers;

Player::Player() : hasItem(false)
{
}

Player::Player(LevelChangedEventArgs* eventArgs, int Id, bool IsComputerControlled) : Player()
{
	// don't want to create a player if it's ID isn't valid
	if (Id < 0 || Id >= Settings::NumberOfPlayers)
		throw string("ID number specified for kart spawn position is not valid!");
	ostringstream oss; oss << id;
	log(string("[Loading] Player with ID ") + oss.str() + " created");

	isComputerControlled = IsComputerControlled;

	// set up the spawn position/orientation
	Vector3 spawnPos = eventArgs->newLevel.getDefinition()->getVectorProperty("KartSpawnPosition" + id);
	Quaternion spawnOrient = eventArgs->newLevel.getDefinition()->getQuatProperty("KartSpawnOrientation" + id, Quaternion::IDENTITY);

	ThingBlock* block = new ThingBlock("TwiCutlass", spawnPos, spawnOrient);

	string driverName, kartName;
	string charName = eventArgs->request.characterNames[id];
	if (charName == "Twilight Sparkle")
	{
		driverName = "Twilight";
		kartName = "TwiCutlass";
	}
	else if (charName == "Rainbow Dash")
	{
		driverName = "RainbowDash";
		kartName = "DashJavelin";
	}
	else if (charName == "Applejack")
	{
		driverName = "Applejack";
		kartName = "AJKart";
	}
	else if (charName == "Rarity")
	{
		driverName = "Rarity";
		kartName = "TwiCutlass";
	}
	else if (charName == "Fluttershy")
	{
		driverName = "Fluttershy";
		kartName = "TwiCutlass";
	}
	else if (charName == "Pinkie Pie")
	{
		driverName = "PinkiePie";
		kartName = "TwiCutlass";
	}
	else
		throw string("Invalid character name : "+charName);

	kart = LKernel::getG<Spawner>()->spawnKart(kartName, block);
	driver = LKernel::getG<Spawner>()->spawnDriver(driverName, block);
	driver->attachToKart(kart, Vector3::ZERO);
	kart->player = this;
	driver->player = this;

	character = eventArgs->request.characterNames[id];

	kart->ownerID = Id;
	id = Id;
}

void Player::detach()
{
	kart = nullptr;
	driver = nullptr;
}

const btRigidBody* const Player::getBody() const
{
	return kart->getBody();
}

const Ogre::Vector3 const Player::getNodePosition()
{
	return kart->getRootNode()->_getDerivedPosition();
}

const Kart* const Player::getKart() const
{
	return kart;
}
