using System;
using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Properties;

namespace Ponykart.Physics {

	public delegate void TriggerReportEvent(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info);

	public class TriggerRegion : LDisposable {
		public GhostObject Ghost { get; protected set; }
		
		public string Name { get; protected set; }
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public HashSet<RigidBody> CurrentlyCollidingWith { get; set; }

		/// <summary>
		/// Invoked by TriggerReporter
		/// </summary>
		public event TriggerReportEvent OnTrigger;

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		public TriggerRegion(string name, Vector3 position, CollisionShape shape)
			: this(name, position, Quaternion.IDENTITY, shape) { }

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		/// <param name="orientation">a degree vector</param>
		public TriggerRegion(string name, Vector3 position, Quaternion orientation, CollisionShape shape) {
			Name = name;
			CurrentlyCollidingWith = new HashSet<RigidBody>();

			// mogre
			var sceneMgr = LKernel.GetG<SceneManager>();

			Node = sceneMgr.RootSceneNode.CreateChildSceneNode(name);
			if (Settings.Default.EnableGlowyRegions) {
				// make a mesh for the region depending on what its type is
				switch (shape.ShapeType) {
					case BroadphaseNativeType.BoxShape:
						Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
						Node.SetScale((shape as BoxShape).HalfExtentsWithoutMargin * 2);
						break;
					case BroadphaseNativeType.CapsuleShape:
						Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
						Vector3 vec = new Vector3();
						vec.y = (shape as CapsuleShape).HalfHeight * 2;
						vec.x = vec.z = (shape as CapsuleShape).Radius * 2;
						Node.SetScale(vec);
						break;
					case BroadphaseNativeType.CylinderShape:
						Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
						Vector3 vec2 = new Vector3();
						vec2.y = (shape as CylinderShape).HalfExtentsWithoutMargin.y;
						vec2.x = vec2.z = (shape as CylinderShape).Radius * 2;
						Node.SetScale(vec2);
						break;
					case BroadphaseNativeType.SphereShape:
						Entity = sceneMgr.CreateEntity(name, "primitives/sphere.mesh");
						float dim = (shape as SphereShape).Radius * 2;
						Node.SetScale(dim, dim, dim);
						break;
					default:
						// for things like meshes, convex hulls, etc
						Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
						break;
				}
				GlowColor = BalloonGlowColour.red;
				Entity.CastShadows = false;

				Node.AttachObject(Entity);
			}
			Node.Position = position;
			Node.Orientation = orientation;

			// physics
			Matrix4 transform = new Matrix4();
			transform.MakeTransform(position, Vector3.UNIT_SCALE, orientation);

			var motionState = new DefaultMotionState();
			motionState.WorldTransform = transform;

			// thanks to kloplop321 in #ogre3d for his help with this
		 	Ghost = new GhostObject();
			Ghost.CollisionShape = shape;
			Ghost.WorldTransform = transform;
			Ghost.UserObject = new CollisionObjectDataHolder(Ghost, PonykartCollisionGroups.Triggers, name);

			Ghost.CollisionFlags |= CollisionFlags.NoContactResponse | CollisionFlags.CustomMaterialCallback;
			LKernel.GetG<PhysicsMain>().World.AddCollisionObject(Ghost, PonykartCollisionGroups.Triggers, PonykartCollidesWithGroups.Triggers);


			// then add this to the trigger reporter
			LKernel.GetG<TriggerReporter>().Regions.Add(name, this);
		}

		/// <summary>
		/// Run the enter event
		/// </summary>
		public void InvokeTrigger(RigidBody otherBody, TriggerReportFlags flags, CollisionReportInfo info) {
			// at the moment this only triggers when the "main" shape of an actor enters. Do we want to change this?
			if (OnTrigger != null) {
#if DEBUG
				try {
#endif
					OnTrigger(this, otherBody, flags, info);
#if DEBUG
				}
				catch (Exception e) {
					Launch.Log("Exception at TriggerRegion.InvokeTrigger: " + e.Message + "  " + e.Source);
				}
#endif
			}
		}


		BalloonGlowColour _balloonColor = BalloonGlowColour.red;
		/// <summary>
		/// Must be one of: red, blue, yellow, green, orange, magenta, purple, cyan, white
		/// </summary>
		public BalloonGlowColour GlowColor {
			get {
				return _balloonColor;
			}
			set {
				_balloonColor = value;
				if (Settings.Default.EnableGlowyRegions)
					Entity.SetMaterialName("BalloonGlow_" + value);
			}
		}

		/// <summary>
		/// Changes the region's color to the next one in the cycle
		/// </summary>
		public void CycleToNextColor() {
			if (Settings.Default.EnableGlowyRegions) {
				GlowColor = (BalloonGlowColour) (((int) _balloonColor + 1) % 9);
			}
		}

		/// <summary>
		/// The width of this region, either its X width or radius.
		/// Returns 0 if the shape is not a box, capsule, cylinder, or sphere.
		/// </summary>
		public float Width {
			get {
				switch (Ghost.CollisionShape.ShapeType) {
					case BroadphaseNativeType.BoxShape:
						return (Ghost.CollisionShape as BoxShape).HalfExtentsWithoutMargin.x;
					case BroadphaseNativeType.CapsuleShape:
						return (Ghost.CollisionShape as CapsuleShape).Radius;
					case BroadphaseNativeType.CylinderShape:
						return (Ghost.CollisionShape as CylinderShape).Radius;
					case BroadphaseNativeType.SphereShape:
						return (Ghost.CollisionShape as SphereShape).Radius;
					default:
						return 0;
				}
			}
		}

		/// <summary>
		/// we can assume our trigger regions are permanent
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				var sceneMgr = LKernel.GetG<SceneManager>();
				if (Settings.Default.EnableGlowyRegions)
					sceneMgr.DestroyEntity(Entity);
				sceneMgr.DestroySceneNode(Node);
			}
			if (Settings.Default.EnableGlowyRegions) {
				Entity.Dispose();
				Entity = null;
			}

			Node.Dispose();
			Node = null;
			

			base.Dispose(disposing);
		}

		public override string ToString() {
			return Name;
		}
		public override int GetHashCode() {
			return Name.GetHashCode();
		}
	}
}
