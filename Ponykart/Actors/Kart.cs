using System;
using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Physics;
using Ponykart.Players;
using PonykartParsers;

namespace Ponykart.Actors {

	public delegate void KartEvent(Kart kart);

	/// <summary>
	/// Base class for karts. -Z is forwards!
	/// </summary>
	public class Kart : LThing {
		/// <summary>
		/// A special ID number just for the karts. 0 is usually the player kart, but don't rely on this.
		/// </summary>
		public int OwnerID { get; set; }

		protected override MotionState InitializationMotionState {
			get { return new KartMotionState(this, SpawnPosition, SpawnOrientation, RootNode); }
		}
		private float _maxSpeed;
		public float MaxSpeed { get { return _maxSpeed; } set { _maxSpeed = value; MaxSpeedSquared = value * value; } }
		private float _maxReverseSpeed;
		public float MaxReverseSpeed { get { return _maxReverseSpeed; } set { _maxReverseSpeed = value; MaxReverseSpeedSquared = value * value; } }
		public float MaxSpeedSquared { get; private set; }
		public float MaxReverseSpeedSquared { get; private set; }

		public readonly float DefaultMaxSpeed;
		/// <summary>
		/// Should only be set by KartHandler
		/// </summary>
		public bool IsInAir { get; set; }
		
		public KartDriftState DriftState { get; set; }

		public Driver Driver { get; set; }
		public Player Player { get; set; }

		// our wheelshapes
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelBL { get; protected set; }
		public Wheel WheelBR { get; protected set; }

		public SceneNode LeftParticleNode { get; private set; }
		public SceneNode RightParticleNode { get; private set; }

		/// <summary> (RADIANS) The angle of the "front" wheels during drifting </summary>
		public readonly float FrontDriftAngle;
		/// <summary> (RADIANS) The angle of the "back" wheels during drifting </summary>
		public readonly float BackDriftAngle;
		/// <summary> (RADIANS) The angle the kart "jumps" through when starting drifting </summary>
		public readonly Radian DriftTransitionAngle;

		protected RaycastVehicle _vehicle;
		public RaycastVehicle Vehicle { get { return _vehicle; } }
		public RaycastVehicle.VehicleTuning Tuning { get; protected set; }
		protected VehicleRaycaster Raycaster;

		private readonly LThingHelperManager helperMgr = LKernel.GetG<LThingHelperManager>();


		public static event KartEvent OnStartDrifting, OnDrifting, OnStopDrifting, OnFinishDrifting;


		public Kart(ThingBlock block, ThingDefinition def) : base(block, def) {
			DefaultMaxSpeed = MaxSpeed = def.GetFloatProperty("maxspeed", 180f);
			MaxReverseSpeed = def.GetFloatProperty("maxreversespeed", 4f);

			MaxSpeedSquared = MaxSpeed * MaxSpeed;
			MaxReverseSpeedSquared = MaxReverseSpeed * MaxReverseSpeed;

			IsInAir = false;

			FrontDriftAngle = new Degree(def.GetFloatProperty("FrontDriftAngle", 46)).ValueRadians;
			BackDriftAngle = new Degree(def.GetFloatProperty("BackDriftAngle", 55)).ValueRadians;
			DriftTransitionAngle = new Degree(def.GetFloatProperty("DriftTransitionAngle", 40));
		}

		/// <summary>
		/// Make some nodes for us to attach wheel particles to
		/// </summary>
		protected override void PostInitialiseComponents(ThingBlock template, ThingDefinition def) {
			Vector3 frontleft  = def.GetVectorProperty("FrontLeftWheelPosition",  null);
			Vector3 frontright = def.GetVectorProperty("FrontRightWheelPosition", null);
			Vector3 backleft   = def.GetVectorProperty("BackLeftWheelPosition",   null);
			Vector3 backright  = def.GetVectorProperty("BackRightWheelPosition",  null);
			LeftParticleNode   = RootNode.CreateChildSceneNode(frontleft.MidPoint(backleft));
			RightParticleNode  = RootNode.CreateChildSceneNode(frontright.MidPoint(backright));
		}

