﻿using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Players {
	public class HumanPlayer : Player {
		KeyBindingManager bindings;


		public HumanPlayer(LevelChangedEventArgs eventArgs, int id)
			: base(eventArgs, id, false) {

			// hook up to input events
			bindings = LKernel.Get<KeyBindingManager>();

			bindings.PressEventsDict[LKey.Accelerate] += OnStartAccelerate;
			bindings.ReleaseEventsDict[LKey.Accelerate] += OnStopAccelerate;
			bindings.PressEventsDict[LKey.Drift] += OnStartDrift;
			bindings.ReleaseEventsDict[LKey.Drift] += OnStopDrift;
			bindings.PressEventsDict[LKey.Reverse] += OnStartReverse;
			bindings.ReleaseEventsDict[LKey.Reverse] += OnStopReverse;
			bindings.PressEventsDict[LKey.TurnLeft] += OnStartTurnLeft;
			bindings.ReleaseEventsDict[LKey.TurnLeft] += OnStopTurnLeft;
			bindings.PressEventsDict[LKey.TurnRight] += OnStartTurnRight;
			bindings.ReleaseEventsDict[LKey.TurnRight] += OnStopTurnRight;
		}

		public override bool IsControlEnabled {
			get {
				return base.IsControlEnabled;
			}
			// we need to do this so we can immediately start moving when we regain control without having to repress keys
			set {
				base.IsControlEnabled = value;

				if (value) {
					// gain control
					if (bindings.IsKeyPressed(LKey.Accelerate))
						OnStartAccelerate();
					if (bindings.IsKeyPressed(LKey.Drift))
						OnStartDrift();
					if (bindings.IsKeyPressed(LKey.Reverse))
						OnStartReverse();
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						OnStartTurnLeft();
					if (bindings.IsKeyPressed(LKey.TurnRight))
						OnStartTurnRight();
				}
				else {
					// lose control
					if (bindings.IsKeyPressed(LKey.Accelerate))
						OnStopAccelerate();
					if (bindings.IsKeyPressed(LKey.Drift))
						OnStopDrift();
					if (bindings.IsKeyPressed(LKey.Reverse))
						OnStopReverse();
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						OnStopTurnLeft();
					if (bindings.IsKeyPressed(LKey.TurnRight))
						OnStopTurnRight();
				}
			}
		}


		#region key events
		protected override void OnStartAccelerate() {
			base.OnStartAccelerate();

			if (IsControlEnabled) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Acceleration = 0;
				// otherwise go forwards as normal
				else
					Kart.Acceleration = 1;
			}
		}
		protected override void OnStopAccelerate() {
			base.OnStopAccelerate();

			if (IsControlEnabled) {
				// if reverse is still held down, then we start reversing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Acceleration = -1;
				// otherwise we just stop accelerating
				else
					Kart.Acceleration = 0;
			}
		}


		protected override void OnStartDrift() {
			base.OnStartDrift();

			if (IsControlEnabled) {
				// if left is pressed and right isn't, start drifting left
				if (bindings.IsKeyPressed(LKey.TurnLeft) && !bindings.IsKeyPressed(LKey.TurnRight)) {
					Kart.StartDrifting(KartDriftState.StartRight);
				}
				// otherwise if right is pressed and left isn't, start drifting right
				else if (bindings.IsKeyPressed(LKey.TurnRight) && !bindings.IsKeyPressed(LKey.TurnLeft)) {
					Kart.StartDrifting(KartDriftState.StartLeft);
				}
				// otherwise it wants to drift but we don't have a direction yet
				else if (Kart.VehicleSpeed > 100) {
					Kart.DriftState = KartDriftState.WantsDriftingButNotTurning;
				}
			}
		}
		/// <summary>
		/// cancel the drift
		/// </summary>
		protected override void OnStopDrift() {
			base.OnStopDrift();

			if (IsControlEnabled) {
				// if we were drifting left
				if (Kart.DriftState == KartDriftState.FullLeft || Kart.DriftState == KartDriftState.StartLeft) {
					Kart.StopDrifting();
				}
				// if we were drifting right
				else if (Kart.DriftState == KartDriftState.FullRight || Kart.DriftState == KartDriftState.StartRight) {
					Kart.StopDrifting();
				}
				// if we had the drift button down but weren't actually drifting
				else if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.DriftState = KartDriftState.None;
				}
			}
		}


		protected override void OnStartReverse() {
			base.OnStartReverse();

			if (IsControlEnabled) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Acceleration = 0;
				// otherwise go forwards as normal
				else
					Kart.Acceleration = -1;
			}
		}
		protected override void OnStopReverse() {
			base.OnStopReverse();

			if (IsControlEnabled) {
				// if forward is still held down, then we start going forwards
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Acceleration = 1;
				// otherwise we just stop accelerating
				else
					Kart.Acceleration = 0;
			}
		}


		protected override void OnStartTurnLeft() {
			base.OnStartTurnLeft();

			if (IsControlEnabled) {
				// if we're waiting to drift
				if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.StartDrifting(KartDriftState.StartRight);
				}
				// normal steering
				else {
					// if both turns are pressed, we go straight
					if (bindings.IsKeyPressed(LKey.TurnRight))
						Kart.TurnMultiplier = 0;
					// otherwise go left
					else
						Kart.TurnMultiplier = 1;
				}
			}
		}
		protected override void OnStopTurnLeft() {
			base.OnStopTurnLeft();

			if (IsControlEnabled) {
				// if right is still pressed, turn right
				if (bindings.IsKeyPressed(LKey.TurnRight))
					Kart.TurnMultiplier = -1;
				// otherwise go straight
				else
					Kart.TurnMultiplier = 0;
			}
		}


		protected override void OnStartTurnRight() {
			base.OnStartTurnRight();

			if (IsControlEnabled) {
				if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.StartDrifting(KartDriftState.StartLeft);
				}
				// normal steering
				else {
					// if both turns are pressed, we go straight
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						Kart.TurnMultiplier = 0;
					// otherwise go right
					else
						Kart.TurnMultiplier = -1;
				}
			}
		}
		protected override void OnStopTurnRight() {
			base.OnStopTurnRight();

			if (IsControlEnabled) {
				// if left is still pressed, turn left
				if (bindings.IsKeyPressed(LKey.TurnLeft))
					Kart.TurnMultiplier = 1;
				// otherwise go straight
				else
					Kart.TurnMultiplier = 0;
			}
		}
		#endregion


		protected override void UseItem() {
			throw new System.NotImplementedException();
		}


		public override void Detach() {
			bindings.PressEventsDict[LKey.Accelerate] -= OnStartAccelerate;
			bindings.ReleaseEventsDict[LKey.Accelerate] -= OnStopAccelerate;
			bindings.PressEventsDict[LKey.Drift] -= OnStartDrift;
			bindings.ReleaseEventsDict[LKey.Drift] -= OnStopDrift;
			bindings.PressEventsDict[LKey.Reverse] -= OnStartReverse;
			bindings.ReleaseEventsDict[LKey.Reverse] -= OnStopReverse;
			bindings.PressEventsDict[LKey.TurnLeft] -= OnStartTurnLeft;
			bindings.ReleaseEventsDict[LKey.TurnLeft] -= OnStopTurnLeft;
			bindings.PressEventsDict[LKey.TurnRight] -= OnStartTurnRight;
			bindings.ReleaseEventsDict[LKey.TurnRight] -= OnStopTurnRight;

			base.Detach();
		}
	}
}
