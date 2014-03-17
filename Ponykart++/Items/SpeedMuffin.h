#ifndef SPEEDMUFFIN_H_INCLUDED
#define SPEEDMUFFIN_H_INCLUDED

#include <string>
#include <OgreVector3.h>
#include "Items/Item.h"

namespace Ponykart
{
	namespace Players{ class Player; }
	namespace Items
	{
		class SpeedMuffin : public Item
		{
		public:
			SpeedMuffin(Players::Player* user);
		private:
			Ogre::Vector3 origin;
		};
	}
}
#endif // SPEEDMUFFIN_H_INCLUDED
