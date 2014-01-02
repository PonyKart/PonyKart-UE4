using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : LDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }

#region properties
#region readonly properties
		/// <summary>
		/// The radius of the wheel
		/// </summary>
		public readonly float DefaultRadius; // 0.5 (lymph)
		/// <summary>
		/// The width of the wheel
		/// </summary>
		public readonly float DefaultWidth; // 0.4 (demo)
		/// <summary>
		/// The length of the suspension when it's fully extended
		/// </summary>
		protected readonly float DefaultSuspensionRestLength; // 0.6 (demo) // 0.3 (zg)

		/// <summary>
		/// The stiffness of the suspension spring. Higher values can help make the suspension return to rest length more quickly,
		/// but can also make the suspension oscillate.
		/// </summary>
		protected readonly float DefaultSpringStiffness; // 100 (zg)
		/// <summary>
		/// I have no idea what this does, since changing it doesn't seem to have any effect on the suspension whatsoever
		/// </summary>
		protected readonly float DefaultSpringCompression; // 4.2 (zg)
		/// <summary>
		/// How much force the suspension spring can absorb before returning to rest length. Higher numbers can make the kart smoother,
		/// but too high and the kart starts to shake (around 100)
		/// </summary>
		protected readonly float DefaultSpringDamping; // 20 (zg)
		/// <summary>
		/// The friction of the wheel. Higher numbers give more friction, 0 gives no friction.
		/// </summary>
		public readonly float DefaultFrictionSlip; // 1000 (demo)
		/// <summary>
		/// How much the wheel resists rolling (around its "forward" axis). Lower numbers give more resistance.
		/// </summary>
		protected readonly float DefaultRollInfluence; // 1 (zg)

		/// <summary>
		/// How much force the wheel exerts when braking.
		/// </summary>
		protected readonly float DefaultBrakeForce;
		/// <summary>
		/// How much force the wheel's motor exerts, if it's a back wheel.
		/// </summary>
		protected readonly float DefaultMotorForce;
		/// <summary>
		/// (RADIANS) The maximum amount the wheel can turn by, if it's a front wheel.
		/// </summary>
		protected readonly float DefaultMaxTurnAngle; // 0.3 rads (demo)
		/// <summary>
		/// Which way is "up"?
		/// </summary>
		protected readonly Vector3 WheelDirection = Vector3.NEGATIVE_UNIT_Y;
		/// <summary>
		/// Which axis does the wheel rotate around?
		/// </summary>
		protected readonly Vector3 WheelAxle = Vector3.NEGATIVE_UNIT_X;
		/// <summary>
		/// any slower than this and you will have the fully multiplied turn angle
		/// </summary>
		protected readonly float DefaultSlowSpeed;
		/// <summary>
		/// any faster than this and you will have the regular turn angle
		/// </summary>
		protected readonly float DefaultHighSpeed;
		/// <summary>
		/// how much should the wheel's turn angle increase by at slow speeds?
		/// </summary>
		protected readonly float DefaultSlowTurnAngleMultiplier;
		/// <summary>
		/// how much should the wheel's rotation speed increase by at slow speeds?
		/// </summary>
		protected readonly float DefaultSlowTurnSpeedMultiplier;
		/// <summary>
		/// how much should the wheel's turn anglebe when drifting?
		/// </summary>
		protected readonly float DefaultDriftingTurnAngle;
		/// <summary>
		/// (RADIANS) how much to increment the wheel's angle by, each frame, when drifting
		/// </summary>
		protected readonly float DefaultDriftingTurnSpeed;
		/// <summary>
		/// (RADIANS) how much to increment the wheel's angle by, each frame
		/// </summary>
		protected readonly float DefaultSteerIncrementTurn;
		/// <summary>
		/// (RADIANS) how much to decrement the wheel's angle by, each frame
		/// </summary>
		protected readonly float DefaultSteerDecrementTurn;
