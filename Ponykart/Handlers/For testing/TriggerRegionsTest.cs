using System;
using BulletSharp;
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// just a little test for the trigger regions
	/// </summary>
	//[Handler(HandlerScope.Level, LevelType.Race)]
	public class TriggerRegionsTest : ILevelHandler {
		TriggerRegion tr1, tr2, tr3;

		/// <summary>
		/// Make some trigger regions - a long box, a sphere, and a rotated box
		/// </summary>
		public TriggerRegionsTest() {
			var reporter = LKernel.GetG<TriggerReporter>();

			tr1 = new TriggerRegion("test trigger area", new Vector3(5, 3, 30), new BoxShape(10, 3, 3));
			reporter.AddEvent(tr1.Name, doSomething);

#if DEBUG
			tr2 = new TriggerRegion("test trigger area 2", new Vector3(-5, 2, 7), new SphereShape(1));
			reporter.AddEvent(tr2.Name, doSomething);

			tr3 = new TriggerRegion("test trigger area 3", new Vector3(-35, 3.5f, 0), new Vector3(0, 45, 0).DegreeVectorToGlobalQuaternion(), new BoxShape(4, 4, 4));
			reporter.AddEvent(tr3.Name, doSomething);
#endif
		}

		/// <summary>
		/// output some text, show/hide a dialogue, and change the color of the region
		/// </summary>
		void doSomething(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info) {
			if (flags.HasFlag(TriggerReportFlags.Enter)) {
				Console.WriteLine(otherBody.GetName() + " has entered trigger area \"" + region.Name + "\"");
				// cycle through the balloon colors
				region.CycleToNextColor();
			}
			else {
				Console.WriteLine(otherBody.GetName() + " has left trigger area \"" + region.Name + "\"");
				region.CycleToNextColor();
			}
		}

		/// <summary>
		/// Remove the events
		/// </summary>
		public void Detach() {
			var reporter = LKernel.GetG<TriggerReporter>();

			reporter.RemoveEvent(tr1.Name, doSomething);
#if DEBUG
			reporter.RemoveEvent(tr2.Name, doSomething);
			reporter.RemoveEvent(tr3.Name, doSomething);
#endif
		}
	}
}
