using System;
using LuaNetInterface;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class MiscWrapper {

		public MiscWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("loadLevel", "Unloads the current level and loads a new one. If you know the level's ID, use loadLevel instead. The name is case insensitive.",
			"string newLevelName - The name of the new level. Case insensitive.")]
		public static void LoadLevel(string newLevelName) {
			LKernel.GetG<LevelManager>().LoadLevel(new LevelChangeRequest() {
				NewLevelName = newLevelName,
				CharacterNames = LKernel.GetG<Players.PlayerManager>().MakeCharacterString()
			});
		}

		[LuaFunction("getLevelName", "Gets the name of the current level. Returns \"\" if the current level is not valid.")]
		public static string GetCurrentLevelName() {
			LevelManager lm = LKernel.GetG<LevelManager>();
			if (lm.IsValidLevel)
				return lm.CurrentLevel.Name;
			else
				return string.Empty;
		}

		[LuaFunction("getOption", "Get an option", "string - the name of the option you want")]
		public static string GetOption(string option) {
			if (string.Equals(option, "ModelDetail", StringComparison.InvariantCultureIgnoreCase))
				return Options.ModelDetail.ToString();

			return Options.Get(option);
		}

		[LuaFunction("getBoolOption", "Get an option as a bool", "string - the name of the option you want")]
		public static bool GetBoolOption(string option) {
			return Options.GetBool(option);
		}

		[LuaFunction("getInputMain", "Get the InputMain singleton")]
		public static InputMain GetInputMain() {
			return LKernel.GetG<InputMain>();
		}

		[LuaFunction("keyCode", "Turns a KeyCode enum into a string", "KeyCode")]
		public static string KeyCode(MOIS.KeyCode kc) {
			return kc + "";
		}
	}
}
