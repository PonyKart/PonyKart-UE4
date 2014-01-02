using System;
using Mogre;
using Ponykart.Handlers.NotReallyHandlers;
using Ponykart.Players;

namespace Ponykart.Handlers
{
	/// <summary>
	/// This class manages stuff to do with player movement, such as responding to WASD key presses and applying forces to the player.
	/// </summary>
	public class PlayerMovementHandler : IDisposable
	{
		// This handles the keyboard events
		private PlayerMovementKeyboardHandler keyboardHandler;

		public PlayerMovementHandler()
		{
			Launch.Log("[Loading] Creating PlayerMovementHandler");

			keyboardHandler = new PlayerMovementKeyboardHandler(LKernel.Get<InputMain>());

			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		public void Dispose() {
			Launch.Log("[Loading] Disposing PlayerMovementHandler");

			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			keyboardHandler.Dispose();
		}

		/// <summary>
		/// Apply a force every frame (can probably reduce this) to the player
		/// </summary>
		bool FrameStarted(FrameEvent evt)
		{
			// if someone is typing something into a text box, we don't want to move around!
			if (LKernel.Get<InputSwallowerManager>().IsSwallowed())
				return true;

			var player = LKernel.Get<PlayerManager>().MainPlayer;
			if (player != null) {
				player.Actor.AddForceAtLocalPos(keyboardHandler.MovementVector * player.Kart.MoveSpeed, Vector3.ZERO);
			}

			return true;
		}
	}
}
