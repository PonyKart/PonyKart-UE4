﻿using System;
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

		public Spawner () {
			database = LKernel.GetG<ThingDatabase>();
			levelManager = LKernel.GetG<LevelManager>();
		}

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

			lock (this) {
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



		public BackgroundPony SpawnBgPony(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (this) {
				BackgroundPony pony;
				var definition = database.GetThingDefinition(thingName);
				if (thingName == "LyraSitting") {
					pony = new Lyra(template, definition);
				}
				else {
					pony = new BackgroundPony(template, definition);
				}
				levelManager.CurrentLevel.AddThing(pony);

				Invoke(OnThingCreation, pony);
				return pony;
			}
		}
		public BackgroundPony SpawnBgPony(string thingName, Vector3 spawnPos) {
			return SpawnBgPony(thingName, new ThingBlock(thingName, spawnPos));
		}


		public Kart SpawnKart(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (this) {
				var definition = database.GetThingDefinition(thingName);
				Kart kart = new Kart(template, definition);
				
				levelManager.CurrentLevel.AddThing(kart);

				Invoke(OnKartCreation, kart);
				Invoke(OnThingCreation, kart);
				return kart;
			}
		}


		public Driver SpawnDriver(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				throw new InvalidOperationException("Attempted to spawn \"" + thingName + "\" while paused!");
			}
			lock (this) {
				var definition = database.GetThingDefinition(thingName);
				Driver driver = new Driver(template, definition);

				levelManager.CurrentLevel.AddThing(driver);

				Invoke(OnDriverCreation, driver);
				Invoke(OnThingCreation, driver);
				return driver;
			}
		}

		/// <summary>
		/// helper
		/// </summary>
		void Invoke<T>(SpawnEvent<T> evt, T actor) {
			if (evt != null)
				evt(actor);
		}
	}
}