		/// <summary>
		/// After we create our RigidBody, we turn it into a vehicle
		/// </summary>
		protected override void PostCreateBody(ThingDefinition def) {
			kartMotionState = MotionState as KartMotionState;

			Body.CcdMotionThreshold = 0.001f;
			Body.CcdSweptSphereRadius = 0.04f;

			Raycaster = new DefaultVehicleRaycaster(LKernel.GetG<PhysicsMain>().World);
			Tuning = new RaycastVehicle.VehicleTuning();
			_vehicle = new RaycastVehicle(Tuning, Body, Raycaster);
			_vehicle.SetCoordinateSystem(0, 1, 2); // I have no idea what this does... I'm assuming something to do with a rotation matrix?

			LKernel.GetG<PhysicsMain>().World.AddAction(_vehicle);

			var wheelFac = LKernel.GetG<WheelFactory>();
			string frontWheelName = def.GetStringProperty("FrontWheel", null);
			string backWheelName = def.GetStringProperty("BackWheel", null);
			WheelFL = wheelFac.CreateWheel(frontWheelName, WheelID.FrontLeft, this, def.GetVectorProperty("FrontLeftWheelPosition", null), def.GetStringProperty("FrontLeftWheelMesh", null));
			WheelFR = wheelFac.CreateWheel(frontWheelName, WheelID.FrontRight, this, def.GetVectorProperty("FrontRightWheelPosition", null), def.GetStringProperty("FrontRightWheelMesh", null));
			WheelBL = wheelFac.CreateWheel(backWheelName, WheelID.BackLeft, this, def.GetVectorProperty("BackLeftWheelPosition", null), def.GetStringProperty("BackLeftWheelMesh", null));
			WheelBR = wheelFac.CreateWheel(backWheelName, WheelID.BackRight, this, def.GetVectorProperty("BackRightWheelPosition", null), def.GetStringProperty("BackRightWheelMesh", null));

			LeftParticleNode.Position -= new Vector3(0, WheelBL.DefaultRadius * 0.7f, 0);
			RightParticleNode.Position -= new Vector3(0, WheelBR.DefaultRadius * 0.7f, 0);
			
			Body.LinearVelocity = new Vector3(0, 1, 0);

			PhysicsMain.FinaliseBeforeSimulation += FinaliseBeforeSimulation;
			RaceCountdown.OnCountdown += OnCountdown;
		}

		// ---------------------------------------------------------------------------

		bool _canDisableKarts = false;
		/// <summary>
		/// limit the wheels' speed!
		/// </summary>
		void FinaliseBeforeSimulation(DiscreteDynamicsWorld world, FrameEvent evt) {
			float currentSpeed = _vehicle.CurrentSpeedKmHour;
			bool isDriftingAtAll = DriftState.IsDriftingAtAll();

			if (!IsInAir) {
				// going forwards
				// using 20 because we don't need to check the kart's linear velocity if it's going really slowly
				if ((currentSpeed > 20f && !isDriftingAtAll)
					|| currentSpeed < -20f && isDriftingAtAll
					|| currentSpeed > 20f && isDriftingAtAll)
				{
					// check its velocity against the max velocity (both are squared to avoid unnecessary square roots)
					if (Body.LinearVelocity.SquaredLength > MaxSpeedSquared) {
						Vector3 vec = Body.LinearVelocity;
						vec.Normalise();
						vec *= _maxSpeed;
						Body.LinearVelocity = vec;
					}
				}
				// going in reverse, so we want to limit the speed even more
				else if (currentSpeed < -20f && !isDriftingAtAll) {
					if (Body.LinearVelocity.SquaredLength > MaxReverseSpeedSquared) {
						Vector3 vec = Body.LinearVelocity;
						vec.Normalise();
						vec *= _maxReverseSpeed;
						Body.LinearVelocity = vec;
					}
				}
				else if (currentSpeed < 4f && currentSpeed > -4f) {
					if (_canDisableKarts && _acceleration == 0f) {
						Body.ForceActivationState(ActivationState.WantsDeactivation);
					}
				}
			}
		}

		/// <summary>
		/// Give it a second or two to get the wheels in the right positions before we can deactivate the karts when they're stopped
		/// </summary>
		void OnCountdown(RaceCountdownState state) {
			if (state == RaceCountdownState.Two) {
				_canDisableKarts = true;

				RaceCountdown.OnCountdown -= OnCountdown;
			}
		}


#region drifting
		/// <summary>
		/// Start drifting in a certain direction
		/// </summary>
		/// <param name="state">This must be either StartDriftLeft or StartDriftRight</param>
		public void StartDrifting(KartDriftState state) {
			// first check to make sure we weren't passed an incorrect argument
			if (!state.IsStartDrift())
				throw new ArgumentException("You must pass either StartDriftLeft or StartDriftRight!", "state");

			if (_vehicle.CurrentSpeedKmHour < 20f || DriftState.IsDriftingAtAll())
				return;

			// update our state
			DriftState = state;

			ForEachWheel(StartDrifting_WheelFunction);

			if (OnStartDrifting != null)
				OnStartDrifting(this);
		}

