using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LuaNetInterface;
using Ponykart.Levels;

namespace Ponykart {
	public static partial class LKernel {
		private static IEnumerable<Type> LevelHandlerTypes;
		private static IEnumerable<Type> GlobalHandlerTypes;
		private static IEnumerable<Type> LuaWrapperTypes;

		static void SetUpHandlers() {
			var types = Assembly.GetExecutingAssembly().GetTypes();
			LevelHandlerTypes = types.Where(
				t => ((HandlerAttribute[]) t.GetCustomAttributes(typeof(HandlerAttribute), false))
					 .Where(a => a.Scope == HandlerScope.Level)
					 .Count() > 0);
			GlobalHandlerTypes = types.Where(
				t => ((HandlerAttribute[]) t.GetCustomAttributes(typeof(HandlerAttribute), false))
					 .Where(a => a.Scope == HandlerScope.Global)
					 .Count() > 0);
			LuaWrapperTypes = types.Where(
				t => ((LuaPackageAttribute[]) t.GetCustomAttributes(typeof(LuaPackageAttribute), false))
					 .Count() > 0);
		}

		/// <summary>
		/// Load global handlers
		/// </summary>
		static void LoadGlobalHandlers() {
			Launch.Log("[Loading] Initialising global handlers...");

			foreach (Type t in GlobalHandlerTypes) {
				Launch.Log("[Loading] \tCreating " + t);
				AddGlobalObject(Activator.CreateInstance(t), t);
			}
		}

		/// <summary>
		/// Load handlers for each level
		/// </summary>
		public static void LoadLevelHandlers(Level newLevel) {
			Launch.Log("[Loading] Initialising per-level handlers...");

			IEnumerable<Type> e = LevelHandlerTypes.Where(
				t => ((HandlerAttribute[]) t.GetCustomAttributes(typeof(HandlerAttribute), false))
					 .Where(a => ((a.LevelType & newLevel.Type) == newLevel.Type) && (a.LevelNames == null || a.LevelNames.Contains(newLevel.Name)))
					 .Count() > 0);

			foreach (Type t in e) {
				Launch.Log("[Loading] \tCreating " + t);
				AddLevelObject(Activator.CreateInstance(t), t);
			}
		}

		/// <summary>
		/// Unload and dispose of the special per-level handlers. This is run before the regular OnLevelUnload event.
		/// </summary>
		public static void UnloadLevelHandlers() {
			Launch.Log("[Loading] Disposing of level handlers...");
			foreach (var obj in LevelObjects.Values) {
				Console.WriteLine("[Loading] \tDisposing: " + obj.GetType().ToString());
				// if this cast fails, then you need to make sure the level handler implements ILevelHandler!
				(obj as ILevelHandler).Detach();
			}
		}

		/// <summary>
		/// Load lua wrappers
		/// </summary>
		static void LoadLuaWrappers() {
			Launch.Log("[Loading] Initialising lua wrappers...");

			foreach (Type t in LuaWrapperTypes) {
				Launch.Log("[Loading] \tCreating " + t);
				AddGlobalObject(Activator.CreateInstance(t), t);
			}
		}
	}
}
