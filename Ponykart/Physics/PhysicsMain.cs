using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.Stuff;

namespace Ponykart.Physics {
	public delegate void PhysicsWorldEvent(DiscreteDynamicsWorld world);
	public delegate void PhysicsSimulateEvent(DiscreteDynamicsWorld world, FrameEvent evt);

	public class PhysicsMain : LDisposable {
		private BroadphaseInterface broadphase;
		private DefaultCollisionConfiguration dcc;
		private CollisionDispatcher dispatcher;
		private SequentialImpulseConstraintSolver solver;

		private DiscreteDynamicsWorld world;

		/// <summary>
		/// Is invoked right after the physics world is created.
		/// </summary>
		public static event PhysicsWorldEvent PostCreateWorld;
		/// <summary>
		/// Is invoked right before the physics world is simulated.
		/// </summary>
		public static event PhysicsSimulateEvent PreSimulate;
		/// <summary>
		/// Is invoked right after the physics world is simulated.
		/// </summary>
		public static event PhysicsSimulateEvent PostSimulate;
		/// <summary>
		/// Is invoked just after PreSimulate but just before the physics world is simulated.
		/// You should use this as a last "get everything ready" point before we simulate.
		/// </summary>
		public static event PhysicsSimulateEvent FinaliseBeforeSimulation;

		public static event ContactAdded ContactAdded;

		/// <summary>
		/// Should we draw debug lines or not?
		/// </summary>
		public static bool DrawLines = false;

		public static bool SlowMo = false;

		/// <summary>
		/// Constructor
		/// </summary>
		public PhysicsMain() {
			Launch.Log("[Loading] Creating PhysicsMain...");

			LevelManager.OnLevelUnload += OnLevelUnload;
		}

		/// <summary>
		/// Disposes the world
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			LKernel.GetG<Root>().FrameEnded -= FrameEnded;

			lock (world) {
				if (!world.IsDisposed) {
					for (int a = 0; a < world.CollisionObjectArray.Count; a++) {
						var obj = world.CollisionObjectArray[a];

						if (obj != null && !obj.IsDisposed) {
							world.RemoveCollisionObject(obj);
							obj.Dispose();
						}
					}

					broadphase.Dispose();
					solver.Dispose();
					dcc.Dispose();
					dispatcher.Dispose();

					world.Dispose();
				}
			}
		}


		const int _maxSubsteps = 60; //45;
        // The fixed timestep is not in sync with the rest. Higher values make it somewhat smoother. 
        //1/300 is playable with the occasional blink half a meter backward
		const float _fixedTimestep = 1f / 80f;
		/// <summary>
		/// Runs just before every frame. Simulates one frame of physics.
		/// Physics simulation should be the only thing that's using FrameEnded!
		/// </summary>
		bool FrameEnded(FrameEvent evt) {
			if (Pauser.IsPaused || world.IsDisposed)
				return true;

			// run the events that go just before we simulate
			if (PreSimulate != null)
				PreSimulate(world, evt);

			if (FinaliseBeforeSimulation != null)
				FinaliseBeforeSimulation(world, evt);

			world.StepSimulation(SlowMo ? evt.timeSinceLastFrame / 10f : evt.timeSinceLastFrame, _maxSubsteps, _fixedTimestep);

			// run the events that go just after we simulate
			if (PostSimulate != null)
				PostSimulate(world, evt);

			if (DrawLines)
				world.DebugDrawWorld();

			/*foreach (var item in LKernel.GetG<LevelManager>().CurrentLevel.Things) {
				if (item.Value.Name.Contains("Apple") && !item.Value.Name.Contains("Tree")) {
					System.Console.WriteLine(item.Value.Body.IsActive);
				}
			}*/

			return true;
		}

