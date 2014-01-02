using IrrKlang;
using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Sound;

namespace Ponykart.Lua {
	//[LuaPackage("Sounds", "The wrapper for the SoundMain class")]
	[LuaPackage(null, null)]
	public class SoundWrapper {
		public SoundWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("create2DSound",
			"Creates a 2D sound i.e. one that has the same volume no matter where the player is. For things like ambient sounds and stuff.",
			"string filePath - The path to the sound file to play.", 
			"bool looping - Whether the sound should loop or not.")]
		public static ISound Create2DSound(string filePath, bool looping) {
			return LKernel.GetG<SoundMain>().Play2D(filePath, looping);
		}

		[LuaFunction("create3DSound",
			"Creates a 3D sound i.e. one that changes volume and stuff depending on where the player is. For things like sound effects and stuff.",
			"string filePath - The path to the sound file to play.",
			"Vector3 pos", "bool looping - Whether the sound should loop or not.")]
		public static ISound Create3DSound(string filePath, Vector3 pos, bool looping) {
			return LKernel.GetG<SoundMain>().Play3D(filePath, pos, looping);
		}

		[LuaFunction("createMusic",
			"Creates a 2D music sound - this differs from create2DSound() by using a different option to toggle whether it's enabled or not.",
			"string filePath - The path to the sound file to play.")]
		public static ISound CreateMusic(string filePath) {
			return LKernel.GetG<SoundMain>().PlayMusic(filePath);
		}

		[LuaFunction("isCurrentlyPlaying", "Tells whether a given sound is playing or not.", "string soundName - The name of the sound. Not a file path.")]
		public static bool IsCurrentlyPlaying(string soundName) {
			return LKernel.GetG<SoundMain>().Engine.IsCurrentlyPlaying(soundName);
		}

		[LuaFunction("addSoundFrameFunction", "Adds a function that you want to be ran every frame, using an lthing's sound components.",
			"LThing - the lthing that owns the sound components", "int componentID", "function(LThing) - the function you want to run every frame")]
		public static void AddSoundFrameEvent(LThing lthing, int componentID, SoundFrameEvent action) {
			lthing.SoundComponents[componentID].OnUpdate += action;
		}
	}
}
