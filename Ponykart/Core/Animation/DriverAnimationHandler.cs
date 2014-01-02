using Mogre;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Core {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DriverAnimationHandler : ILevelHandler {
		Kart[] karts;

		public DriverAnimationHandler() {
			Player[] players = LKernel.GetG<PlayerManager>().Players;
			karts = new Kart[players.Length];

			for (int a = 0; a < players.Length; a++) {
				karts[a] = players[a].Kart;
			}

			Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
		}

		void EveryTenth(object o) {
			for (int a = 0; a < karts.Length; a++) {
				Kart kart = karts[0];
				Driver driver = kart.Driver;

				if (kart.VehicleSpeed >= -6 || kart.IsInAir) {
					// forwards
					if (kart.DriftState == KartDriftState.StartLeft || kart.DriftState == KartDriftState.FullLeft) {
						driver.ChangeAnimationIfNotBlending(DriverAnimation.DriftLeft);
					}
					else if (kart.DriftState == KartDriftState.StartRight || kart.DriftState == KartDriftState.FullRight) {
						driver.ChangeAnimationIfNotBlending(DriverAnimation.DriftRight);
					}
					else {
						if (kart.TurnMultiplier < 0.2f && kart.TurnMultiplier > -0.2f) {
							// straight
							driver.ChangeAnimationIfNotBlending(DriverAnimation.Drive);
						}
						else if (kart.TurnMultiplier < -0.2f) {
							// right
							driver.ChangeAnimationIfNotBlending(DriverAnimation.TurnRight);
						}
						else {
							// left
							driver.ChangeAnimationIfNotBlending(DriverAnimation.TurnLeft);
						}
					}
				}
				else {
					// reverse
					// don't have multiple reverse anims at the moment
					driver.ChangeAnimationIfNotBlending(DriverAnimation.Reverse, AnimationBlendingTransition.BlendWhileAnimating, 0.3f);
				}
			}
		}

		public void Detach() {
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;
		}
	}
}