#endregion

		/// <summary>
		/// Lets us keep track of which wheel this is on the kart
		/// </summary>
		public WheelID ID { get; private set; }
		/// <summary>
		/// Since we want the ID number of this wheel in int form so much, this is used to keep track of it without casting it every time.
		/// </summary>
		public readonly int IntWheelID;
		/// <summary>
		/// Keeps track of whether we're drifting or not, and if we are, which direction we're moving in.
		/// </summary>
		public WheelDriftState DriftState { get; set; }
		/// <summary>
		/// The point on the kart where this wheel is connected.
		/// </summary>
		protected Vector3 AxlePoint;

		// we use these three things to control the wheels
		/// <summary>
		/// 1 for forwards, -1 for backwards, 0 for no torque
		/// </summary>
		public float AccelerateMultiplier { get; set; }
		/// <summary>
		/// 1 for left, -1 for right
		/// </summary>
		public float TurnMultiplier { get; set; }
		/// <summary>
		/// true for brake on, false for brake off
		/// </summary>
		public bool IsBrakeOn { get; set; }
		/// <summary>
		/// (RADIANS) the angle the wheel should try to be at when they aren't turning
		/// </summary>
		public float IdealSteerAngle { get; set; }
		/// <summary>
		/// The current friction of the wheel
		/// </summary>
		public float Friction { get; set; } 

		readonly Kart kart;
		readonly RaycastVehicle vehicle;
