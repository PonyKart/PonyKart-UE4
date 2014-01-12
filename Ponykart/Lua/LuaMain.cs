// I put these in #if thingies because when I save files, VS removes usings I'm not using
#if !DEBUG
using System.Collections.Generic;
using Mogre;
#endif
using System;
using System.Collections;
using System.IO;
using System.Linq;
using LuaInterface;
using LuaNetInterface;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.UI;

namespace Ponykart.Lua {
	public delegate void LuaEvent();

	// PackageAttributes are kinda like namespaces... I think? Anyway if you set this to null then it's treated as if it's in the global namespace
	// obviously if you want it to be in the global namespace, adding documentation for that namespace is pointless
	public class LuaMain {
		public LuaVirtualMachine LuaVM { get; private set; }
		public static readonly string luaLevelFileLocation = "media/level scripts/";

		public event LuaEvent OnRegister;

		public LuaMain() {
			LuaVM = new LuaVirtualMachine();

			// we don't have to register them on level load because this is a global singleton, not a level one
			RegisterLuaFunctions(this);
			// though we do have to restart lua when we change levels
			LevelManager.OnLevelLoad +=
				(e) => {
					Restart();
				};


			LuaVM.Lua.DoString("print(\"All working over here!\")");
			Launch.Log("[Loading] Lua engine successfully started!");
		}

		/// <summary>
		/// If you have a class that needs to have some of its functions added to lua, add this to the constructor:
		/// LKernel.Get&lt;LuaMain&gt;().RegisterLuaFunctions(this);
		/// It will subscribe that object to the OnRegister event.
		/// </summary>
		/// <param name="o">The class whose functions you want to add to the Lua VM</param>
		/// <remarks>Shorthand</remarks>
		public void RegisterLuaFunctions(object o) {
			//Launch.Log("[LuaMain] \tRegistering lua functions from " + o.GetType());

			OnRegister += () => LuaVM.RegisterLuaFunctions(o);
		}

		/// <summary>
		/// Loads up all of the script files.
		/// </summary>
		/// <param name="levelName">We will also load files from /level scripts/levelName/ if it exists</param>
		public void LoadScriptFiles(string levelName) {
			Launch.Log("[LuaMain] Loading all scripts...");

#if DEBUG
			// first get all of the scripts that aren't in the /level scripts/ directory
			var scripts = Directory.EnumerateFiles("media/", "*.lua", SearchOption.AllDirectories).Where(s => !s.Contains("/level scripts"));

			// then get all of the scripts that are in the /level scripts/ directory (but only the level we're interested in)
			if (Directory.Exists(luaLevelFileLocation + levelName + "/")) {
				Launch.Log("[LuaMain] Loading all scripts from " + luaLevelFileLocation + levelName + "/");
				scripts = scripts.Concat(Directory.EnumerateFiles(luaLevelFileLocation + levelName + "/", "*.lua", SearchOption.AllDirectories));
			}

			foreach (string file in scripts) {
				DoFile(file);
			}
#else
			foreach (string group in ResourceGroupManager.Singleton.GetResourceGroups().Where(s => ResourceGroupManager.Singleton.IsResourceGroupInitialised(s))) {
				if (group == "Bootstrap")
					continue;

				var resourceLocations = ResourceGroupManager.Singleton.ListResourceLocations(group);

				foreach (string loc in resourceLocations) {
					var scripts = Directory.EnumerateFiles(loc, "*.lua", SearchOption.TopDirectoryOnly);

					foreach (string file in scripts) {
						DoFile(file);
					}
				}
			}
#endif
		}

		public void DoFunctionForLThing(string functionName, LThing thing) {
			if (LKernel.GetG<LevelManager>().IsValidLevel) {
				lock (LuaVM.Lua) {
					try {
						LuaVM.Lua["lthing"] = thing;
						LuaVM.Lua.GetFunction(functionName).Call(thing);
					}
					catch (LuaException ex) {
						HandleException(ex);
					}
				}
			}
		}

		/// <summary>
		/// Runs a lua function
		/// </summary>
		/// <param name="functionName">The name of the function to run.</param>
		/// <param name="parameters">The parameters to pass the function</param>
		/// <returns>Stuff returned from the function, or null if the level is not valid</returns>
		public object[] DoFunction(string functionName, params object[] parameters) {
			if (LKernel.GetG<LevelManager>().IsValidLevel) {
				try {
					return LuaVM.Lua.GetFunction(functionName).Call(parameters);
				}
				catch (LuaException ex) {
					HandleException(ex);
				}
			}
			return null;
		}

