using System;
using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;

namespace Ponykart.Handlers {
	/// <summary>
	/// This handler helps karts with drifting, mostly involving rotating them and lowering their friction in the drifting "setup" and "finish"
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DriftingHandler : ILevelHandler {
		/// <summary>
		/// Need to keep track of which nlerpers are ours and which ones we're using
		/// </summary>
		IList<Nlerper> startNlerpers, stopNlerpers;
		readonly LThingHelperManager helperMgr = LKernel.GetG<LThingHelperManager>();


		public DriftingHandler() {

			startNlerpers = new List<Nlerper>();
			stopNlerpers = new List<Nlerper>();

			Kart.OnStartDrifting += OnStartDrifting;
			Kart.OnStopDrifting += OnStopDrifting;
			Nlerper.Finished += NlerperFinished;
			KartHandler.OnGround += OnGround;
		}

		/// <summary>
		/// If we're going in reverse or start moving slowly, stop drifting.
		/// </summary>
		void OnGround(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			if (kart.IsCompletelyDrifting && kart.VehicleSpeed < 20)
				kart.StopDrifting();
		}

		/// <summary>
		/// This helps "set up" the drifting - rotates the kart appropriately and lowers the friction while doing so.
		/// </summary>
		void OnStartDrifting(Kart kart) {
			startNlerpers.Add(helperMgr.CreateNlerper(kart, 0.2f, makeNewOrientation(kart, StartOrStopState.StartDrifting)));
			helperMgr.CreateSkidder(kart, 0.5f);
		}

		/// <summary>
		/// When a nlerper is finished doing its job, we want the kart to start drifting.
		/// This method listens for when the nlerpers finish, and if it's a nlerper we're interested in, do something with it!
		/// </summary>
		/// <param name="nlerper">keep in mind this nlerper has already been removed from LThingHelperManager</param>
		void NlerperFinished(Nlerper nlerper, LThing thing) {
			Kart kart = thing as Kart;
			if (kart != null) {
				// first see if it's a nlerper used to start off the drifting
				int index = startNlerpers.IndexOf(nlerper);
				if (index != -1) {
					// okay so that means we need to start actually drifting now!
					//kart.ForEachWheel(w => w.Friction = w.FrictionSlip);
					kart.StartActuallyDrifting();
					// remove it
					startNlerpers.RemoveAt(index);
				}
				// nope
				else {
					// so now see if it's a nlerper used to finish the drifting
					index = stopNlerpers.IndexOf(nlerper);
					if (index != -1) {
						// now we need to finish up
						//kart.ForEachWheel(w => w.Friction = w.FrictionSlip);
						kart.FinishDrifting();
						// remove it
						stopNlerpers.RemoveAt(index);
					}
				}
			}
		}

		/// <summary>
		/// This happens right at the end and helps "finish" the drifting - rotates the kart appropriately and lowers the friction while doing so.
		/// </summary>
		void OnStopDrifting(Kart kart) {
			// making it nlerp without actually changing its orientation is a way of "locking" its orientation for a duration
			stopNlerpers.Add(helperMgr.CreateNlerper(kart, 0.25f, kart.ActualOrientation /*makeNewOrientation(kart, StartOrStopState.StopDrifting)*/));
			helperMgr.CreateSkidder(kart, 0.4f);
		}

		/// <summary>
		/// Just a little helper function to make a new orientation for us to use for our nlerpers
		/// </summary>
		/// <param name="kart">The kart we want to rotate</param>
		/// <returns>A quaternion representing the new orientation you want the kart to be when the nlerper's done.</returns>
		private Quaternion makeNewOrientation(Kart kart, StartOrStopState state) {
			// make the angle we need to rotate the kart by
			Radian angle;
			if ((state == StartOrStopState.StartDrifting && kart.DriftState == KartDriftState.StartLeft)
				|| (state == StartOrStopState.StopDrifting && kart.DriftState == KartDriftState.StopRight))
			{
				angle = -kart.DriftTransitionAngle;
			}
			else if ((state == StartOrStopState.StartDrifting && kart.DriftState == KartDriftState.StartRight)
				|| (state == StartOrStopState.StopDrifting && kart.DriftState == KartDriftState.StopLeft))
			{
				angle = kart.DriftTransitionAngle;
			}
			else
				throw new ApplicationException("How did we get here?");

			Quaternion rot = new Quaternion(angle, kart.ActualOrientation.YAxis);
			Quaternion newOrientation = kart.ActualOrientation * rot;

			return newOrientation;
		}

		public void Detach() {
			Kart.OnStartDrifting -= OnStartDrifting;
			Kart.OnStopDrifting -= OnStopDrifting;
			Nlerper.Finished -= NlerperFinished;
			KartHandler.OnGround -= OnGround;
		}

		enum StartOrStopState {
			StartDrifting,
			StopDrifting
		}
	}
}
