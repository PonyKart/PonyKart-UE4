using BulletSharp;
using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class PhysicsWrapper {

		public PhysicsWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("getBodyName", "Gets the name of this body.", "The body you want to get the name of.")]
		public static string GetBodyName(CollisionObject obj) {
			return obj.GetName();
		}

		[LuaFunction("getCollisionGroup", "Gets the collision group of this body.", "The body you want to get the collision group of.")]
		public static PonykartCollisionGroups GetCollisionGroup(CollisionObject obj) {
			return obj.GetCollisionGroup();
		}

		[LuaFunction("addConstraint", "Adds a constraint to the physics world", "TypedConstraint - The constraint to add", "bool - disable collisions between affected bodies?")]
		public static void AddConstraint(TypedConstraint constraint, bool disableCollisionsBetweenBodies) {
			LKernel.GetG<PhysicsMain>().World.AddConstraint(constraint, disableCollisionsBetweenBodies);
		}

		[LuaFunction("setBodyOrientation", "Sets the orientation of a RigidBody", "RigidBody", "Quaternion")]
		public static void SetBodyOrientation(RigidBody body, Quaternion quat) {
			body.SetOrientation(quat);
		}

		[LuaFunction("setBodyPosition", "Sets the position of a RigidBody", "RigidBody", "Vector3")]
		public static void SetBodyPosition(RigidBody body, Vector3 vec) {
			body.WorldTransform.SetTrans(vec);
		}

		[LuaFunction("hingeConstraint", "Connects two bodies with a hinge constraint",
			"RigidBody body1", "RigidBody body2", "Vector3 pivotOn1", "Vector3 pivotOn2", "Vector3 axisOn1", "Vector3 axisOn2")]
		public static HingeConstraint HingeConstraint(RigidBody body1, RigidBody body2, Vector3 pivotOn1, Vector3 pivotOn2, Vector3 axisOn1, Vector3 axisOn2) {
			return new HingeConstraint(body1, body2, pivotOn1, pivotOn2, axisOn1, axisOn2);
		}

		/*
		 * ActiveTag = 1,
		 * IslandSleeping = 2,
		 * WantsDeactivation = 3,
		 * DisableDeactivation = 4,
		 * DisableSimulation = 5,
		 */
		[LuaFunction("forceActivationState", "Forces the activation state of a RigidBody to be something.", "", "")]
		public static void ForceActivationState(LThing thing, int activationState) {
			if (thing != null && thing.Body != null)
				thing.Body.ForceActivationState((ActivationState) activationState);
		}

		[LuaFunction("deactivateThing", "Deactivates a LThing's physics body. This means it won't move until something else (that's moving) comes near it.",
			"LThing - the thing to deactivate")]
		public static void DeactivateThing(LThing thing) {
			if (thing != null && thing.Body != null)
				thing.Body.ForceActivationState(ActivationState.WantsDeactivation);
		}

		// ------------------------------------

		[LuaFunction("hookFunctionToCollisionReport", "Hooks a function to the collision reporter's event stuff",
			"int firstType - the first collision group to listen for", "int secondType - the second collision group to listen for",
			"function(CollisionReportInfo)")]
		public static void HookFunctionToCollisionReport(int firstType, int secondType, CollisionReportEvent handler) {
			LKernel.GetG<CollisionReporter>().AddEvent(firstType, secondType, handler);
		}
	}
}
