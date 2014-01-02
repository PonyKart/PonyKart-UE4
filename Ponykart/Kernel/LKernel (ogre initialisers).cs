using System.Collections.Generic;
using Mogre;
using Ponykart.Core;
using Ponykart.Properties;

namespace Ponykart {
	public static partial class LKernel {

		/// <summary>
		/// Initialises the root
		/// </summary>
		private static Root InitRoot() {
			return new Root("media/plugins/plugins.cfg", "", "Ponykart.log");
		}

		/// <summary>
		/// Initialises the render system, tells it to use directx, windowed, etc
		/// </summary>
		private static RenderSystem InitRenderSystem(Root root) {
			Launch.Log("[Loading] Creating RenderSystem...");

			if (root.RenderSystem == null) {
				var renderSystem = root.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
				renderSystem.SetConfigOption("Full Screen", Options.Get("Full Screen"));
				renderSystem.SetConfigOption("Floating-point mode", Options.Get("Floating-point mode"));
				renderSystem.SetConfigOption("VSync", Options.Get("VSync"));
				renderSystem.SetConfigOption("VSync Interval", Options.Get("VSync Interval"));
				renderSystem.SetConfigOption("FSAA", Options.Get("FSAA"));
				renderSystem.SetConfigOption("Video Mode", Options.Get("Video Mode"));
				renderSystem.SetConfigOption("sRGB Gamma Conversion", Options.Get("sRGB Gamma Conversion"));

				root.RenderSystem = renderSystem;
			}

#if DEBUG
			// print out the things we can support
			var renderList = root.GetAvailableRenderers();
			foreach (var renderSystem in renderList) {
				Launch.Log("\n**** Available options for Render System: " + renderSystem.Name + " ****");
				foreach (var option in renderSystem.GetConfigOptions()) {
					Launch.Log("\t" + option.Key);
					foreach (var p in option.Value.possibleValues) {
						Launch.Log("\t\t" + p);
					}
				}
				Launch.Log("***********************************");
			}
#endif

			return root.RenderSystem;
		}

		/// <summary>
		/// Creates the render window, tells it to use our WinForms window, and enables vsync
		/// </summary>
		private static RenderWindow InitRenderWindow(Root root, RenderSystem renderSystem) {
			Launch.Log("[Loading] Creating RenderWindow...");

			//Ensure RenderSystem has been Initialised
			root.RenderSystem = renderSystem;

			var window = root.Initialise(true, "Ponykart");
			window.SetVisible(false);
			window.SetIcon(Resources.Icon_2);
			window.SetDeactivateOnFocusChange(false);

			return window;
		}

		/// <summary>
		/// Creates the scene manager and sets up shadow stuff
		/// </summary>
		private static SceneManager InitSceneManager(Root root) {
			Launch.Log("[Loading] Creating SceneManager");
			var sceneMgr = root.CreateSceneManager("OctreeSceneManager", "sceneMgr");
			return sceneMgr;
		}

		/// <summary>
		/// Creates the viewport
		/// </summary>
		private static Viewport InitViewport(RenderWindow window) {
			Launch.Log("[Loading] Creating Viewport...");
			// just make a temporary camera now for this
			return window.AddViewport(GetG<SceneManager>().CreateCamera("tempCam"));
		}

		/// <summary>
		/// Basically adds all of the resource locations but doesn't actually load anything.
		/// </summary>
		private static void InitResources() {
			ConfigFile file = new ConfigFile();
			file.Load("media/plugins/resources.cfg", "\t:=", true);
			ConfigFile.SectionIterator sectionIterator = file.GetSectionIterator();

			while (sectionIterator.MoveNext()) {
				string currentKey = sectionIterator.CurrentKey;
				foreach (KeyValuePair<string, string> pair in sectionIterator.Current) {
					string key = pair.Key;
					string name = pair.Value;
					ResourceGroupManager.Singleton.AddResourceLocation(name, key, currentKey);
				}
			}

			file.Dispose();
			sectionIterator.Dispose();
		}

		/// <summary>
		/// This is where resources are actually loaded into memory.
		/// </summary>
		private static void LoadResourceGroups() {
			// knighty, this uncommented means we USE the mipmaps from the DDSes
			// try it - comment it out, then look at the detail on the roads and far-off tree billboards (yes you'll need to change your ModelDetail to see them)
			// then uncomment this and look at them again - notice how much sharper they are? That's because they're now using the DDS files' mipmaps,
			// which were created in photoshop to look a lot nicer than the blurry mess ogre creates on the fly.
			TextureManager.Singleton.DefaultNumMipmaps = 8;

#if !DEBUG
			TextureManager.Singleton.Verbose = false;
			MeshManager.Singleton.Verbose = false;
			SkeletonManager.Singleton.Verbose = false;
#endif

#if DEBUG
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
#else
			ResourceGroupManager.Singleton.InitialiseResourceGroup("Bootstrap");
			ResourceGroupManager.Singleton.InitialiseResourceGroup("General");
#endif
		}
	}
}