		/// <summary>
		/// Make lua parse and execute a string of code
		/// </summary>
		/// <param name="s">the string to execute</param>
		public void DoString(string s) {
			if (LKernel.GetG<LevelManager>().IsValidLevel) {
				try {
					LuaVM.Lua.DoString(s);
				}
				catch (LuaException ex) {
					HandleException(ex);
				}
				catch (Exception ex) {
					LKernel.GetG<LuaConsoleManager>().AddLabel(ex.Message);
				}
			}
		}

		/// <summary>
		/// make lua parse and execute a file
		/// </summary>
		/// <param name="filename">the filename of the file to execute</param>
		public void DoFile(string filename) {
			if (LKernel.GetG<LevelManager>().IsValidLevel) {
				Launch.Log("[LuaMain] Running file: " + filename);

				try {
					LuaVM.Lua.DoFile(filename);
				}
				catch (LuaException ex) {
					HandleException(ex);
				}
			}
		}

		/// <summary>
		/// Handles a lua exception
		/// </summary>
		private void HandleException(LuaException ex) {
			Launch.Log("[Lua] *** EXCEPTION *** at " + ex.Source + ": " + ex.Message);
			foreach (var v in ex.Data)
				Launch.Log("[Lua] " + v);
			LKernel.GetG<LuaConsoleManager>().AddLabel("ERROR: " + ex.Message);
			Launch.Log(ex.StackTrace);
		}

		/// <summary>
		/// Gets all of the wrappers to (re-)register their functions
		/// </summary>
		public void RunRegisterEvent() {
			if (OnRegister != null)
				OnRegister();
		}

		/// <summary>
		/// Quits the Lua VM. Does not dispose it - use Restart() if you want to do that.
		/// </summary>
		public void Quit() {
			LuaVM.Stop();
		}

		/// <summary>
		/// Quits, disposes, and creates a new Lua VM, then runs the re-registration event.
		/// </summary>
		public void Restart() {
			Quit();
			LuaVM.Lua.Dispose();

			LuaVM = new LuaVirtualMachine();
			RunRegisterEvent();
		}

		/// <summary>
		/// This overwrites lua's built-in print function so it prints to our console
		/// </summary>
		/// <param name="s">The string to print</param>
		public void Print(string s) {
			Launch.Log("[Lua] " + s);
			LKernel.GetG<LuaConsoleManager>().AddLabel(s);
		}

		/// <summary>
		/// Gets help for a command
		/// </summary>
		/// <param name="command">Package to get help of.</param>
		public void GetCommandHelp(string command) {
			LuaPackageDescriptor packageDesc;

			if (LuaVM.Functions.ContainsKey(command)) {
				LuaFunctionDescriptor descriptor = (LuaFunctionDescriptor) LuaVM.Functions[command];
				Print(descriptor.FullDocumentationString);
				return;
			}

			if (command.IndexOf(".") == -1) {
				if (LuaVM.Packages.ContainsKey(command)) {
					LuaPackageDescriptor descriptor = (LuaPackageDescriptor) LuaVM.Packages[command];
					descriptor.ShowHelp();
					return;
				}
				else {
					Print("No such function or package: " + command);
					return;
				}
			}

			string[] parts = command.Split('.');

			if (!LuaVM.Packages.ContainsKey(parts[0])) {
				Print("No such function or package: " + command);
				return;
			}

			packageDesc = (LuaPackageDescriptor) LuaVM.Packages[parts[0]];

			if (!packageDesc.HasFunction(parts[1])) {
				Print("Package " + parts[0] + " doesn't have a " + parts[1] + " function.");
				return;
			}

			packageDesc.ShowHelp(parts[1]);
		}

		/// <summary>
		/// Lists available commands
		/// </summary>
		public void GetHelp() {
			Print("Available commands: \n");

			IDictionaryEnumerator functions = LuaVM.Functions.GetEnumerator();

			while (functions.MoveNext()) {
				Print(((LuaFunctionDescriptor) functions.Value).GetFunctionHeader());
			}

			if (LuaVM.Packages.Count > 0) {
				Print("\n\nAvailable packages: \n");

				IDictionaryEnumerator packages = LuaVM.Packages.GetEnumerator();

				while (packages.MoveNext()) {
					Print((string) packages.Key + " - " + ((LuaPackageDescriptor) packages.Value).GetPackageHeader());
				}
			}
		}
	}
}
