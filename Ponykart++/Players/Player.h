#ifndef PLAYER_H_INCLUDED
#define PLAYER_H_INCLUDED

#include <string>
#include "Actors/Driver.h"
#include "Actors/Kart.h"


namespace Ponykart
{
namespace Levels{ class LevelChangedEventArgs; }
namespace Players
{
// Abstract class for players - each player controls a kart, and abstracting away the player will help when it comes to things like AI and/or networking
class Player // TODO: Implement shortcuts and key events. Finish implementing the rest.
{
public:
	Player(Levels::LevelChangedEventArgs* eventArgs, int Id, bool IsComputerControlled);
	virtual void detach();
	// Getters
	const Actors::Kart* const getKart() const;
	const Actors::Driver* const getDriver() const;
	const btRigidBody* const getBody() const; ///< Gets the kart's Body
	int getId();
	const std::string& getCharacter();
	bool getIsComputerControlled();
	bool getIsLocal();
	const Ogre::SceneNode* const getNode(); // Gets the kart's SceneNode
	//const RigidBody* const getRigidBody(); // Gets the kart's Body // TODO: Implement with OgreBullet
	const Ogre::Quaternion* const getOrientation(); // Gets the kart's SceneNode's orientation
	// Gets the kart's Node's position. No setter because it's automatically changed to whatever the position of its body is
	// Use the Bullet RigidBody if you want to change the kart's position!
	const Ogre::Vector3 const getNodePosition();
protected:
	Player(); // Set some default values
	virtual void useItem()=0; // Uses an item
	// Key events : it's very important that these are run before any of the "override" methods do anything else
	virtual void onStartAccelerate() {}
	virtual void onStopAccelerate() {}
	virtual void onStartDrift() {}
	virtual void onStopDrift() {}
	virtual void onStartReverse() {}
	virtual void onStopReverse() {}
	virtual void onStartTurnLeft() {}
	virtual void onStopTurnLeft() {}
	virtual void onStartTurnRight() {}
	virtual void onStopTurnRight() {}
public:
	bool isControlEnabled; // Can the player control his kart?
protected:
	Actors::Kart* kart; // The kart that this player is driving
	Actors::Driver* driver; // The driver in the kart
	int id; // id number. Same thing that's used as the array index in PlayerManager.
	std::string character;
	bool hasItem;
	std::string heldItem;
private:
	bool isComputerControlled;
	bool isLocal;
};
} // Players
} // Ponykart

#endif // PLAYER_H_INCLUDED
