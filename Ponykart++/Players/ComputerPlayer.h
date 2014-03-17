#ifndef COMPUTERPLAYER_H_INCLUDED
#define COMPUTERPLAYER_H_INCLUDED
#include <OgreVector3.h>
#include "Players/Player.h"

namespace Ponykart
{
	namespace Actors { class LThing; }
	namespace Core { class KeyBindingManager; }
	namespace Levels{ class LevelChangedEventArgs; }
	namespace Players
	{
		class ComputerPlayer : public Player
		{
		public:
			ComputerPlayer(Levels::LevelChangedEventArgs* eventArgs, int id);
			void everyTenth(void* o);
		protected:
			virtual void useItem() override;
		private:
			float steerTowards(Ogre::Vector3 vecToTar);
		private:
			Actors::LThing *axis, *axis2;
			Ogre::Vector3 nextWaypoint;
			int reverseCooldown = 0;
			int direction = 1;
		};
	}
}
#endif // COMPUTERPLAYER_H_INCLUDED