		public void StartActuallyDrifting() {
			// "upgrade" our drift states
			if (DriftState == KartDriftState.StartLeft)
				DriftState = KartDriftState.FullLeft;
			else if (DriftState == KartDriftState.StartRight)
				DriftState = KartDriftState.FullRight;

			if (OnDrifting != null)
				OnDrifting(this);
		}

		/// <summary>
		/// Put this in a separate function so we can edit it at runtime, since VS doesn't like us trying to edit anonymous functions
		/// </summary>
		private void StartDrifting_WheelFunction(Wheel w) {
			// left
			if (this.DriftState == KartDriftState.StartLeft) {
				w.DriftState = WheelDriftState.Left;

				// change the back wheels' angles
				if (w.ID == WheelID.FrontRight || w.ID == WheelID.BackRight) {
					w.IdealSteerAngle = BackDriftAngle;
					_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = false;
				}
				// change the front wheels' angles
				else {
					w.IdealSteerAngle = FrontDriftAngle;
					_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = true;
				}
			}
			// right
			else if (this.DriftState == KartDriftState.StartRight) {
				w.DriftState = WheelDriftState.Right;

				// change the back wheels' angles
				if (w.ID == WheelID.FrontLeft || w.ID == WheelID.BackLeft) {
					w.IdealSteerAngle = -BackDriftAngle;
					_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = false;
				}
				// change the front wheels' angles
				else {
					w.IdealSteerAngle = -FrontDriftAngle;
					_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = true;
				}
			}
		}

		/// <summary>
		/// Stop drifting. This is run when we let go of the drift button.
		/// </summary>
		public void StopDrifting() {
			// "upgrade" our events
			if (DriftState == KartDriftState.FullLeft || DriftState == KartDriftState.StartLeft)
				DriftState = KartDriftState.StopLeft;
			else if (DriftState == KartDriftState.FullRight || DriftState == KartDriftState.StartRight)
				DriftState = KartDriftState.StopRight;

			// make the wheels back to normal
			ForEachWheel(StopDrifting_WheelFunction);

			// eeeeeveeeeeent
			if (OnStopDrifting != null)
				OnStopDrifting(this);
		}

		/// <summary>
		/// Put this in a separate function so we can edit it at runtime, since VS doesn't like us trying to edit anonymous functions
		/// </summary>
		private void StopDrifting_WheelFunction(Wheel w) {
			w.DriftState = WheelDriftState.None;
			w.IdealSteerAngle = 0f;

			if (w.ID == WheelID.FrontRight || w.ID == WheelID.FrontLeft) {
				_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = true;
			}
			else {
				_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = false;
			}
		}

		public void FinishDrifting() {
			DriftState = KartDriftState.None;

			if (OnFinishDrifting != null)
				OnFinishDrifting(this);
		}
#endregion


#region Properties
		protected float _friction;
		/// <summary>
		/// Sets the friction of the wheels
		/// </summary>
		public float WheelFriction {
			get {
				return _friction;
			}
			set {
				this._friction = value;

				ForEachWheel(w => {
					w.Friction = value;
				});
			}
		}

		protected float _acceleration;
		/// <summary>
		/// Sets the motor torque of all wheels and sets their brake torque to 0.
		/// </summary>
		public float Acceleration {
			get {
				return _acceleration;
			}
			set {
				if (value != 0f)
					Body.Activate();
				this._acceleration = value;

				ForEachWheel(w => {
					w.AccelerateMultiplier = value;
					w.IsBrakeOn = false;
				});
			}
		}

		private readonly Radian _turnMultiplierPositiveDriftDegree = new Degree(10);
		private readonly Radian _turnMultiplierNegativeDriftDegree = new Degree(-10);
		protected float _turnMultiplier;
		/// <summary>
		/// Turns the wheels
		/// 
		/// Turn left is positive, turn right is negative
		/// </summary>
		public float TurnMultiplier {
			get {
				return _turnMultiplier;
			}
			set {
				this._turnMultiplier = value;

				ForEachWheel(w => w.TurnMultiplier = value);
			}
		}

