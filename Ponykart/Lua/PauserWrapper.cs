/*using LuaNetInterface;
using Ponykart.Core;

namespace Ponykart.Lua {
	
	//[LuaPackage("Pauser", "A wrapper for the pauser class")]
	[LuaPackage(null, null)]
	public class PauserWrapper {

		public PauserWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("pause", "Pauses the game but does not fire any events.")]
		public static void Pause() {
			Pauser.IsPaused = true;
		}

		[LuaFunction("unpause", "Unpauses the game but does not fire any events.")]
		public static void Unpause() {
			Pauser.IsPaused = false;
		}

		[LuaFunction("pauseWithEvent", "Pauses the game and fires an event.")]
		public static void PauseWithEvent() {
			if (Pauser.IsPaused) // don't call this if it's already paused
				return;

			Pauser p = LKernel.GetG<Pauser>();
			if (p != null)
				p.InvokePauseEvent();
		}

		[LuaFunction("unpauseWithEvent", "Unauses the game and fires an event.")]
		public static void UnpauseWithEvent() {
			if (!Pauser.IsPaused) // don't call this if it's already unpaused
				return;

			Pauser p = LKernel.GetG<Pauser>();
			if (p != null)
				p.InvokePauseEvent();
		}

		[LuaFunction("isPaused", "Returns whether the game is currently paused or not.")]
		public static bool IsPaused() {
			return Pauser.IsPaused;
		}

		[LuaFunction("hookFunctionToPauseEvent", "Hook up a lua function so it will run whenever the pause event fires.",
			"function(PausingState)")]
		public static void HookFunctionToPauseEvent(PauseEvent pe) {
			Pauser p = LKernel.GetG<Pauser>();
			if (p != null) {
				p.PauseEvent += pe;
			}
		}
	}
}
*/