using System.Windows.Forms;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Lua;
using Ponykart.Networking;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Sound;
using Ponykart.Stuff;
using Ponykart.UI;
using Ponykart.Items;
using PonykartParsers;

namespace Ponykart {
	public static partial class LKernel {
		/// <summary>
		/// Load global objects on startup
		/// </summary>
		public static void LoadInitialObjects(Splash splash) {
			// this goes first since lots of things rely on it
			splash.Increment("Setting up level manager...");
			var levelManager = AddGlobalObject(new LevelManager());

			// mogre stuff
			splash.Increment("Initialising Mogre core...");
			var root		 = AddGlobalObject(InitRoot());
			splash.Increment("Initialising render system...");
			var renderSystem = AddGlobalObject(InitRenderSystem(root));
			splash.Increment("Creating render window...");
			var renderWindow = AddGlobalObject(InitRenderWindow(root, renderSystem));

			splash.Increment("Initialising resources and resource groups...");
			InitResources();
			LoadResourceGroups();

			// physics stuff
			splash.Increment("Initialising Bullet physics engine, collision reporter, trigger region reporter, and contact reporter...");
			try {
				AddGlobalObject(new PhysicsMain());
				AddGlobalObject(new CollisionShapeManager());
				AddGlobalObject(new CollisionReporter());
				AddGlobalObject(new TriggerReporter());
				AddGlobalObject(new PhysicsMaterialFactory());
			}
			catch {
				MessageBox.Show("BulletSharp loading unsuccessful! Try installing the 2010 VC++ Redistributable (x86) - google it!",
					"Well, shit.", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// level
			splash.Increment("Creating scene manager...");
			AddGlobalObject(InitSceneManager(root));

			splash.Increment("Loading first level physics...");
			GetG<PhysicsMain>().LoadPhysicsLevel(Settings.Default.MainMenuName);

			splash.Increment("Creating player camera and viewport...");
			AddGlobalObject(InitViewport(renderWindow));
			AddGlobalObject(new CameraManager());

			// MOIS and input stuff
			splash.Increment("Starting input system...");
			AddGlobalObject(new InputMain());
			AddGlobalObject(new KeyBindingManager());
			AddGlobalObject(new InputSwallowerManager());
			AddGlobalObject(new Pauser());

			// spawner
			splash.Increment("Creating spawner...");
			AddGlobalObject(new WheelFactory());
			AddGlobalObject(new ThingDatabase());
			AddGlobalObject(new Spawner());
			AddGlobalObject(new AnimationManager());

			// networking
			splash.Increment("Creating Network manager...");
			AddGlobalObject(new NetworkManager());

			// Miyagi and stuff
			splash.Increment("Initialising Miyagi...");
			AddGlobalObject(new DebugOverlayManager());
			AddGlobalObject(new UIMain());
			AddGlobalObject(new LuaConsoleManager());
			AddGlobalObject(new MainMenuManager());
            AddGlobalObject(new GameUIManager());

			// lua needs these
			splash.Increment("Registering handlers and wrappers...");
			SetUpHandlers();

			// lua
			splash.Increment("Setting up Lua engine and wrappers...");
			var lua = AddGlobalObject(new LuaMain());
			LoadLuaWrappers();
			lua.RunRegisterEvent();

			// players
			splash.Increment("Spawning players...");
			AddGlobalObject(new PlayerManager());
			AddGlobalObject(new RaceCountdown());
			AddGlobalObject(new LapCounter());
			AddGlobalObject(new LThingHelperManager());

            //items
            splash.Increment("Loading items...");
            AddGlobalObject(new ItemManager());

			// more mogre stuff
			splash.Increment("Setting up static and instanced geometry managers...");
			AddGlobalObject(new StaticGeometryManager());
			AddGlobalObject(new InstancedGeometryManager());
			AddGlobalObject(new ImposterBillboarder());

			// sound stuff
			splash.Increment("Setting up sound system...");
			AddGlobalObject(new SoundMain());

			// handlers
			splash.Increment("Loading global handlers...");
			LoadGlobalHandlers();


			splash.Increment("Running post-initialisation events...");
			levelManager.RunPostInitEvents();
		}
	}
}
