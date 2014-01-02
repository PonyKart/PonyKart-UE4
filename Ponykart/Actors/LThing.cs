using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using Mogre;
using Ponykart.Levels;
using Ponykart.Lua;
using Ponykart.Physics;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Our game object class! Pretty much everything you see in the game uses this
	/// </summary>
	public class LThing : LDisposable {
		/// <summary>
		/// Every lthing has an ID, though it's mostly just used to stop ogre complaining about duplicate names
		/// </summary>
		public uint ID { get; protected set; }
		/// <summary>
		/// This lthing's name. It's usually the same as its .thing filename.
		/// </summary>
		public string Name { get; protected set; }
		/// <summary>
		/// Physics! If we have 0 shape components, this is null; if we have 1 shape component, this is a body made from that shape;
		/// if we have 2 or more shape components, this is a body made from a compound shape using all of the components' shapes
		/// </summary>
		public RigidBody Body { get; protected set; }
		/// <summary>
		/// A scene node that all of the model components attach stuff to.
		/// </summary>
		public SceneNode RootNode { get; protected set; }


		/// <summary>
		/// Initial motion state setter. Override this if you want something different. This is only used for initialisation!
		/// </summary>
		protected virtual MotionState InitializationMotionState {
			get {
				if (SoundComponents != null)
					return new MogreMotionState(this, SpawnPosition, SpawnOrientation, RootNode);
				else
					return new MogreMotionState(null, SpawnPosition, SpawnOrientation, RootNode);
			}
		}
		/// <summary>
		/// The actual motion state.
		/// </summary>
		protected MotionState MotionState { get; private set; }
		/// <summary>
		/// The body's collision group
		/// </summary>
		public PonykartCollisionGroups CollisionGroup { get; protected set; }
		/// <summary>
		/// What does the body collide with?
		/// </summary>
		public PonykartCollidesWithGroups CollidesWith { get; protected set; }


		/// <summary>
		/// The thing's initial position when it's first created
		/// </summary>
		public Vector3 SpawnPosition { get; private set; }
		/// <summary>
		/// The thing's initial orientation when it's first created
		/// </summary>
		public Quaternion SpawnOrientation { get; private set; }
		/// <summary>
		/// The thing's initial scale when it's first created. This is only used if it doesn't have any shape components.
		/// </summary>
		public Vector3 SpawnScale { get; private set; }


		protected RigidBodyConstructionInfo Info;
		public string Script { get; private set; }


		public List<ModelComponent> ModelComponents { get; protected set; }
		public List<ShapeComponent> ShapeComponents { get; protected set; }
		public List<RibbonComponent> RibbonComponents { get; protected set; }
		public List<BillboardSetComponent> BillboardSetComponents { get; protected set; }
		public List<SoundComponent> SoundComponents { get; protected set; }


		/// <summary>
		/// Constructor woo!
		/// </summary>
		/// <param name="template">
		/// This is the part that comes from the .muffin file (if we used one) or somewhere else in the program. It has basic information needed for constructing this
		/// lthing, such as where it should be in the world, its rotation, etc.
		/// </param>
		/// <param name="def">
		/// This is the part that comes from the .thing file. It's got all of the properties that specifies what this lthing is, what it should look like, and
		/// information about all of its components.
		/// </param>
		public LThing(ThingBlock template, ThingDefinition def) {
			ID = IDs.Incremental;
			Name = template.ThingName;

			// get our three basic transforms
			SpawnPosition = template.GetVectorProperty("position", null);

			SpawnOrientation = template.GetQuatProperty("orientation", Quaternion.IDENTITY);
			if (SpawnOrientation == Quaternion.IDENTITY)
				SpawnOrientation = template.GetVectorProperty("rotation", Vector3.ZERO).DegreeVectorToGlobalQuaternion();

			SpawnScale = template.GetVectorProperty("scale", Vector3.UNIT_SCALE);


			// and start setting up this thing!
			PreSetup(template, def);
			SetupMogre(template, def);

			InitialiseComponents(template, def);

			RootNode.Position = SpawnPosition;
			RootNode.Orientation = SpawnOrientation;
			// only scale up the root node if it doesn't have any physics things attached - bullet really does not like scaling.
			// Need a few variations of identical objects with different scales? Gonna have to make different .things for them.
			// Though it might be easier to just have one general .thing for them, and all it does is run a script that randomly
			// gets one of the others.
			if (ShapeComponents == null)
				RootNode.Scale(SpawnScale);
			RootNode.SetInitialState();

			PostInitialiseComponents(template, def);

			SetupPhysics(template, def);

			// get our script token and run it, if it has one and if this thing was created on the fly instead
			// of through a .muffin file
			string script;
			if (def.StringTokens.TryGetValue("script", out script)) {
				this.Script = script;
				RunScript();
			}

			DisposeIfStaticOrInstanced(def);
		}

		/// <summary>
		/// Use this method if you need some more stuff to happen before the constructor starts setting everything up.
		/// For example if you need to get more things out of the ThingTemplate, you can use this for that.
		/// </summary>
		protected virtual void PreSetup(ThingBlock template, ThingDefinition def) { }

		/// <summary>
		/// Sets up mogre stuff, like our root scene node
		/// </summary>
		private void SetupMogre(ThingBlock template, ThingDefinition def) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			// create our root node
			// need to check for map regions
			string mapRegion = template.GetStringProperty("MapRegion", string.Empty);
			if (string.IsNullOrEmpty(mapRegion)) {
				// no map region, continue on as normal
				this.RootNode = sceneMgr.RootSceneNode.CreateChildSceneNode(Name + ID);
			}
			else {
				string mapRegionNodeName = mapRegion + "Node";
				// there is a map region, make our root node a child of a node with the region's name
				// first check to see if that node exists already
				if (sceneMgr.HasSceneNode(mapRegionNodeName)) {
					// if it does, just attach our node to it
					this.RootNode = sceneMgr.GetSceneNode(mapRegionNodeName).CreateChildSceneNode(Name + ID);
				}
				else {
					// if it doesn't, create it first, then attach our node to it
					SceneNode newSceneNode = sceneMgr.RootSceneNode.CreateChildSceneNode(mapRegionNodeName);
					this.RootNode = newSceneNode.CreateChildSceneNode(Name + ID);
				}
			}
		}

		/// <summary>
		/// Make our components
		/// </summary>
		protected virtual void InitialiseComponents(ThingBlock template, ThingDefinition def) {
			// ogre stuff
			if (def.ModelBlocks.Count > 0) {
				ModelComponents = new List<ModelComponent>(def.ModelBlocks.Count);
				foreach (var mblock in def.ModelBlocks)
					ModelComponents.Add(new ModelComponent(this, template, mblock, def));
			}
			// bullet stuff
			if (def.ShapeBlocks.Count > 0) {
				ShapeComponents = new List<ShapeComponent>(def.ShapeBlocks.Count);
				foreach (var sblock in def.ShapeBlocks)
					ShapeComponents.Add(new ShapeComponent(this, sblock));
			}
			// ribbons
			if (def.RibbonBlocks.Count > 0) {
				RibbonComponents = new List<RibbonComponent>(def.RibbonBlocks.Count);
				foreach (var rblock in def.RibbonBlocks)
					RibbonComponents.Add(new RibbonComponent(this, template, rblock));
			}
			// billboard sets
			if (def.BillboardSetBlocks.Count > 0) {
				BillboardSetComponents = new List<BillboardSetComponent>(def.BillboardSetBlocks.Count);
				foreach (var bblock in def.BillboardSetBlocks)
					BillboardSetComponents.Add(new BillboardSetComponent(this, template, bblock));
			}
			// sounds
			if (def.SoundBlocks.Count > 0) {
				SoundComponents = new List<SoundComponent>(def.SoundBlocks.Count);
				foreach (var sblock in def.SoundBlocks)
					SoundComponents.Add(new SoundComponent(this, template, sblock));
			}
		}

		protected virtual void PostInitialiseComponents(ThingBlock template, ThingDefinition def) { }

		private void SetupPhysics(ThingBlock template, ThingDefinition def) {
			// if we have no shape components then we don't set up physics
			if (ShapeComponents == null)
				return;

			PreSetUpBodyInfo(def);
			SetUpBodyInfo(def);
			PostSetUpBodyInfo(def);
			CreateBody(def);
			PostCreateBody(def);
			SetBodyUserObject();
		}

		/// <summary>
		/// If you need anything before we set up the body info
		/// </summary>
		protected virtual void PreSetUpBodyInfo(ThingDefinition def) { }

		/// <summary>
		/// Set up all of the stuff needed before we create our body
		/// </summary>
		private void SetUpBodyInfo(ThingDefinition def) {
			// set up our collision shapes
			CollisionShape shape = LKernel.GetG<CollisionShapeManager>().CreateAndRegisterShape(this, def);

			// get the physics type and set up the mass of the body
			ThingEnum physicsType = def.GetEnumProperty("physics", null);
			float mass = physicsType.HasFlag(ThingEnum.Static) ? 0 : def.GetFloatProperty("mass", 1);

			// create our construction info thingy
			Vector3 inertia;
			shape.CalculateLocalInertia(mass, out inertia);

			// if it's static and doesn't have a sound, we don't need a mogre motion state because we'll be disposing of the root node afterwards
			if (def.GetBoolProperty("Static", false) && SoundComponents == null)
				MotionState = new DefaultMotionState();
			else
				MotionState = InitializationMotionState;

			Info = new RigidBodyConstructionInfo(mass, MotionState, shape, inertia);

			// physics material stuff from a .physmat file
			string physmat = def.GetStringProperty("PhysicsMaterial", "Default");
			LKernel.GetG<PhysicsMaterialFactory>().ApplyMaterial(Info, physmat);

			// we can override some of them in the .thing file
			if (def.FloatTokens.ContainsKey("bounciness"))
				Info.Restitution = def.GetFloatProperty("bounciness", PhysicsMaterial.DEFAULT_BOUNCINESS);
			if (def.FloatTokens.ContainsKey("friction"))
				Info.Friction = def.GetFloatProperty("friction", PhysicsMaterial.DEFAULT_FRICTION);
			if (def.FloatTokens.ContainsKey("angulardamping"))
				Info.AngularDamping = def.GetFloatProperty("angulardamping", PhysicsMaterial.DEFAULT_ANGULAR_DAMPING);
			if (def.FloatTokens.ContainsKey("lineardamping"))
				Info.LinearDamping = def.GetFloatProperty("lineardamping", PhysicsMaterial.DEFAULT_LINEAR_DAMPING);

			// choose which group to use for a default
			ThingEnum defaultGroup;
			if (physicsType.HasFlag(ThingEnum.Dynamic))
				defaultGroup = ThingEnum.Default;
			else if (physicsType.HasFlag(ThingEnum.Static))
				defaultGroup = ThingEnum.Environment;
			else // kinematic
				defaultGroup = ThingEnum.Default;

			// collision group
			ThingEnum collisionGroup = def.GetEnumProperty("CollisionGroup", defaultGroup);
			PonykartCollisionGroups pcg;
			if (!Enum.TryParse<PonykartCollisionGroups>(collisionGroup + String.Empty, true, out pcg))
				throw new FormatException("Invalid collision group!");
			CollisionGroup = pcg;

			// collides-with group
			ThingEnum collidesWith = def.GetEnumProperty("CollidesWith", defaultGroup);
			PonykartCollidesWithGroups pcwg;
			if (!Enum.TryParse<PonykartCollidesWithGroups>(collidesWith + String.Empty, true, out pcwg))
				throw new FormatException("Invalid collides-with group!");
			CollidesWith = pcwg;

			// update the transforms
			Matrix4 transform = new Matrix4();
			transform.MakeTransform(SpawnPosition, SpawnScale, SpawnOrientation);
			Info.StartWorldTransform = transform;
			MotionState.WorldTransform = transform;
		}

		/// <summary>
		/// Override this if you want to do more to the construction info before it's used to create the body but after it's been created
		/// </summary>
		protected virtual void PostSetUpBodyInfo(ThingDefinition def) { }

		/// <summary>
		/// Creates the body and makes it static/kinematic if specified.
		/// </summary>
		private void CreateBody(ThingDefinition def) {
			Body = new RigidBody(Info);

			// stick on our flags
			ThingEnum te = def.GetEnumProperty("physics", null);
			if (te.HasFlag(ThingEnum.Static))
				Body.CollisionFlags |= CollisionFlags.StaticObject;
			else if (te.HasFlag(ThingEnum.Kinematic))
				Body.CollisionFlags |= CollisionFlags.KinematicObject;

			if (def.GetBoolProperty("CollisionEvents", false))
				Body.CollisionFlags |= CollisionFlags.CustomMaterialCallback;

			if (def.GetBoolProperty("DisableVisualization", false))
				Body.CollisionFlags |= CollisionFlags.DisableVisualizeObject;

			Body.WorldTransform = Info.StartWorldTransform;

			LKernel.GetG<PhysicsMain>().World.AddRigidBody(Body, CollisionGroup, CollidesWith);

			if (def.GetBoolProperty("Deactivated", false))
				Body.ForceActivationState(ActivationState.WantsDeactivation);
		}

		/// <summary>
		/// Override this if you want to do more to the rigid body
		/// </summary>
		protected virtual void PostCreateBody(ThingDefinition td) { }

		/// <summary>
		/// Sets the body's UserObject
		/// </summary>
		private void SetBodyUserObject() {
			Body.UserObject = new CollisionObjectDataHolder(this);
		}

		/// <summary>
		/// Runs the thing's script, if it has one.
		/// If this thing was made from a .muffin, this is called from Level.RunLevelScripts to make sure it runs after everything else is created.
		/// If it was made on the fly, it runs at the end of the constructor, as long as it's a valid level of course.
		/// </summary>
		public void RunScript() {
			if (Script != null)
				LKernel.GetG<LuaMain>().DoFunctionForLThing(Script, this);
		}

		/// <summary>
		/// Makes all model components play the specified animation immediately, if they have it.
		/// </summary>
		public virtual void ChangeAnimation(string animationName) {
			if (ModelComponents != null) {
				foreach (var mcomp in ModelComponents) {
					if (mcomp.AnimationBlender != null && mcomp.Entity.AllAnimationStates.HasAnimationState(animationName)) {
						mcomp.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
						mcomp.AnimationBlender.AddTime((int) ID);
					}
				}
			}
		}

		/// <summary>
		/// Plays a random animation, if it has one.
		/// </summary>
		public virtual void RandomAnimation() {
			if (ModelComponents != null) {
				var anims = ModelComponents[0].GetAnimationNames();
				if (anims.Count() > 0) {
					Random rand = new Random(IDs.Random);
					string animName = "";
					do {
						int index = rand.Next(anims.Count());
						animName = anims.ElementAt(index);
						// don't want to play any "Basis" animations
					} while (!animName.Contains("Basis"));

					ChangeAnimation(animName);
				}
			}
		}

		private bool _soundsNeedUpdate = false;
		public bool SoundsNeedUpdate {
			get {
				return _soundsNeedUpdate;
			}
			set {
				// only change the sound components if this property is changing from false to true
				// they will change back to false by themselves
				// and we don't need to do the foreach loop if they're already true
				if (value && _soundsNeedUpdate != value) {
					foreach (var soundComponent in SoundComponents) {
						soundComponent.NeedUpdate = value;
					}
				}
				_soundsNeedUpdate = value;
			}
		}

		/// <summary>
		/// If this is a static/instanced thing with no ribbons, billboards, or sounds, we can clean up a whole bunch of stuff
		/// to make it faster for ogre.
		/// </summary>
		private void DisposeIfStaticOrInstanced(ThingDefinition def) {
			if (def.GetBoolProperty("Static", false) || def.GetBoolProperty("Instanced", false)) {
				if (IsDisposed)
					return;

				var sceneMgr = LKernel.GetG<SceneManager>();

				// this bool is to check we only fully dispose lthings if ALL of their model components are static/instanced
				bool removedAllModelComponents = true;
				// dispose of all of the model components
				if (ModelComponents != null) {
					foreach (ModelComponent mc in ModelComponents) {
						if (mc.Entity == null) {
							mc.Dispose();
						}
						else {
							removedAllModelComponents = false;
						}
					}
				}

				// if we have no ribbons, billboards, or sounds, we can get rid of the root node
				if (removedAllModelComponents && RibbonComponents == null && BillboardSetComponents == null && SoundComponents == null) {
					// if we have no shapes, we can get rid of everything
					if (ShapeComponents == null/*.Count == 0*/) {
						Dispose(true);
					}
					// but otherwise we can still get rid of the root scene node
					else {
						sceneMgr.DestroySceneNode(RootNode);
						RootNode.Dispose();
						RootNode = null;
					}
				}
			}
		}

		/// <summary>
		/// clean up
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			var sceneMgr = LKernel.GetG<SceneManager>();
			var world = LKernel.GetG<PhysicsMain>().World;
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (disposing) {
				// dispose all of our components
				if (ModelComponents != null) { 
					foreach (ModelComponent mc in ModelComponents)
						mc.Dispose();
					ModelComponents.Clear();
				}
				if (ShapeComponents != null) {
					foreach (ShapeComponent sc in ShapeComponents)
						sc.Dispose();
					ShapeComponents.Clear();
				}
				if (RibbonComponents != null) {
					foreach (RibbonComponent rc in RibbonComponents)
						rc.Dispose();
					RibbonComponents.Clear();
				}
				if (BillboardSetComponents != null) {
					foreach (BillboardSetComponent bb in BillboardSetComponents)
						bb.Dispose();
					BillboardSetComponents.Clear();
				}
				if (SoundComponents != null) {
					foreach (SoundComponent sb in SoundComponents)
						sb.Dispose();
					SoundComponents.Clear();
				}
			}

			// these are conditional in case we want to dispose stuff in the middle of a level
			if (RootNode != null) {
				if (valid && disposing)
					sceneMgr.DestroySceneNode(RootNode);
				RootNode.Dispose();
				RootNode = null;
			}
			if (Body != null) {
				if (valid && disposing)
					world.RemoveRigidBody(Body);
				Body.Dispose();
				Body = null;
			}

			base.Dispose(disposing);
		}

		public override int GetHashCode() {
			return (int) ID;
		}
	}
}
