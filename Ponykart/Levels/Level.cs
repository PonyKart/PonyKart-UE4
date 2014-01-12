using System;
using System.Collections.Concurrent;
using System.IO;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Lua;
using Ponykart.Physics;
using Ponykart.Properties;
using PonykartParsers;

namespace Ponykart.Levels {
	/// <summary>
	/// Represents a level or world in our game.
	/// </summary>
	public class Level : LDisposable {
		/// <summary>
		/// The world's name - this serves as its identifier
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The type of this level
		/// </summary>
		public LevelType Type { get; private set; }

		public MuffinDefinition Definition { get; private set; }
		/// <summary>
		/// We use the thing's Name as the key
		/// </summary>
		public ConcurrentDictionary<string, LThing> Things { get; private set; }

		/// <summary>
		/// Constructor - Initialises the dictionaries and hooks up to the spawn event
		/// </summary>
		/// <param name="name">The name of the level - this is case sensitive!</param>
		public Level(string name) {
			Name = name;
			Things = new ConcurrentDictionary<string, LThing>();

			if (string.IsNullOrEmpty(name))
				Type = LevelType.EmptyLevel;
			else if (name == Settings.Default.MainMenuName)
				Type = LevelType.Menu;
		}

		/// <summary>
		/// Reads the main .muffin file for this level and loads any extra ones that were "linked" from the main one
		/// </summary>
		public void ReadMuffin() {
			Definition = new MuffinImporter().ParseByName(Name);
			foreach (string file in Definition.ExtraFiles) {
				Definition = new MuffinImporter().ParseByName(file, Definition);
			}

			// get the type of the level
			ThingEnum tempType = Definition.GetEnumProperty("type", null);
			LevelType type;
			Enum.TryParse<LevelType>(tempType + string.Empty, true, out type);
			Type = type;
		}

		/// <summary>
		/// Parses a .scene file and sets up physics stuff
		/// </summary>
		public void ReadDotSceneAndSetupPhysics() {
			LKernel.GetG<PhysicsMain>().LoadPhysicsLevel(Name);
		}

		/// <summary>
		/// Parses the level's save file and creates all of the Things that will be in it.
		/// Also creates the player.
		/// </summary>
		public void CreateEntities() {
			// load up everything into this world

			var spawner = LKernel.GetG<Spawner>();
			foreach (ThingBlock tb in Definition.ThingBlocks) {
				spawner.Spawn(tb.ThingName, tb);
			}
		}

		/// <summary>
		/// Runs the lua function that uses this level's name, if it exists.
		/// If this documentation is up to date, it should be in
		/// "media/scripts/levelNameGoesHere"
		/// </summary>
		public void RunLevelScript() {
			if (Directory.Exists(LuaMain.luaLevelFileLocation + Name + "/"))
				LKernel.GetG<LuaMain>().LuaVM.Lua.GetFunction(Name).Call(this);
		}

		/// <summary>
		/// Runs all of the scripts that the .things have defined, if any
		/// </summary>
		public void RunThingScripts() {
			foreach (LThing l in Things.Values)
				l.RunScript();
		}

		//public void Save() {
		//}

		/// <summary>
		/// Runs whenever we spawn something. This just adds it to the level's dictionary of Things.
		/// </summary>
		public void AddThing(LThing newThing) {
			// try adding it without its ID
			if (!Things.TryAdd(newThing.Name, newThing)) {
				// okay that didn't work, now try adding it with its ID
				if (!Things.TryAdd(newThing.Name + newThing.ID, newThing)) {
					// still didn't work so we must've had a problem while adding it.
					Launch.Log("[Level] **WARNING** (AddThing) A problem occurred when we tried to add this new LThing to the Things dictionary!");
				}
			}
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				if (Things != null) {
					foreach (LThing t in Things.Values)
						t.Dispose();
					Things.Clear();
				}
				LKernel.GetG<ThingDatabase>().ClearDatabase();

				if (Definition != null)
					Definition.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
