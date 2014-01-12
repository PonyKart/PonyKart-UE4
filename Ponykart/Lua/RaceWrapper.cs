using LuaNetInterface;
using Ponykart.Core;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class RaceWrapper {

		public RaceWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("hookFunctionToCountdownEvent", "Hooks a function to one of the countdown events.", "Function(RaceCountdownState)")]
		public static void HookFunctionToCountdownEvent(RaceCountdownEvent rce) {
			RaceCountdown.OnCountdown += rce;
		}

		[LuaFunction("hookFunctionToLapEvent", "Hooks a function to the lap event.", "Function(Kart, int newLapCount)")]
		public static void HookFunctionToLapEvent(LapCounterEvent lce) {
			LapCounter.OnLap += lce;
		}

		[LuaFunction("hookFunctionToPlayerLapEvent", "Hooks a function to the player lap event.", "Function(Kart, int newLapCount)")]
		public static void HookFunctionToPlayerLapEvent(LapCounterEvent lce) {
			LapCounter.OnPlayerLap += lce;
		}

		[LuaFunction("hookFunctionToFinishEvent", "Hooks a function to the finish event.", "Function(Kart)")]
		public static void HookFunctionToFinishEvent(RaceFinishEvent lce) {
			LapCounter.OnFinish += lce;
		}

		[LuaFunction("hookFunctionToPlayerFinishEvent", "Hooks a function to the player finish event.", "Function(Kart)")]
		public static void HookFunctionToPlayerFinishEvent(RaceFinishEvent lce) {
			LapCounter.OnPlayerFinish += lce;
		}

		[LuaFunction("hookFunctionToFirstFinishEvent", "Hooks a function to the first finish event.", "Function(Kart)")]
		public static void HookFunctionToFirstFinishEvent(RaceFinishEvent lce) {
			LapCounter.OnFirstFinish += lce;
		}
	}
}