		/// <summary>
		/// Creates the world
		/// </summary>
		void CreateWorld(string levelName) {
			Launch.Log("[PhysicsMain] Creating new world...");
			// have to make more of these every level because disposing the world apparently disposes of them too.
			broadphase = new DbvtBroadphase();
			solver = new SequentialImpulseConstraintSolver();
			dcc = new DefaultCollisionConfiguration();
			dispatcher = new CollisionDispatcher(dcc);
			// set up this stuff... not quite sure what it's for, but you need it if you want the CCD to work for the karts
			dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.ConvexHullShape, BroadphaseNativeType.ConvexHullShape,
				dcc.GetCollisionAlgorithmCreateFunc(BroadphaseNativeType.TriangleMeshShape, BroadphaseNativeType.TriangleMeshShape));
			dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.ConvexHullShape, BroadphaseNativeType.ConvexHullShape,
				dcc.GetCollisionAlgorithmCreateFunc(BroadphaseNativeType.ConvexHullShape, BroadphaseNativeType.ConvexHullShape));

			world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, dcc);
			// and then turn on CCD
			world.DispatchInfo.UseContinuous = true;

			world.Gravity = new Vector3(0, Settings.Default.Gravity, 0);

			ManifoldPoint.ContactAddedCallback = ContactAdded;
		}

		/// <summary>
		/// Sets up a new physics scene.
		/// 
		/// This is called manually from LevelManager at the moment because the Scene needs to be set up before anything else is put in it.
		/// </summary>
		public void LoadPhysicsLevel(string levelName) {
			Launch.Log("[Loading] Setting up Physics world and loading shapes from .scene file");

			CreateWorld(levelName);

			// creates collision meshes out of static objects
			// get the scene manager
			SceneManager sceneMgr = LKernel.GetG<SceneManager>();
			// parse our .scene file
			
			if (System.IO.File.Exists(Settings.Default.WorldFileLocation + levelName + ".scene")) {
				DotSceneLoader dsl = new DotSceneLoader();
				dsl.ParseDotScene(levelName + ".scene", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, sceneMgr.RootSceneNode);

				// then go through each of the static objects and turn them into trimeshes.
				foreach (string s in dsl.StaticObjects) {
					// apparently triangle meshes only screw up if you turn on debug drawing for them. No I don't know why the fuck that should matter.
					Entity dslEnt = sceneMgr.GetEntity(s);
					SceneNode dslNode = sceneMgr.GetSceneNode(s);

					CollisionShape shape;

					string bulletFilePath = dslNode.Name + ".bullet";

					shape = LKernel.GetG<CollisionShapeManager>().GetShapeFromFile(bulletFilePath, dslEnt, dslNode);

					// then do the rest as usual
					var info = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape, Vector3.ZERO);
					var body = new RigidBody(info);
					body.CollisionFlags = CollisionFlags.StaticObject | CollisionFlags.DisableVisualizeObject;
					body.UserObject = new CollisionObjectDataHolder(body, PonykartCollisionGroups.Road, dslNode.Name);
					world.AddRigidBody(body, PonykartCollisionGroups.Road, PonykartCollidesWithGroups.Road);
				}
			}

			// make a ground plane for us
			CreateGroundPlane(-15);

			// run some events
			if (PostCreateWorld != null)
				PostCreateWorld(world);
		}

		/// <summary>
		/// To be called from the LevelManager
		/// </summary>
		public void StartSimulation() {
			LKernel.GetG<Root>().FrameEnded += FrameEnded;
		}

		/// <summary>
		/// Create a static ground plane facing upwards.
		/// </summary>
		/// <param name="yposition">The Y position that the plane is located at.</param>
		void CreateGroundPlane(float yposition) {
			// make an infinite plane so we don't fall forever. TODO: hook up an event so when we collide with this, we respawn back on the track
			Matrix4 matrix = new Matrix4();
			matrix.MakeTransform(new Vector3(0, yposition, 0), Vector3.UNIT_SCALE, new Quaternion(0, 0, 0, 1));

			CollisionShape groundShape = new StaticPlaneShape(Vector3.NEGATIVE_UNIT_Y, 1);
			var groundInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(matrix), groundShape, Vector3.ZERO);
			var groundBody = new RigidBody(groundInfo);
			groundBody.UserObject = new CollisionObjectDataHolder(groundBody, PonykartCollisionGroups.Environment, "ground");
			groundBody.CollisionFlags = CollisionFlags.StaticObject | CollisionFlags.DisableVisualizeObject;
			world.AddRigidBody(groundBody, PonykartCollisionGroups.Environment, PonykartCollidesWithGroups.Environment);
		}

		public DiscreteDynamicsWorld World {
			get { return world; }
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			OnLevelUnload(default(LevelChangedEventArgs));

			base.Dispose(disposing);
		}
	}
}
