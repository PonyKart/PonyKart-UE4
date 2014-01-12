using System.Collections.Generic;
using BulletSharp;
using LuaNetInterface;
using Mogre;
using Ponykart.Levels;
using Ponykart.Physics;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class TriggerWrapper {
		/// <summary>
		/// for cleaning up all of our events we'll have to dispose of
		/// </summary>
		static IList<Pair<TriggerRegion, TriggerReportEvent>> toDispose;

		public TriggerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);

			toDispose = new List<Pair<TriggerRegion, TriggerReportEvent>>();

			LevelManager.OnLevelUnload += OnLevelUnload;
		}

		/// <summary>
		/// Go through all of the functions we've hooked up to trigger regions and unhook them. We need to do this otherwise the
		/// regions won't be disposed properly.
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			foreach (Pair<TriggerRegion, TriggerReportEvent> pair in toDispose) {
				pair.first.OnTrigger -= pair.second;
			}
		}

		/// <summary>
		/// Shortcut method to add something to the dispose queue.
		/// </summary>
		static void AddToDispose(TriggerRegion region, TriggerReportEvent handler) {
			toDispose.Add(new Pair<TriggerRegion, TriggerReportEvent>(region, handler));
		}

		//-----------------------------------------------

		[LuaFunction("isEnterFlag", "Is this flag an enter flag?", "The flag you want to check")]
		public static bool IsEnterFlag(TriggerReportFlags flag) {
			return flag.HasFlag(TriggerReportFlags.Enter);
		}

		[LuaFunction("isLeaveFlag", "Is this flag an leave flag?", "The flag you want to check")]
		public static bool IsLeaveFlag(TriggerReportFlags flag) {
			return flag.HasFlag(TriggerReportFlags.Leave);
		}

		/// <summary>
		/// create a box trigger region
		/// </summary>
		[LuaFunction("createBoxTriggerRegion", "Creates a box trigger region given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerRegion, otherShape, triggerFlags)",
			"vector3 dimensions", "vector3 position", "quaternion orientation")]
		public static TriggerRegion CreateBoxTriggerRegion(string name, TriggerReportEvent trh, Vector3 dimensions, Vector3 position, Quaternion orientation) {
			var csm = LKernel.GetG<CollisionShapeManager>();
			CollisionShape shape;

			if (!csm.TryGetShape(name, out shape)) {
				shape = new BoxShape(dimensions);
				csm.RegisterShape(name, shape);
			}

			var tr = new TriggerRegion(name, position, orientation, shape);
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
			return tr;
		}

		/// <summary>
		/// create a capsule trigger region
		/// </summary>
		[LuaFunction("createCapsuleTriggerRegion", "Creates a capsule trigger region given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerRegion, otherShape, triggerFlags)",
			"number radius", "number height", "vector3 position", "quaternion orientation")]
		public static TriggerRegion CreateCapsuleTriggerRegion(
			string name, TriggerReportEvent trh, float radius, float height, Vector3 position, Quaternion orientation)
		{
			var csm = LKernel.GetG<CollisionShapeManager>();
			CollisionShape shape;

			if (!csm.TryGetShape(name, out shape)) {
				shape = new CapsuleShape(radius, height);
				csm.RegisterShape(name, shape);
			}

			var tr = new TriggerRegion(name, position, orientation, shape);
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
			return tr;
		}

		/// <summary>
		/// create a sphere trigger region
		/// </summary>
		[LuaFunction("createSphereTriggerRegion", "Creates a sphere trigger region given a name and some info and a function to call.",
			"string name - The name of the shape", "function() trigger report handler - (triggerRegion, otherShape, triggerFlags)",
			"number radius", "vector3 position")]
		public static TriggerRegion CreateSphereTriggerRegion(string name, TriggerReportEvent trh, float radius, Vector3 position) {
			var csm = LKernel.GetG<CollisionShapeManager>();
			CollisionShape shape;

			if (!csm.TryGetShape(name, out shape)) {
				shape = new SphereShape(radius);
				csm.RegisterShape(name, shape);
			}

			var tr = new TriggerRegion(name, position, shape);
			tr.OnTrigger += trh;
			AddToDispose(tr, trh);
			return tr;
		}

		/// <summary>
		/// Hooks up a function to a triggerregion event so it will run whenever an actor enters of leaves the specified trigger Region.
		/// </summary>
		/// <param name="nameOfRegion">The name of the trigger region</param>
		/// <param name="trh">(Shape triggerShape, Shape otherShape, TriggerFlags flags)</param>
		[LuaFunction("hookFunctionToTriggerRegion",
			"Hooks up a function to a trigger region event so it will run whenever an actor enters of leaves the specified trigger region.",
			"string nameOfRegion - The name of the trigger region",
			"function() trigger report handler - (Shape triggerShape, Shape otherShape, TriggerFlags flags)")]
		public static void HookFunctionToTriggerRegion(string nameOfRegion, TriggerReportEvent trh) {
			TriggerRegion tr = LKernel.Get<TriggerReporter>().AddEvent(nameOfRegion, trh);

			if (tr != null)
				AddToDispose(tr, trh);
		}

		[LuaFunction("getTriggerRegion", "Gets a trigger region", "string nameOfRegion")]
		public static TriggerRegion GetTriggerRegion(string nameOfRegion) {
			TriggerRegion tr;
			if (LKernel.Get<TriggerReporter>().Regions.TryGetValue(nameOfRegion, out tr))
				return tr;
			else
				return null;
		}
	}
}
