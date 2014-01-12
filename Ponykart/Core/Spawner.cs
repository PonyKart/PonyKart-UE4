using System;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Core {
	public delegate void SpawnEvent<T>(T thing);

	public class Spawner {
		/// <summary>
		/// Fires whenever anything is spawned.
		/// </summary>
		public static event SpawnEvent<LThing> OnThingCreation;
		/// <summary>
		/// Fires whenever a Kart is spawned.
		/// </summary>
		public static event SpawnEvent<Kart> OnKartCreation;
		/// <summary>
		/// Fires whenever a Driver is spawned.
		/// </summary>
		public static event SpawnEvent<Driver> OnDriverCreation;

		private ThingDatabase database;
		private LevelManager levelManager;

		public Spawner() {
			database = LKernel.GetG<ThingDatabase>();
			levelManager = LKernel.GetG<LevelManager>();
		}

		private object _spawnLock = new Object();

		/// <summary>
		/// Spawns something!
		/// </summary>
		/// <param name="thingName">What do you want to spawn? This is the filename of the .thing file to use, minus the extension.</param>
		/// <param name="template">The template for the thing you want to spawn</param>
		/// <returns>The thing you just spawned. Returns null if you are paused.</returns>
		public LThing Spawn(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}

			lock (_spawnLock) {
				var definition = database.GetThingDefinition(thingName);
				LThing thing = new LThing(template, definition);

				levelManager.CurrentLevel.AddThing(thing);

				Invoke(OnThingCreation, thing);
				return thing;
			}
		}

		/// <summary>
		/// Spawns something! This takes a string instead of an enum for the type, but if the string is not a valid type,
		/// then an exception gets thrown, so be careful! Note that it is not case sensitive.
		/// </summary>
		/// <param name="thingName">The type (class name) for the thing you want to spawn</param>
		/// <param name="spawnPos">Where should it spawn?</param>
		/// <returns>The thing you spawned</returns>
		public LThing Spawn(string thingName, Vector3 spawnPos) {
			var tt = new ThingBlock(thingName, spawnPos);

			return Spawn(thingName, tt);
		}

		/// <summary>
		/// To avoid needing a separate spawner method for every single subclass of LThing, we can use this method instead since it's, well, generic.
		/// </summary>
		/// <typeparam name="T">
		/// Your subclass of LThing that you want to spawn. Don't use Driver or Kart; there are already specific spawner methods for those types.
		/// </typeparam>
		/// <param name="thingName">The name of the Thing you want to spawn.</param>
		/// <param name="template">The template for the thing you want to spawn.</param>
		/// <param name="construct">
		/// This is the fun part. Here you specify a function that calls the constructor of the T type you specified.
		/// For example, if we want to spawn Derpy, you'd use <code>(t, d) => new Derpy(t, d)</code>.
		/// </param>
		/// <returns>The thing you just spawned.</returns>
		public T Spawn<T>(string thingName, ThingBlock template, Func<ThingBlock, ThingDefinition, T> construct) where T : LThing {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (_spawnLock) {
				var definition = database.GetThingDefinition(thingName);
				T thing = construct(template, definition);

				levelManager.CurrentLevel.AddThing(thing);

				Invoke(OnThingCreation, thing);
				return thing;
			}
		}

		/// <summary>
		/// To avoid needing a separate spawner method for every single subclass of LThing, we can use this method instead since it's, well, generic.
		/// </summary>
		/// <typeparam name="T">
		/// Your subclass of LThing that you want to spawn. Don't use Driver or Kart; there are already specific spawner methods for those types.
		/// </typeparam>
		/// <param name="thingName">The name of the Thing you want to spawn.</param>
		/// <param name="template">The template for the thing you want to spawn.</param>
		/// <param name="construct">
		/// This is the fun part. Here you specify a function that calls the constructor of the T type you specified.
		/// For example, if we want to spawn Derpy, you'd use <code>(t, d) => new Derpy(t, d)</code>.
		/// </param>
		/// <returns>The thing you just spawned.</returns>
		public T Spawn<T>(string thingName, string extraParam, ThingBlock template, Func<string, ThingBlock, ThingDefinition, T> construct) where T : LThing {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (_spawnLock) {
				var definition = database.GetThingDefinition(thingName);
				T thing = construct(extraParam, template, definition);

				levelManager.CurrentLevel.AddThing(thing);

				Invoke(OnThingCreation, thing);
				return thing;
			}
		}

		/// <summary>
		/// To avoid needing a separate spawner method for every single subclass of LThing, we can use this method instead since it's, well, generic.
		/// </summary>
		/// <typeparam name="T">
		/// Your subclass of LThing that you want to spawn. Don't use Driver or Kart; there are already specific spawner methods for those types.
		/// </typeparam>
		/// <param name="thingName">The name of the Thing you want to spawn.</param>
		/// <param name="pos">The position that you want this thing to be spawned at.</param>
		/// <param name="construct">
		/// This is the fun part. Here you specify a function that calls the constructor of the T type you specified.
		/// For example, if we want to spawn Derpy, you'd use <code>(t, d) => new Derpy(t, d)</code>.
		/// </param>
		/// <returns>The thing you just spawned.</returns>
		public T Spawn<T>(string thingName, Vector3 pos, Func<ThingBlock, ThingDefinition, T> construct) where T : LThing {
			return Spawn<T>(thingName, new ThingBlock(thingName, pos), construct);
		}


#region Specific spawners
		/// <summary>
		/// Spawns a kart.
		/// </summary>
		public Kart SpawnKart(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (_spawnLock) {
				var definition = database.GetThingDefinition(thingName);

				Kart kart;
                if (thingName == "DashJavelin")
                    kart = new DashJavelin(template, definition);
                else if (thingName == "TwiCutlass")
                    kart = new TwiCutlass(template, definition);
                else
                    kart = new Kart(template, definition);
				levelManager.CurrentLevel.AddThing(kart);

				Invoke(OnKartCreation, kart);
				Invoke(OnThingCreation, kart);
				return kart;
			}
		}

		/// <summary>
		/// Spawns a driver.
		/// </summary>
		public Driver SpawnDriver(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (_spawnLock) {
				var definition = database.GetThingDefinition(thingName);
				Driver driver = new Driver(template, definition);

				levelManager.CurrentLevel.AddThing(driver);

				Invoke(OnDriverCreation, driver);
				Invoke(OnThingCreation, driver);
				return driver;
			}
		}
#endregion

		/// <summary>
		/// helper
		/// </summary>
		void Invoke<T>(SpawnEvent<T> evt, T actor) {
			if (evt != null)
				evt(actor);
		}
	}
}