#endregion

		/// <summary>
		/// This should only be used by the WheelFactory
		/// </summary>
		/// <param name="owner">Which kart is the wheel attached to?</param>
		/// <param name="connectionPoint">Where is the wheel attached?</param>
		/// <param name="wheelID">ID number of the wheel</param>
		/// <param name="dict">The properties and values from the .wheel file this wheel was built from</param>
		/// <param name="meshName">The filename of the mesh we should use for this wheel</param>
		public Wheel(Kart owner, Vector3 connectionPoint, WheelID wheelID, IDictionary<string, float> dict, string meshName) {
			// set up these
			kart = owner;
			ID = wheelID;
			vehicle = kart.Vehicle;

			// set up our readonlies
			DefaultRadius = dict["Radius"];
			DefaultWidth = dict["Width"];
			DefaultSuspensionRestLength = dict["SuspensionRestLength"];
			DefaultSpringStiffness = dict["SpringStiffness"];
			DefaultSpringCompression = dict["SpringCompression"];
			DefaultSpringDamping = dict["SpringDamping"];
			Friction = dict["FrictionSlip"];
			DefaultRollInfluence = dict["RollInfluence"];
			DefaultBrakeForce = dict["BrakeForce"];
			DefaultMotorForce = dict["MotorForce"];
			DefaultMaxTurnAngle = new Degree(dict["TurnAngle"]).ValueRadians;
			DefaultSlowSpeed = dict["SlowSpeed"];
			DefaultHighSpeed = dict["HighSpeed"];
			DefaultSlowTurnAngleMultiplier = dict["SlowTurnAngleMultiplier"];
			DefaultSlowTurnSpeedMultiplier = dict["SlowTurnSpeedMultiplier"];
			DefaultDriftingTurnAngle = new Degree(dict["DriftingTurnAngle"]).ValueRadians;
			DefaultDriftingTurnSpeed = new Degree(dict["DriftingTurnSpeed"]).ValueRadians;
			DefaultSteerIncrementTurn = new Degree(dict["SteerIncrementTurn"]).ValueRadians;
			DefaultSteerDecrementTurn = new Degree(dict["SteerDecrementTurn"]).ValueRadians;

			// give our fields some default values
			AccelerateMultiplier = 0;
			TurnMultiplier = 0;
			IsBrakeOn = false;
			DriftState = WheelDriftState.None;
			IntWheelID = (int) wheelID;
			DefaultFrictionSlip = Friction;
			IdealSteerAngle = 0f;

			// need to tell bullet whether it's a front wheel or not
			bool isFrontWheel;
			if (ID == WheelID.FrontLeft || ID == WheelID.FrontRight)
				isFrontWheel = true;
			else
				isFrontWheel = false;

			vehicle.AddWheel(connectionPoint, WheelDirection, WheelAxle, DefaultSuspensionRestLength, DefaultRadius, kart.Tuning, isFrontWheel);

			WheelInfo info = vehicle.GetWheelInfo(IntWheelID);
			info.SuspensionStiffness = DefaultSpringStiffness;
			info.WheelDampingRelaxation = DefaultSpringDamping;
			info.WheelDampingCompression = DefaultSpringCompression;
			info.FrictionSlip = Friction;
			info.RollInfluence = DefaultRollInfluence;

			AxlePoint = connectionPoint + new Vector3(0, -DefaultSuspensionRestLength, 0);

			// create our node and entity
			Node = owner.RootNode.CreateChildSceneNode("wheelNode" + kart.ID + ID, AxlePoint);
			Entity = LKernel.GetG<SceneManager>().CreateEntity("wheelNode" + kart.ID + ID, meshName);
			Node.AttachObject(Entity);
			Node.InheritOrientation = false;

			Node.Orientation = kart.ActualOrientation;

			// and then hook up to the event
			PhysicsMain.PostSimulate += PostSimulate;
		}

		/// <summary>
		/// Update our node's position and orientation and accelerate/brake/turn if we aren't paused
		/// </summary>
		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!Pauser.IsPaused) {
				WheelInfo info = kart.Vehicle.GetWheelInfo(IntWheelID);
				float currentSpeed = vehicle.CurrentSpeedKmHour;

				if (kart.Body.IsActive && (kart.VehicleSpeed > 1f || kart.VehicleSpeed < -1f)) {
					// don't change the kart's orientation when we're drifting
					if (kart.IsDriftingAtAll) {
						Node.Orientation = kart.ActualOrientation;
					}
					else {
						Node.Orientation = info.WorldTransform.ExtractQuaternion();
					}
				}
				else {
					// TODO: fix it so wheels aren't spinning when we're stopped, but they can still move left and right
					Node.Orientation = info.WorldTransform.ExtractQuaternion();
				}

				// the wheel sorta "comes off" when it's moving quickly in the air, so we only need to update the z translation then
				if (!kart.IsInAir) {
					Vector3 trans = info.WorldTransform.GetTrans();
					Node.SetPosition(AxlePoint.x, kart.RootNode.ConvertWorldToLocalPosition(trans).y, AxlePoint.z);
				}
				else {
					Node.Position = AxlePoint;
				}
				
				ChangeFriction(info, currentSpeed);
				Accelerate(currentSpeed);
				Brake(currentSpeed);
				Turn(evt.timeSinceLastFrame, currentSpeed);
			}
		}

		private const float MINIMUM_FRICTION = 3f;

		/// <summary>
		/// Changes wheel friction depending on our current speed
		/// </summary>
		protected void ChangeFriction(WheelInfo info, float currentSpeed) {
			// really strong friction so we don't roll down hills when we're stopped
			if (AccelerateMultiplier == 0f && currentSpeed > -2f && currentSpeed < 2f)
				info.FrictionSlip = 10000f;
			// if we're going slower than the slow speed, keep friction at maximum
			else if (currentSpeed < DefaultSlowSpeed)
				info.FrictionSlip = DefaultFrictionSlip;
			// otherwise, change friction based on speed to a minimum
			else {
				float newFric = this.Friction * (1 - ((currentSpeed - DefaultSlowSpeed) / ((kart.MaxSpeed * 3.6f) - DefaultSlowSpeed)));
				info.FrictionSlip = System.Math.Max(newFric, MINIMUM_FRICTION);
			}
		}

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		protected void Accelerate(float currentSpeed) {
			// if we're mostly stopped and we aren't trying to accelerate, then brake
			if (AccelerateMultiplier == 0f) {
				if (currentSpeed > -2f || currentSpeed < 2f) {
					vehicle.ApplyEngineForce(0f, IntWheelID);
					IsBrakeOn = true;
				}
				else {
					vehicle.ApplyEngineForce(0f, IntWheelID);
					IsBrakeOn = true; //false;
				}
			}
			else {
				// the wheels with motor force change depending on whether the kart is drifting or not
				// rear-wheel drive, remember!
				float _motorForce = GetMotorForceForDriftState(ID, DriftState, DefaultMotorForce);
                //Old accleration
		//vehicle.ApplyEngineForce(_motorForce * AccelerateMultiplier, IntWheelID);
                
                //Here is the new line that makes it accelerate but the max speed need to be increase inorder to make if over jumps.
                vehicle.ApplyEngineForce(_motorForce * AccelerateMultiplier * (((currentSpeed + kart.MaxSpeed *3.6f) / (kart.MaxSpeed * 3.6f))/2f), IntWheelID);
                
				// if we are trying to accelerate in the opposite direction that we're moving, then brake
				if ((AccelerateMultiplier > 0f && currentSpeed < -2f) || (AccelerateMultiplier < 0f && currentSpeed > 2f))
					IsBrakeOn = true;
				// if we're either mostly stopped or going in the correct direction, take off the brake and accelerate
				else if ((AccelerateMultiplier > 0f && currentSpeed > -2f) || (AccelerateMultiplier < 0f && currentSpeed < 2f))
					IsBrakeOn = false;
			}
		}

		/// <summary>
		/// Depending on the wheel's ID and our drift state, this determines what its motor force should be, since the karts are rear-wheel drive
		/// </summary>
		private float GetMotorForceForDriftState(WheelID id, WheelDriftState driftState, float motorForce) {
			if (driftState == WheelDriftState.None) {
				if (id == WheelID.BackLeft || id == WheelID.BackRight)
					return motorForce;
			}
			else if (driftState == WheelDriftState.Left) {
				if (id == WheelID.FrontRight || id == WheelID.BackRight)
					return motorForce;
			}
			else if (driftState == WheelDriftState.Right) {
				if (id == WheelID.FrontLeft || id == WheelID.BackLeft)
					return motorForce;
			}
			return 0f;
		}

		/// <summary>
		/// Apply some brake torque.
		/// </summary>
		protected void Brake(float currentSpeed) {
			if (IsBrakeOn) {
				// handbrake
				if (AccelerateMultiplier == 0f && (currentSpeed > -2f && currentSpeed < 2f)) {
					// the point of this is to lock the wheels in place so we don't move when we're stopped
					vehicle.SetBrake(10000f, IntWheelID);
				}
				// normal brake
				else if ((AccelerateMultiplier > 0f && currentSpeed < -2f) || (AccelerateMultiplier < 0f && currentSpeed > 2f)) {
					// brake to apply when we're changing direction
					vehicle.SetBrake(DefaultBrakeForce, IntWheelID);
				}
				// normal brake
				else {
					// brake to apply when we're just slowing down
					vehicle.SetBrake(1f/*BrakeForce * 0.25f*/, IntWheelID);
				}
			}
			else {
				vehicle.SetBrake(0f, IntWheelID);
			}
		}

		// ---------------------------------------------------------

		/// <summary>
		/// Calculates the turning multipliers, based on our current speed and drift state.
		/// </summary>
		protected void CalculateTurnMultipliers(float currentSpeed, out float turnAngleMultiplier, out float turnSpeedMultiplier) {
			if (DriftState == WheelDriftState.None) {
				// less than the slow speed = extra turn multiplier
				if (currentSpeed < DefaultSlowSpeed) {
					turnAngleMultiplier = DefaultSlowTurnAngleMultiplier;
					turnSpeedMultiplier = DefaultSlowTurnSpeedMultiplier;
				}
				// more than the high speed = no extra multiplier
				else if (currentSpeed > DefaultHighSpeed) {
					turnAngleMultiplier = 1f;
					turnSpeedMultiplier = 1f;
				}
				// somewhere in between = time for a cosine curve!
				else {
					float relativeSpeed = currentSpeed - DefaultSlowSpeed;
					float maxRelativeSpeed = DefaultHighSpeed - DefaultSlowSpeed;
					turnAngleMultiplier = 1f + (Mogre.Math.Cos((relativeSpeed * Mogre.Math.PI) / (maxRelativeSpeed * 2f)) * (DefaultSlowTurnAngleMultiplier - 1f));
					turnSpeedMultiplier = 1f + (Mogre.Math.Cos((relativeSpeed * Mogre.Math.PI) / (maxRelativeSpeed * 2f)) * (DefaultSlowTurnSpeedMultiplier - 1f));
				}
			}
			// no multiplier when we're drifting
			else {
				turnAngleMultiplier = 1f;
				turnSpeedMultiplier = 1f;
			}
		}

		/// <summary>
		/// Same as CalculateTurnMultipliers(), but only does the Angle part.
		/// </summary>
		public float CalculateTurnAngleMultiplier(float currentSpeed) {
			if (DriftState == WheelDriftState.None) {
				// less than the slow speed = extra turn multiplier
				if (currentSpeed < DefaultSlowSpeed) {
					return DefaultSlowTurnAngleMultiplier;
				}
				// more than the high speed = no extra multiplier
				else if (currentSpeed > DefaultHighSpeed) {
					return 1f;
				}
				// somewhere in between = time for a cosine curve!
				else {
					float relativeSpeed = currentSpeed - DefaultSlowSpeed;
					float maxRelativeSpeed = DefaultHighSpeed - DefaultSlowSpeed;
					return 1f + (Mogre.Math.Cos((relativeSpeed * Mogre.Math.PI) / (maxRelativeSpeed * 2f)) * (DefaultSlowTurnAngleMultiplier - 1f));
				}
			}
			// no multiplier when we're drifting
			else {
				return 1f;
			}
		}


		/// <summary>
		/// Calculates the angle the wheel should try to be at (in radians).
		/// Use this one if you already know what the turnAngleMultiplier is.
		/// </summary>
		/// <param name="turnAngleMultiplier"></param>
		/// <returns>Radians</returns>
		public float CalculateTurnAngle(float turnAngleMultiplier) {
			bool isFrontWheel = vehicle.GetWheelInfo(IntWheelID).IsFrontWheel;
			if (DriftState == WheelDriftState.None && (ID == WheelID.FrontLeft || ID == WheelID.FrontRight)) {
				// front wheels, no drift
				// calculate what angle the wheels should try to be at
				return (DefaultMaxTurnAngle * TurnMultiplier * turnAngleMultiplier) + IdealSteerAngle;
			}
			else if ((DriftState == WheelDriftState.Left && (ID == WheelID.FrontLeft || ID == WheelID.BackLeft))
				 || (DriftState == WheelDriftState.Right && (ID == WheelID.FrontRight || ID == WheelID.BackRight))) {
				// "front" wheels, yes drift
				return (DefaultDriftingTurnAngle * TurnMultiplier) + IdealSteerAngle;
			}
			else {
				// back wheels
				return IdealSteerAngle;
			}
		}

		const float STOP_DRIFT_STEER_CHANGE = 0.0104719755f /*(0.6 degrees)*/ * 0.5f * 5f /*(slow turn angle multiplier)*/;
		const float START_DRIFT_STEER_CHANGE = 0.0104719755f /*(0.6 degrees)*/ * 3f * 5f /*(slow turn angle multiplier)*/;

		/// <summary>
		/// now we have to figure out how much we have to change by.
		/// smooth out the turning
		/// </summary>
		protected float CalculateSteerChange(float targetSteerAngle, float speedTurnSpeedMultiplier, float currentAngle, float timestep) {
			if (DriftState == WheelDriftState.None) {
				if (kart.DriftState.IsStopDrift()) {
					return STOP_DRIFT_STEER_CHANGE * timestep;
				}
				else if (System.Math.Abs(targetSteerAngle - IdealSteerAngle) < System.Math.Abs(currentAngle - IdealSteerAngle)) {
					// we are not turning any more, so the wheels are moving back to their forward positions
					return DefaultSteerDecrementTurn * speedTurnSpeedMultiplier * timestep;
				}
				else {
					// we are turning, so the wheels are moving to their turned positions
					return DefaultSteerIncrementTurn * speedTurnSpeedMultiplier * timestep;
				}
			}
			else {
				if (kart.DriftState.IsStartDrift()) {
					return START_DRIFT_STEER_CHANGE * timestep;
				}
				return DefaultDriftingTurnSpeed * timestep;
			}
		}


		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		protected void Turn(float timeSinceLastFrame, float currentSpeed) {
			float speedTurnSpeedMultiplier;
			// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
			float speedTurnAngleMultiplier;

			CalculateTurnMultipliers(currentSpeed, out speedTurnAngleMultiplier, out speedTurnSpeedMultiplier);

			// pick whether the wheel can turn or not depending on whether it's drifting
			// only "front" wheels turn!
			float targetSteerAngle = CalculateTurnAngle(speedTurnAngleMultiplier);


			float currentAngle = vehicle.GetSteeringValue(IntWheelID);
			// now we have to figure out how much we have to change by
			// smooth out the turning
			float steerChange = CalculateSteerChange(targetSteerAngle, speedTurnSpeedMultiplier, currentAngle, timeSinceLastFrame * 100f);


			// turn the wheels! All of the logic here makes sure that we don't turn further than the wheel can turn,
			// and don't turn too far when we're straightening out


			if (currentAngle < targetSteerAngle) {
				if (currentAngle + steerChange <= targetSteerAngle)
					vehicle.SetSteeringValue(currentAngle + steerChange, IntWheelID);
				// close enough to the ideal angle, so we snap
				else if (currentAngle + steerChange > targetSteerAngle)
					vehicle.SetSteeringValue(targetSteerAngle, IntWheelID);
			}
			else if (currentAngle > targetSteerAngle) {
				if (currentAngle - steerChange >= targetSteerAngle)
					vehicle.SetSteeringValue(currentAngle - steerChange, IntWheelID);
				// can't decrement any more to the ideal angle, so we snap
				else if (currentAngle - steerChange < targetSteerAngle)
					vehicle.SetSteeringValue(targetSteerAngle, IntWheelID);
			}
		}

		/// <summary>
		/// clean up stuff
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			PhysicsMain.PostSimulate -= PostSimulate;

			Node.Dispose();
			Node = null;
			Entity.Dispose();
			Entity = null;

			base.Dispose(disposing);
		}
	}
}
