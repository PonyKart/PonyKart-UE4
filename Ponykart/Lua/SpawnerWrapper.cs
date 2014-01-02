using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Lua {
	//[LuaPackage("Spawner", "This is a wrapper for the Spawner. Use it to spawn game objects.")]
	[LuaPackage(null, null)]
	public class SpawnerWrapper {

		public SpawnerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("spawn", "Spawns a game object! Returns null if it didn't spawn correctly",
			"string thingName - The name of the .thing you want to spawn. Not case sensitive.",
			"Vector3 pos - Spawn position")]
		public static LThing Spawn(string thingName, Vector3 pos) {
			return LKernel.Get<Spawner>().Spawn(thingName, pos);
		}

		[LuaFunction("relativeSpawn", "Spawns a game object relative to another game object. This does not return anything.", 
			"string thingName - the name of the .thing you want to spawn. Not case sensitive.",
			"LThing thing - the 'parent' game object",
			"Vector3 offset")]
		public static void RelativeSpawn(string thingName, LThing thing, Vector3 offset) {
			Vector3 pos = thing.RootNode.ConvertLocalToWorldPosition(offset);
			Spawn(thingName, pos);
		}

		[LuaFunction("spawnBgPony", "Spawns a background pony! Returns null if it didn't spawn correctly",
			"string thingName - The name of the .thing you want to spawn. Not case sensitive.",
			"Vector3 pos - Spawn position")]
		public static LThing SpawnBgPony(string thingName, Vector3 pos) {
			if (thingName == "LyraSitting")
				return LKernel.Get<Spawner>().Spawn<Lyra>(thingName, pos, (t, d) => new Lyra(t, d));
			else
				return LKernel.Get<Spawner>().Spawn<BackgroundPony>("BgPony", thingName, new ThingBlock("BgPony", pos), (n, t, d) => new BackgroundPony(n, t, d));
		}

		[LuaFunction("spawnRandomStandingBgPony", "Spawns a random standing background pony", "Vector3 pos")]
		public static BackgroundPony SpawnRandomStandingBgPony(Vector3 pos) {
			return BackgroundPony.SpawnRandomStandingPony(pos);
		}

		[LuaFunction("spawnRandomSittingBgPony", "Spawns a random sitting background pony", "Vector3 pos")]
		public static BackgroundPony SpawnRandomSittingBgPony(Vector3 pos) {
			return BackgroundPony.SpawnRandomSittingPony(pos);
		}

		[LuaFunction("spawnRandomFlyingBgPony", "Spawns a random flying background pony", "Vector3 pos")]
		public static BackgroundPony SpawnRandomFlyingBgPony(Vector3 pos) {
			return BackgroundPony.SpawnRandomFlyingPony(pos);
		}
		
		[LuaFunction("spawnBgPonyByNum", "words words", "int", "vector")]
		public static BackgroundPony SpawnBgPonyByNum(int id, Vector3 pos) {
			return BackgroundPony.SpawnByNum(id, pos);
		}

		[LuaFunction("spawnDerpy", "Spawns derpy", "Vector3 pos")]
		public static Derpy SpawnDerpy(Vector3 pos) {
			return LKernel.GetG<Spawner>().Spawn<Derpy>("Derpy", pos, (t, d) => new Derpy(t, d));
		}
	}
}
