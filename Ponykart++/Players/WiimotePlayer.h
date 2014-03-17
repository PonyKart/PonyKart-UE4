#include "Players/Player.h"

namespace Ponykart
{
	namespace Core { class KeyBindingManager; }
	namespace Levels{ class LevelChangedEventArgs; }
	namespace Players
	{
		class WiimotePlayer : public Player
		{
		public:
			WiimotePlayer(Levels::LevelChangedEventArgs* eventArgs, int id);
		protected:
			virtual void useItem() override;
		public:
			Core::KeyBindingManager* bindings;
		};
	}
}
