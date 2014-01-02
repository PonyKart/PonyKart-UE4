using MOIS;
using Ponykart.Levels;

namespace Ponykart.Core {
	public delegate void PauseEvent(PausingState state);

	public enum PausingState {
		Pausing,
		Unpausing
	}

	public class Pauser {
		/// <summary>
		/// An event for things that need it
		/// </summary>
		public event PauseEvent PauseEvent;
		/// <summary>
		/// I think most things will be fine with just a boolean.<br />
		/// Setting this to true will pause the spawner, physics engine, level changer, movement managers, etc.
		/// It won't pause animations, UI, scripts, cameras, and so on.
		/// </summary>
		public static bool IsPaused = false;

		public Pauser() {
			Launch.Log("[Loading] Creating Pauser");

			// if we press `, then pause
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += InvokePauseEvent;

			LKernel.GetG<InputSwallowerManager>().AddSwallower(() => IsPaused, this);
		}

		/// <summary>
		/// Checks to make sure the key pressed matches the pause key (`) then checks to make sure we aren't on the main menu
		/// </summary>
		public void InvokePauseEvent(KeyEvent ke) {
			if (ke.key == KeyCode.KC_GRAVE)
				InvokePauseEvent();
		}

		/// <summary>
		/// Checks to make sure we aren't on the main menu (don't want to unpause that!). This is a separate method so we can call it from e.g. Lua.
		/// </summary>
		public void InvokePauseEvent() {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel)
				PauseWithEvent();
		}

		/// <summary>
		/// Use this to pause things but it also fires off a pause event, which may cause other things to happen that you don't want.
		/// </summary>
		public void PauseWithEvent() {
			Launch.Log("Pause!");
			IsPaused = !IsPaused;
			if (PauseEvent != null) {
				if (IsPaused)
					PauseEvent(PausingState.Pausing);
				else
					PauseEvent(PausingState.Unpausing);
			}
		}
	}
}
