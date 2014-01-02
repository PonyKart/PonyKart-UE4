using Mogre;
using Ponykart.UI;

namespace Ponykart {
	public static partial class LKernel {
		/// <summary>
		/// This is run just after the OnLevelUnload event.
		/// </summary>
		public static void Cleanup() {
			RemakeSceneManager();
			LevelObjects.Clear();
		}

		/// <summary>
		/// destroys everything in the scene manager so it's as good as new without destroying the scene manager itself
		/// </summary>
		private static void RemakeSceneManager() {
			Launch.Log("[Loading] Remaking SceneManager...");
			// get our old scene manager
			SceneManager oldMgr = GetG<SceneManager>();
			// destroy it
			GetG<Root>().DestroySceneManager(oldMgr);
			// make our new scene manager
			SceneManager newMgr = GetG<Root>().CreateSceneManager(SceneType.ST_EXTERIOR_FAR, "sceneMgr");

			// re-add it to the kernel objects
			GlobalObjects[typeof(SceneManager)] = newMgr;
			// tell miyagi that it changed
			GetG<UIMain>().ChangeSceneManager(newMgr);
			// then dispose of the old one
			oldMgr.Dispose();
		}
	}
}
