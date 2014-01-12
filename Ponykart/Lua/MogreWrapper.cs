using LuaNetInterface;
using Mogre;
using Ponykart.Actors;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class MogreWrapper {

		public MogreWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("vector", "Creates a Mogre.Vector3 object", "x", "y", "z")]
		public static Vector3 vec(float x, float y, float z) {
			return new Vector3(x, y, z);
		}

		// shorter form. The wrapper won't let us have two attributes on one method, so we have to make a duplicate method.
		[LuaFunction("vec", "Creates a Mogre.Vector3 object", "x", "y", "z")]
		public static Vector3 vec2(float x, float y, float z) {
			return vec(x, y, z);
		}

		[LuaFunction("vectorToQuaternion", "Turns a degree vector into a global quaternion", "Vector3 vec")]
		public static Quaternion vec2quat(Vector3 vec) {
			return vec.DegreeVectorToGlobalQuaternion();
		}

		[LuaFunction("addVectors", "one + two", "Vector3 one", "Vector3 two")]
		public static Vector3 addVec(Vector3 vec1, Vector3 vec2) {
			return vec1 + vec2;
		}

		[LuaFunction("subtractVectors", "one - two", "Vector3 one", "Vector3 two")]
		public static Vector3 subtractVec(Vector3 vec1, Vector3 vec2) {
			return vec1 - vec2;
		}

		[LuaFunction("multiplyVectors", "one * two", "Vector3 one", "Vector3 two")]
		public static Vector3 multiplyVec(Vector3 vec1, Vector3 vec2) {
			return vec1 * vec2;
		}

		[LuaFunction("scaleVector", "one * two", "Vector3 one", "float two")]
		public static Vector3 multiplyVec(Vector3 vec1, float scalar) {
			return vec1 * scalar;
		}

		// ------------------------------------

		[LuaFunction("quaternion", "Creates a Mogre.Quaternion object", "x", "y", "z", "w")]
		public static Quaternion quat(float x, float y, float z, float w) {
			return new Quaternion(w, x, y, z);
		}

		// shorter form. The wrapper won't let us have two attributes on one method, so we have to make a duplicate method.
		[LuaFunction("quat", "Creates a Mogre.Quaternion object", "x", "y", "z", "w")]
		public static Quaternion quat2(float x, float y, float z, float w) {
			return quat(w, x, y, z);
		}

		[LuaFunction("multiplyQuaternions", "Multiplies two quaternions", "quat 1", "quat 2")]
		public static Quaternion multiplyQuat(Quaternion quat1, Quaternion quat2) {
			return quat1 * quat2;
		}

		[LuaFunction("quaternionFromAngleAxis", "Creates a quaternion from an angle and an axis", "float - angle, in radians", "Vector3 - the axis")]
		public static Quaternion quatFromAngleAxis(float radians, Vector3 axis) {
			return new Quaternion(radians, axis);
		}

		// ------------------------------------

		[LuaFunction("createParticleSystem", "Creates a particle system", "string - The name of the particle system", "string - The particle template to use")]
		public static ParticleSystem createParticleSystem(string name, string template) {
			return LKernel.GetG<SceneManager>().CreateParticleSystem(name, template);
		}

		[LuaFunction("getRootSceneNode", "Gets the root scene node")]
		public static SceneNode getRootSceneNode() {
			return LKernel.GetG<SceneManager>().RootSceneNode;
		}

		[LuaFunction("getRoot", "Gets the Ogre Root singleton")]
		public static Root getRoot() {
			return LKernel.GetG<Root>();
		}

		// ------------------------------------

		[LuaFunction("initResourceGroup", "Initialises a mogre resource group", "string resGroup")]
		public static void InitialiseResourceGroup(string resGroup) {
			ResourceGroupManager.Singleton.InitialiseResourceGroup(resGroup);
		}

		[LuaFunction("initAllResourceGroups", "Initialises all mogre resource groups")]
		public static void InitAllResGroups() {
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
		}

		// ------------------------------------

		[LuaFunction("setInstancedGeometryVisibility", "Sets the visibility of all of the instanced geometry in a map region.",
			"string regionName - the name of the region. Case insensitive.", "bool visible")]
		public static void SetInstancedGeometryVisibility(string regionName, bool visible) {
			LKernel.GetG<InstancedGeometryManager>().SetVisibility(regionName, visible);
		}

		[LuaFunction("setStaticGeometryVisibility", "Sets the visibility of all of the static geometry in a map region.",
			"string regionName - the name of the region. Case insensitive.", "bool visible")]
		public static void SetStaticGeometryVisibility(string regionName, bool visible) {
			LKernel.GetG<StaticGeometryManager>().SetVisibility(regionName, visible);
		}

		[LuaFunction("setImposterVisibility", "Sets the visibility of all of the imposter billboards in a map region.",
			"string regionName - the name of the region. Case sensitive!", "bool visible")]
		public static void SetImposterVisibility(string regionName, bool visible) {
			LKernel.GetG<ImposterBillboarder>().SetVisibility(regionName, visible);
		}

		[LuaFunction("setRegionNodeVisibility", "Sets the visibility of the scene node for the specified map region.", 
			"string regionName - the name of the region. Case sensitive!", "bool visible")]
		public static void SetRegionNodeVisibility(string regionName, bool visible) {
			LKernel.GetG<SceneManager>().GetSceneNode(regionName + "Node").SetVisible(visible);
		}

		// ------------------------------------

		[LuaFunction("setMaterial", "Sets all of the model components of the given LThing to use the new material.", "LThing thing", "string newMaterial")]
		public static void SetMaterial(LThing thing, string newMaterial) {
			foreach (ModelComponent mc in thing.ModelComponents) {
				mc.Entity.SetMaterialName(newMaterial);
			}
		}

		[LuaFunction("setOneMaterial", "Sets the model component with the given ID of the given LThing to use the new material.",
			"LThing thing", "int componentID", "string newMaterial")]
		public static void SetOneMaterial(LThing thing, int componentID, string newMaterial) {
			thing.ModelComponents[componentID].Entity.SetMaterialName(newMaterial);
		}

		[LuaFunction("setSubMaterial", "Sets the subentities with the given ID of the model components of the given LThing to use the new material.",
			"LThing thing", "int subEntityID", "string newMaterial")]
		public static void SetSubMaterial(LThing thing, int subEntityID, string newMaterial) {
			foreach (ModelComponent mc in thing.ModelComponents) {
				if (mc.Entity.NumSubEntities > subEntityID)
					mc.Entity.GetSubEntity((uint) subEntityID).SetMaterialName(newMaterial);
			}
		}

		[LuaFunction("setOneSubMaterial", "Sets the subentity of the model component with the given ID of the given LThing to use the new material.",
			"LThing thing", "int componentID", "int subEntityID", "string newMaterial")]
		public static void SetOneSubMaterial(LThing thing, int componentID, int subEntityID, string newMaterial) {
			thing.ModelComponents[componentID].Entity.GetSubEntity((uint) subEntityID).SetMaterialName(newMaterial);
		}
	}
}