		/// <summary>
		/// shortcut
		/// </summary>
		public float VehicleSpeed {
			get {
				return _vehicle.CurrentSpeedKmHour;
			}
		}

		/// <summary>
		/// Returns true if we're completely drifting - not starting, not stopping, but in between.
		/// </summary>
		public bool IsCompletelyDrifting {
			get {
				return DriftState.IsFullDrift();
			}
		}

		/// <summary>
		/// Returns true if we're starting to drift
		/// </summary>
		public bool IsStartingDrifting {
			get {
				return DriftState.IsStartDrift();
			}
		}

		/// <summary>
		/// Returns true if we're stopping drifting
		/// </summary>
		public bool IsStoppingDrifting {
			get {
				return DriftState.IsStopDrift();
			}
		}

		/// <summary>
		/// Returns true if we're drifting at all - starting, stopping, or in between.
		/// </summary>
		public bool IsDriftingAtAll {
			get {
				return DriftState.IsDriftingAtAll();
			}
		}
#endregion

#region helpers
		/// <summary>
		/// A little helper method since we do stuff to all four wheels so often
		/// </summary>
		/// <param name="action"></param>
		public void ForEachWheel(Action<Wheel> action) {
			action(WheelFL);
			action(WheelFR);
			action(WheelBL);
			action(WheelBR);
		}

		/// <summary>
		/// Gets a wheel
		/// </summary>
		/// <param name="id">Must be between 0 and 3!</param>
		public Wheel GetWheel(int id) {
			if (id < 0 || id > 3)
				throw new ArgumentOutOfRangeException("id", "The ID number must be between 0 and 3 inclusive!");
			return GetWheel((WheelID) id);
		}

		/// <summary>
		/// Gets a wheel
		/// </summary>
		public Wheel GetWheel(WheelID wid) {
			switch (wid) {
				case WheelID.FrontLeft:
					return WheelFL;
				case WheelID.FrontRight:
					return WheelFR;
				case WheelID.BackLeft:
					return WheelBL;
				case WheelID.BackRight:
					return WheelBR;
				default:
					throw new ArgumentOutOfRangeException("wid", "Invalid wheel ID number!");
			}
		}
#endregion

#region Properties for actual and interpolated position and orientation

		KartMotionState kartMotionState;
		/// <summary>
		/// Gets the kart's actual orientation according to the physics world and not the graphics world.
		/// </summary>
		public Quaternion ActualOrientation {
			get {
				return kartMotionState.actualOrientation;
			}
		}
		/// <summary>
		/// Gets the kart's interpolated orientation according to the graphics world and not the physics world.
		/// </summary>
		public Quaternion InterpolatedOrientation {
			get {
				return RootNode.Orientation;
			}
		}
		/// <summary>
		/// Gets the kart's actual position according to the physics world and not the graphics world.
		/// </summary>
		public Vector3 ActualPosition {
			get {
				return kartMotionState.actualPosition;
			}
		}
		/// <summary>
		/// Gets the kart's interpolated position according to the graphics world and not the physics world.
		/// </summary>
		public Vector3 InterpolatedPosition {
			get {
				return RootNode.Position;
			}
		}
#endregion

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			PhysicsMain.FinaliseBeforeSimulation -= FinaliseBeforeSimulation;
			RaceCountdown.OnCountdown -= OnCountdown;

			if (disposing) {
				// then we have to dispose of all of the wheels
				WheelFL.Dispose();
				WheelFR.Dispose();
				WheelBL.Dispose();
				WheelBR.Dispose();
			}

			_vehicle.Dispose();

			base.Dispose(disposing);
		}

        /// <summary>
        /// Changes the position, velocity, and orientation according to the remote instructions
        /// </summary>
        public void SetState(Vector3 newPosition, Vector3 newLinearVelocity, Quaternion newOrientation) {
            kartMotionState.actualPosition = newPosition;
            kartMotionState.actualOrientation = newOrientation;


            Matrix4 mat = new Matrix4();
            mat.MakeTransform(newPosition, Vector3.UNIT_SCALE, newOrientation);

            Body.WorldTransform = mat;
            Body.LinearVelocity = newLinearVelocity;
            Body.Activate();
        }
    }
}
