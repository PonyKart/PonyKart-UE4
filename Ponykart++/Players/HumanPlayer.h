#ifndef HUMANPLAYER_H_INCLUDED
#define HUMANPLAYER_H_INCLUDED
#include "Players/Player.h"

// comment this out if you want drifting to be disabled for human-controlled karts.
// useful if you want to send a version out to users while the drifting is broken
/// TODO: Do something about the comment above. Might want to remove the define and #ifdef'd out parts.
#define DRIFTING_ENABLED

namespace Ponykart
{
namespace Core { class KeyBindingManager; }
namespace Levels{ class LevelChangedEventArgs; }
namespace Players
{
	class HumanPlayer : public Player
	{
	public:
		HumanPlayer(Levels::LevelChangedEventArgs* eventArgs, int id);
	protected:
		virtual void onStartAccelerate() override;
		virtual void onStopAccelerate() override;
		virtual void onStartDrift() override;
		virtual void onStopDrift() override;
		virtual void onStartReverse() override;
		virtual void onStopReverse() override;
		virtual void onStartTurnLeft() override;
		virtual void onStopTurnLeft() override;
		virtual void onStartTurnRight() override;
		virtual void onStopTurnRight() override;
		virtual void useItem() override;
	public:
		Core::KeyBindingManager* bindings;
	};
}
}
#endif // HUMANPLAYER_H_INCLUDED
