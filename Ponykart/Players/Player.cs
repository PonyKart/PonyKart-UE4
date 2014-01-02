using System;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using PonykartParsers;


namespace Ponykart.Players {
	/// <summary>
	/// Abstract class for players - each player controls a kart, and abstracting away the player will help when it comes to things like AI and/or networking
	/// </summary>
	public abstract class Player {
		/// <summary>
		/// The kart that this player is driving
		/// </summary>
		public Kart Kart { get; protected set; }
		/// <summary>
		/// The driver in the kart
		/// </summary>
		public Driver Driver { get; protected set; }
		/// <summary>
		/// ID number. Same thing that's used as the array index in PlayerManager.
		/// </summary>
		public int ID { get; protected set; }
		/// <summary>
		/// Can the player control his kart?
		/// </summary>
		public virtual bool IsControlEnabled { get; set; }

		public string Character { get; protected set; }

        public bool IsComputerControlled { get; private set; }

        public bool IsLocal { get; private set; }

        public bool hasItem = false;
        public string heldItem;

		public Player(LevelChangedEventArgs eventArgs, int id, bool isComputerControlled) {
			// don't want to create a player if it's ID isn't valid
			if (id < 0 || id >= Settings.Default.NumberOfPlayers)
				throw new ArgumentOutOfRangeException("id", "ID number specified for kart spawn position is not valid!");
			Launch.Log("[Loading] Player with ID " + id + " created");

			this.IsComputerControlled = isComputerControlled;

			// set up the spawn position/orientation
			Vector3 spawnPos = eventArgs.NewLevel.Definition.GetVectorProperty("KartSpawnPosition" + id, null);
			Quaternion spawnOrient = eventArgs.NewLevel.Definition.GetQuatProperty("KartSpawnOrientation" + id, Quaternion.IDENTITY);

			ThingBlock block = new ThingBlock("TwiCutlass", spawnPos, spawnOrient);

			string driverName, kartName;
			switch (eventArgs.Request.CharacterNames[id]) {
				case "Twilight Sparkle":
					driverName = "Twilight";
					kartName = "TwiCutlass";
					break;
				case "Rainbow Dash":
					driverName = "RainbowDash";
					kartName = "DashJavelin";
					break;
				case "Applejack":
					driverName = "Applejack";
					kartName = "AJKart";
					break;
				case "Rarity":
					driverName = "Rarity";
					kartName = "TwiCutlass";
					break;
				case "Fluttershy":
					driverName = "Fluttershy";
					kartName = "TwiCutlass";
					break;
				case "Pinkie Pie":
					driverName = "PinkiePie";
					kartName = "TwiCutlass";
					break;
				default:
					throw new ArgumentException("Invalid character name!", "eventArgs");
			}

			Kart = LKernel.GetG<Spawner>().SpawnKart(kartName, block);
			Driver = LKernel.GetG<Spawner>().SpawnDriver(driverName, block);
			Driver.AttachToKart(Kart, Vector3.ZERO);
			Kart.Player = this;
			Driver.Player = this;

			Character = eventArgs.Request.CharacterNames[id];

			Kart.OwnerID = id;
			ID = id;
		}

		/// <summary>
		/// Uses an item
		/// </summary>
		protected abstract void UseItem();


		#region key events
		// it's very important that these are run before any of the "override" methods do anything else

		protected virtual void OnStartAccelerate() {

		}
		protected virtual void OnStopAccelerate() {
		}


		protected virtual void OnStartDrift() {
		}
		protected virtual void OnStopDrift() {
		}


		protected virtual void OnStartReverse() {
		}
		protected virtual void OnStopReverse() {
		}


		protected virtual void OnStartTurnLeft() {
		}
		protected virtual void OnStopTurnLeft() {
		}


		protected virtual void OnStartTurnRight() {
		}
		protected virtual void OnStopTurnRight() {
		}
		#endregion



		#region shortcuts
		/// <summary>
		/// Gets the kart's SceneNode
		/// </summary>
		public SceneNode Node { get { return Kart.RootNode; } }
		/// <summary>
		/// Gets the kart's Body
		/// </summary>
		public RigidBody Body { get { return Kart.Body; } }
		/// <summary>
		/// Gets the kart's Node's position. No setter because it's automatically changed to whatever the position of its
		/// body is - use the <see cref="BulletSharp.RigidBody"/> if you want to change the kart's position!
		/// </summary>
		public Vector3 NodePosition {
			get { return Kart.RootNode._getDerivedPosition(); }
		}
		/// <summary>
		/// Gets the kart's SceneNode's orientation
		/// </summary>
		public Quaternion Orientation { get { return Kart.RootNode._getDerivedOrientation(); } }
		#endregion


		public virtual void Detach() {
			Kart = null;
			Driver = null;
		}
	}
}
