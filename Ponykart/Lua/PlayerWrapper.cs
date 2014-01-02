using System;
using BulletSharp;
using LuaNetInterface;
using Ponykart.Actors;
using Ponykart.Handlers;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class PlayerWrapper {

		public PlayerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		// some helpers to do things to every kart or player

		[LuaFunction("forEachPlayer", "Runs a function for each player.", "function(player, int playerID)")]
		public static void ForEachPlayer(Action<Player, int> action) {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
				foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
					action.Invoke(p, p.ID);
				}
			}
		}

		[LuaFunction("forEachKart", "Runs a function for each kart.", "function(kart, int playerID)")]
		public static void ForEachKart(Action<Kart, int> action) {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
				foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
					action.Invoke(p.Kart, p.ID);
				}
			}
		}

		[LuaFunction("getKart", "Gets the kart with the given ID", "int id - 0 is the player kart")]
		public static Kart GetKart(int id) {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel && LKernel.GetG<PlayerManager>().Players.Length > id) {
				return LKernel.GetG<PlayerManager>().Players[id].Kart;
			}
			else
				return null;
		}

		[LuaFunction("playerKart", "Returns the player's kart")]
		public static Kart GetPlayerKart() {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
				return LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			}
			else
				return null;
		}

		[LuaFunction("playerDriver", "Returns the player's driver")]
		public static Driver GetPlayerDriver() {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
				return LKernel.GetG<PlayerManager>().MainPlayer.Driver;
			}
			else
				return null;
		}

		[LuaFunction("getKartHandler", "Gets the KartHandler")]
		public static KartHandler GetKartHandler() {
			return LKernel.GetG<KartHandler>();
		}

		[LuaFunction("getKartFromBody", "gets the Kart out of a RigidBody", "RigidBody")]
		public static Kart GetKartFromBody(RigidBody enteredBody) {
			if (enteredBody.UserObject is CollisionObjectDataHolder) {
				return (enteredBody.UserObject as CollisionObjectDataHolder).GetThingAsKart();
			}
			return null;
		}
	}
}
