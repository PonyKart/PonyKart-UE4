#ifndef DRIFTSTATE_H_INCLUDED
#define DRIFTSTATE_H_INCLUDED

namespace Ponykart
{
namespace Actors
{

	// Lets us keep track of whether we're drifting or not, and if we are, which direction we're drifting in
	enum class KartDriftState
	{
		None, // Turn angle is zero
		FullLeft, // Turn angle is negative
		FullRight, // Turn angle is positive
		StartLeft, // Starting to drift left
		StartRight, // Starting to drift right
		StopLeft, // Stopping drifting left
		StopRight, // Stopping drifting right
		// Used if we're pressing the drift button but we haven't specified which way we want to drift yet.
		// You should unset this flag once you know the desired direction.
		WantsDriftingButNotTurning
	};

    namespace KartDriftStateExtensions
	{
		static inline bool isFullDrift(KartDriftState kds)
		{
			return kds == KartDriftState::FullLeft || kds == KartDriftState::FullRight;
		};

		static inline bool isStartDrift(KartDriftState kds)
		{
			return kds == KartDriftState::StartLeft || kds == KartDriftState::StartRight;
		};

		static inline bool isStopDrift(KartDriftState kds)
		{
			return kds == KartDriftState::StopLeft || kds == KartDriftState::StopRight;
		};

		static inline bool isDriftingAtAll(KartDriftState kds)
		{
			return kds == KartDriftState::FullLeft || kds == KartDriftState::FullRight
				|| kds == KartDriftState::StartLeft || kds == KartDriftState::StartRight
				|| kds == KartDriftState::StopLeft || kds == KartDriftState::StopRight;
		};
	};

	// The wheels don't really care if we're starting or stopping drifting, so they use a much simpler enum.
	enum class WheelDriftState
	{
		None,
		Left,
		Right
	};

} // Actors
} // Ponykart

#endif // DRIFTSTATE_H_INCLUDED
