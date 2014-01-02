// comment this out if you want drifting to be disabled for human-controlled karts.
// useful if you want to send a version out to users while the drifting is broken
#define DRIFTING_ENABLED

using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Items;
using Ponykart.UI;


namespace Ponykart.Players {
	public class HumanPlayer : Player {
		KeyBindingManager bindings;


		public HumanPlayer(LevelChangedEventArgs eventArgs, int id)
			: base(eventArgs, id, false) {

			// hook up to input events
			bindings = LKernel.Get<KeyBindingManager>();

            LKernel.GetG<GameUIManager>().SetItemLevel(0);
            
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
            bindings.PressEventsDict[LKey.Item] += UseItem;
            //bindings.ReleaseEventsDict[LKey.=Item] += ; Might need this later
			//bindings.AxisEvents[LKey.SteeringAxis] += OnSteeringAxisMoved;
			//bindings.AxisEvents[LKey.AccelerateAxis] += OnAccelerateAxisMoved;
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
					Kart.Acceleration = 0f;
				// otherwise go forwards as normal
				else
					Kart.Acceleration = 1f;
			}
		}
		protected override void OnStopAccelerate() {
			base.OnStopAccelerate();

			if (IsControlEnabled) {
				// if reverse is still held down, then we start reversing
				if (bindings.IsKeyPressed(LKey.Reverse))
					Kart.Acceleration = -1f;
				// otherwise we just stop accelerating
				else
					Kart.Acceleration = 0f;
			}
		}


		protected override void OnStartDrift() {
			base.OnStartDrift();

#if DRIFTING_ENABLED
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
				else if (Kart.VehicleSpeed > 20f) {
					Kart.DriftState = KartDriftState.WantsDriftingButNotTurning;
				}
			}
#endif
		}
		/// <summary>
		/// cancel the drift
		/// </summary>
		protected override void OnStopDrift() {
			base.OnStopDrift();

#if DRIFTING_ENABLED
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
#endif
		}


		protected override void OnStartReverse() {
			base.OnStartReverse();

			if (IsControlEnabled) {
				// if we have both forward and reverse pressed at the same time, do nothing
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Acceleration = 0f;
				// otherwise go forwards as normal
				else
					Kart.Acceleration = -1f;
			}
		}
		protected override void OnStopReverse() {
			base.OnStopReverse();

			if (IsControlEnabled) {
				// if forward is still held down, then we start going forwards
				if (bindings.IsKeyPressed(LKey.Accelerate))
					Kart.Acceleration = 1f;
				// otherwise we just stop accelerating
				else
					Kart.Acceleration = 0f;
			}
		}


		protected override void OnStartTurnLeft() {
			base.OnStartTurnLeft();

			if (IsControlEnabled) {
#if DRIFTING_ENABLED
				// if we're waiting to drift
				if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.StartDrifting(KartDriftState.StartRight);
				}
				// normal steering
				else
#endif
				{
					// if both turns are pressed, we go straight
					if (bindings.IsKeyPressed(LKey.TurnRight))
						Kart.TurnMultiplier = 0f;
					// otherwise go left
					else
						Kart.TurnMultiplier = 1f;
				}
			}
		}
		protected override void OnStopTurnLeft() {
			base.OnStopTurnLeft();

			if (IsControlEnabled) {
				// if right is still pressed, turn right
				if (bindings.IsKeyPressed(LKey.TurnRight))
					Kart.TurnMultiplier = -1f;
				// otherwise go straight
				else
					Kart.TurnMultiplier = 0f;
			}
		}


		protected override void OnStartTurnRight() {
			base.OnStartTurnRight();

			if (IsControlEnabled) {
#if DRIFTING_ENABLED
				if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning) {
					Kart.StartDrifting(KartDriftState.StartLeft);
				}
				// normal steering
				else
#endif
				{
					// if both turns are pressed, we go straight
					if (bindings.IsKeyPressed(LKey.TurnLeft))
						Kart.TurnMultiplier = 0f;
					// otherwise go right
					else
						Kart.TurnMultiplier = -1f;
				}
			}
		}
		protected override void OnStopTurnRight() {
			base.OnStopTurnRight();

			if (IsControlEnabled) {
				// if left is still pressed, turn left
				if (bindings.IsKeyPressed(LKey.TurnLeft))
					Kart.TurnMultiplier = 1f;
				// otherwise go straight
				else
					Kart.TurnMultiplier = 0f;
			}
		}
		#endregion


		protected override void UseItem()
        {
            if (hasItem)
            {
                LKernel.GetG<GameUIManager>().SetItemLevel(0);
                LKernel.GetG<GameUIManager>().SetItemImage("none");
                LKernel.GetG<ItemManager>().SpawnItem(this, heldItem);
            }
            else
            {
                //Mogre.Vector3 pos = Kart.ActualPosition;
                //pos.y += 5;
                //LKernel.GetG<ItemManager>().RequestBox(pos);
            }
            hasItem = false;
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

		public System.Action OnSteeringAxisMoved { get; set; }

		public System.Action OnAccelerateAxisMoved { get; set; }
	}
}
