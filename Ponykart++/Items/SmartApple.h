#ifndef SMARTAPPLE_H_INCLUDED
#define SMARTAPPLE_H_INCLUDED
#include <string>
#include <OgreVector3.h>
#include "Items/Item.h"

namespace Ponykart
{
namespace Players
{ 
	class Player; 
	class PlayerManager;
}
namespace Items
{
	class SmartApple : public Item
	{
	public:
		SmartApple(Players::Player* user);
	private:
		Ogre::Vector3 origin;
		Players::PlayerManager* playerManager;
	};
}
}
#endif // SMARTAPPLE_H_INCLUDED
