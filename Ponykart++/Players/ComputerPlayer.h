#include "Players/Player.h"

namespace Ponykart
{
	namespace Core { class KeyBindingManager; }
	namespace Levels{ class LevelChangedEventArgs; }
	namespace Players
	{
		class ComputerPlayer : public Player
		{
		public:
			ComputerPlayer(Levels::LevelChangedEventArgs* eventArgs, int id);
		protected:
			virtual void useItem() override;
		};
	}
}