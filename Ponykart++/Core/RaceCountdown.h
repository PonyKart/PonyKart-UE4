#include <functional>
#include <vector>

namespace Ponykart
{
namespace Core
{
	enum class RaceCountdownState
	{
		Three = 3,
		Two = 2,
		One = 1,
		Go = 0,
		OneSecondAfterGo = -1
	};

	using RaceCountdownEvent = std::vector<std::function<void(RaceCountdownState)>>;

	class RaceCountdown
	{
		// TODO: Implement RaceCountdown with all its members
	public:
		RaceCountdown(); ///< Hook up to the level un/load events
	public:
		static RaceCountdownEvent onCountdown;
	};
}
}