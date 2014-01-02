#if DEBUG
using BulletSharp;
using Mogre;
using Ponykart.Levels;
using Ponykart.Players;

namespace Ponykart.Physics {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class BulletDebugDrawer : IDebugDraw, ILevelHandler {
		SceneManager sceneMgr;
		ManualObject lines;
		ManualObject triangles;
		public DebugDrawModes DebugMode { get; set; }
		readonly float maxRenderDistanceSquared = 20 * 20;
		LevelManager levelMgr;
		SceneNode mainPlayerKartNode;

		bool begin = false;

		public BulletDebugDrawer() {
			sceneMgr = LKernel.GetG<SceneManager>();

			lines = new ManualObject("physics lines");
			triangles = new ManualObject("physics triangles");
			lines.Dynamic = true;
			triangles.Dynamic = true;

			sceneMgr.RootSceneNode.AttachObject(lines);
			sceneMgr.RootSceneNode.AttachObject(triangles);

			string matName = "BulletDebugDrawerMaterial";
			MaterialPtr mtl = MaterialManager.Singleton.GetDefaultSettings().Clone(matName);
			mtl.ReceiveShadows = false;
			mtl.SetSceneBlending(SceneBlendType.SBT_TRANSPARENT_ALPHA);
			mtl.SetDepthBias(0.1f, 0);

			TextureUnitState tu = mtl.GetTechnique(0).GetPass(0).CreateTextureUnitState();
			tu.SetColourOperationEx(LayerBlendOperationEx.LBX_SOURCE1, LayerBlendSource.LBS_DIFFUSE);
			mtl.GetTechnique(0).SetLightingEnabled(false);

			lines.Begin(matName, RenderOperation.OperationTypes.OT_LINE_LIST);
			begin = true;
			lines.Position(Vector3.ZERO);
			lines.Colour(ColourValue.Blue);
			lines.Position(Vector3.ZERO);
			lines.Colour(ColourValue.Blue);
			lines.End();

			triangles.Begin(matName, RenderOperation.OperationTypes.OT_TRIANGLE_LIST);
			triangles.Position(Vector3.ZERO);
			triangles.Colour(ColourValue.Blue);
			triangles.Position(Vector3.ZERO);
			triangles.Colour(ColourValue.Blue);
			triangles.Position(Vector3.ZERO);
			triangles.Colour(ColourValue.Blue);
			triangles.End();
			begin = false;

			DebugMode = DebugDrawModes.DrawWireframe | /*DebugDrawModes.DrawAabb |*/ DebugDrawModes.FastWireframe
				| DebugDrawModes.EnableCCD | DebugDrawModes.DrawNormals;

			levelMgr = LKernel.GetG<LevelManager>();
			mainPlayerKartNode = LKernel.GetG<PlayerManager>().MainPlayer.Kart.RootNode;

			PhysicsMain.PreSimulate += PreSimulate;
			PhysicsMain.PostSimulate += PostSimulate;

			LKernel.GetG<PhysicsMain>().World.DebugDrawer = this;
		}

		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!begin) {
				lines.BeginUpdate(0);
				triangles.BeginUpdate(0);
				begin = true;
			}
		}

		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (begin) {
				lines.End();
				triangles.End();
				begin = false;
			}
		}

		public void Detach() {
			PhysicsMain.PreSimulate -= PreSimulate;
			PhysicsMain.PostSimulate -= PostSimulate;
		}

		/// <summary>
		/// A little condition to check whether we should render a line or not
		/// </summary>
		bool DrawCondition(Vector3 compare) {
			return !PhysicsMain.DrawLines || !levelMgr.IsValidLevel
				|| (mainPlayerKartNode.Position - compare).SquaredLength > maxRenderDistanceSquared;
		}

		/// <summary>
		/// A little condition to check whether we should render a line or not
		/// </summary>
		bool DrawCondition(Matrix4 compare) {
			return DrawCondition(compare.GetTrans());
		}


		/// <summary>
		/// How many "steps" when we draw circles
		/// </summary>
		private const int numIter = 12;
		/// <summary>
		/// radian amount to increase the angle by when drawing circles
		/// </summary>
		private const float increaseAmount = (360f / numIter) * (Math.TWO_PI / 360f);
		/// <summary>
		/// maximum angle to draw circles with
		/// </summary>
		private const float limit = Math.TWO_PI + increaseAmount;





		public void Draw3dText(Vector3 location, string textString) { }

		/// <summary>
		/// Draws an axis-aligned bounding box
		/// </summary>
		/// <param name="colour">
		/// I override this and make it white with 30% opacity
		/// </param>
		public void DrawAabb(Vector3 from, Vector3 to, ColourValue colour) {
			if (!begin || DrawCondition(from))
				return;

			colour = new ColourValue(1f, 1f, 1f, 0.3f);

			// I'm sure there's a better way of doing this
			Vector3 loo = new Vector3(to.x, from.y, from.z);
			Vector3 olo = new Vector3(from.x, to.y, from.z);
			Vector3 ool = new Vector3(from.x, from.y, to.z);
			Vector3 llo = new Vector3(to.x, to.y, from.z);
			Vector3 lol = new Vector3(to.x, from.y, to.z);
			Vector3 oll = new Vector3(from.x, to.y, to.z);

			lines.Position(from); lines.Colour(colour);
			lines.Position(loo); lines.Colour(colour);
			lines.Position(from); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(from); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);

			lines.Position(to); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(to); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(to); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);

			lines.Position(loo); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(loo); lines.Colour(colour);
		}

		public void DrawArc(Vector3 center, Vector3 normal, Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ColourValue colour, bool drawSect, float stepDegrees) {
		
		}

		public void DrawArc(Vector3 center, Vector3 normal, Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ColourValue colour, bool drawSect) {
		
		}

		public void DrawBox(Vector3 bbMin, Vector3 bbMax, Matrix4 trans, ColourValue colour) {
			Vector3 origin = trans.GetTrans();
			if (DrawCondition(origin))
				return;

			Vector3 loo = trans * new Vector3(bbMin.x, bbMax.y, bbMax.z);
			Vector3 olo = trans * new Vector3(bbMax.x, bbMin.y, bbMax.z);
			Vector3 ool = trans * new Vector3(bbMax.x, bbMax.y, bbMin.z);
			Vector3 llo = trans * new Vector3(bbMin.x, bbMin.y, bbMax.z);
			Vector3 lol = trans * new Vector3(bbMin.x, bbMax.y, bbMin.z);
			Vector3 oll = trans * new Vector3(bbMax.x, bbMin.y, bbMin.z);

			bbMin = trans * bbMin;
			bbMax = trans * bbMax;

			lines.Position(bbMax); lines.Colour(colour);
			lines.Position(loo); lines.Colour(colour);
			lines.Position(bbMax); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(bbMax); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);

			lines.Position(bbMin); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(bbMin); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(bbMin); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);

			lines.Position(loo); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(loo); lines.Colour(colour);
		}

		public void DrawBox(Vector3 bbMin, Vector3 bbMax, ColourValue colour) {
			if (DrawCondition(bbMin))
				return;

			Vector3 loo = new Vector3(bbMin.x, bbMax.y, bbMax.z);
			Vector3 olo = new Vector3(bbMax.x, bbMin.y, bbMax.z);
			Vector3 ool = new Vector3(bbMax.x, bbMax.y, bbMin.z);
			Vector3 llo = new Vector3(bbMin.x, bbMin.y, bbMax.z);
			Vector3 lol = new Vector3(bbMin.x, bbMax.y, bbMin.z);
			Vector3 oll = new Vector3(bbMax.x, bbMin.y, bbMin.z);

			lines.Position(bbMax); lines.Colour(colour);
			lines.Position(loo); lines.Colour(colour);
			lines.Position(bbMax); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(bbMax); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);

			lines.Position(bbMin); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(bbMin); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(bbMin); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);

			lines.Position(loo); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(lol); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);
			lines.Position(ool); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);
			lines.Position(oll); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(olo); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(llo); lines.Colour(colour);
			lines.Position(loo); lines.Colour(colour);
		}

		/// <summary>
		/// Draws a cylinder, then draws two half-spheres on the top and bottom
		/// </summary>
		public void DrawCapsule(float radius, float halfHeight, int upAxis, Matrix4 transform, ColourValue colour) {
			if (DrawCondition(transform))
				return;

			DrawCylinder(radius, halfHeight, upAxis, transform, colour);

			Vector3 previousXYPos = transform * new Vector3(radius, halfHeight, 0);
			Vector3 previousYZPos = transform * new Vector3(0, halfHeight, radius);
			Vector3 previousNXYPos = transform * new Vector3(radius, -halfHeight, 0);
			Vector3 previousNYZPos = transform * new Vector3(0, -halfHeight, radius);

			float capsuleLimit = limit * 0.5f;

			// y-x circle
			for (float a = 0; a <= capsuleLimit; a += increaseAmount) {
				float y = Math.Sin(a) * radius;
				float x = Math.Cos(a) * radius;

				// xy
				Vector3 xyPos = transform * new Vector3(x, y + halfHeight, 0);
				lines.Position(previousXYPos);
				lines.Colour(colour);
				lines.Position(xyPos);
				lines.Colour(colour);
				previousXYPos = xyPos;

				// yz
				float z = Math.Cos(a) * radius;

				Vector3 yzPos = transform * new Vector3(0, y + halfHeight, z);
				lines.Position(previousYZPos);
				lines.Colour(colour);
				lines.Position(yzPos);
				lines.Colour(colour);
				previousYZPos = yzPos;

				// -xy
				Vector3 nxyPos = transform * new Vector3(x, -y - halfHeight, 0);
				lines.Position(previousNXYPos);
				lines.Colour(colour);
				lines.Position(nxyPos);
				lines.Colour(colour);
				previousNXYPos = nxyPos;

				// -yz
				Vector3 nyzPos = transform * new Vector3(0, -y - halfHeight, z);
				lines.Position(previousNYZPos);
				lines.Colour(colour);
				lines.Position(nyzPos);
				lines.Colour(colour);
				previousNYZPos = nyzPos;
			}
		}

		/// <summary>
		/// Draws a cone
		/// </summary>
		/// <param name="upAxis">
		/// I have no idea what this is for
		/// </param>
		public void DrawCone(float radius, float height, int upAxis, Matrix4 transform, ColourValue colour) {
			if (DrawCondition(transform))
				return;

			float halfHeight = height / 2f;
			Vector3 previousPos = transform * new Vector3(0, -halfHeight, radius);
			Vector3 tip = transform * new Vector3(0, halfHeight, 0);

			for (float a = 0; a <= limit; a += increaseAmount) {
				float z = Math.Cos(a) * radius;
				float x = Math.Sin(a) * radius;

				// the circle
				Vector3 pos = transform * new Vector3(x, -halfHeight, z);
				lines.Position(previousPos);
				lines.Colour(colour);
				lines.Position(pos);
				lines.Colour(colour);
				previousPos = pos;

				// the sides
				lines.Position(pos);
				lines.Colour(colour);
				lines.Position(tip);
				lines.Colour(colour);
			}
		}

		/// <summary>
		/// This doesn't seem to even work half the time
		/// </summary>
		public void DrawContactPoint(Vector3 pointOnB, Vector3 normalOnB, float distance, int lifeTime, ColourValue colour) {
			//if (DrawCondition(pointOnB))
			//	return;

			lines.Position(pointOnB);
			lines.Colour(colour);
			lines.Position(pointOnB + normalOnB * distance);
			lines.Colour(colour);
		}

		/// <summary>
		/// Draws a cylinder
		/// </summary>
		/// <param name="upAxis">
		/// no idea what this is even for
		/// </param>
		public void DrawCylinder(float radius, float halfHeight, int upAxis, Matrix4 transform, ColourValue colour) {
			if (DrawCondition(transform))
				return;

			Vector3 previousPos = transform * new Vector3(0, halfHeight, radius);
			Vector3 previousNPos = transform * new Vector3(0, -halfHeight, radius);

			for (float a = 0; a <= limit; a += increaseAmount) {
				float z = Math.Cos(a) * radius;
				float x = Math.Sin(a) * radius;

				// positive
				Vector3 pos = transform * new Vector3(x, halfHeight, z);
				lines.Position(previousPos);
				lines.Colour(colour);
				lines.Position(pos);
				lines.Colour(colour);
				previousPos = pos;

				// negative
				Vector3 npos = transform * new Vector3(x, -halfHeight, z);
				lines.Position(previousNPos);
				lines.Colour(colour);
				lines.Position(npos);
				lines.Colour(colour);
				previousNPos = npos;

				// the sides
				lines.Position(pos);
				lines.Colour(colour);
				lines.Position(npos);
				lines.Colour(colour);
			}
		}

		/// <summary>
		/// Draws a line
		/// </summary>
		public void DrawLine(Vector3 from, Vector3 to, ColourValue colour) {
			if (DrawCondition(from))
				return;

			lines.Position(from);
			lines.Colour(colour);
			lines.Position(to);
			lines.Colour(colour);
		}

		/// <summary>
		/// Draws a line
		/// </summary>
		public void DrawLine(Vector3 from, Vector3 to, ColourValue fromcolour, ColourValue tocolour) {
			if (DrawCondition(from))
				return;

			lines.Position(from);
			lines.Colour(fromcolour);
			lines.Position(to);
			lines.Colour(tocolour);
		}

		public void DrawPlane(Vector3 planeNormal, float planeConst, Matrix4 transform, ColourValue colour) {
			
		}

		/// <summary>
		/// Draws a sphere that doesn't rotate
		/// </summary>
		public void DrawSphere(Vector3 p, float radius, ColourValue colour) {
			if (DrawCondition(p))
				return;

			Vector3 previousXYPos = p + new Vector3(0, radius, 0);
			Vector3 previousYZPos = p + new Vector3(0, radius, 0);
			Vector3 previousXZPos = p + new Vector3(0, 0, radius);

			for (float a = 0; a <= limit; a += increaseAmount) {
				float y = Math.Cos(a) * radius;
				float x = Math.Sin(a) * radius;

				// xy
				Vector3 xyPos = p + new Vector3(x, y, 0);
				lines.Position(previousXYPos);
				lines.Colour(colour);
				lines.Position(xyPos);
				lines.Colour(colour);
				previousXYPos = xyPos;

				// yz
				float z = Math.Sin(a) * radius;

				Vector3 yzPos = p + new Vector3(0, y, z);
				lines.Position(previousYZPos);
				lines.Colour(colour);
				lines.Position(yzPos);
				lines.Colour(colour);
				previousYZPos = yzPos;

				// xz
				z = Math.Cos(a) * radius;

				Vector3 xzPos = p + new Vector3(x, 0, z);
				lines.Position(previousXZPos);
				lines.Colour(colour);
				lines.Position(xzPos);
				lines.Colour(colour);
				previousXZPos = xzPos;
			}

		}

		/// <summary>
		/// Draws a sphere that does rotate
		/// </summary>
		public void DrawSphere(float radius, Matrix4 transform, ColourValue colour) {
			if (DrawCondition(transform))
				return;

			Vector3 previousXYPos = transform * new Vector3(0, radius, 0);
			Vector3 previousYZPos = transform * new Vector3(0, radius, 0);
			Vector3 previousXZPos = transform * new Vector3(0, 0, radius);

			for (float a = 0; a <= limit; a += increaseAmount) {
				float y = Math.Cos(a) * radius;
				float x = Math.Sin(a) * radius;

				// xy
				Vector3 xyPos = transform * new Vector3(x, y, 0);
				lines.Position(previousXYPos);
				lines.Colour(colour);
				lines.Position(xyPos);
				lines.Colour(colour);
				previousXYPos = xyPos;

				// yz
				float z = Math.Sin(a) * radius;

				Vector3 yzPos = transform * new Vector3(0, y, z);
				lines.Position(previousYZPos);
				lines.Colour(colour);
				lines.Position(yzPos);
				lines.Colour(colour);
				previousYZPos = yzPos;

				// xz
				z = Math.Cos(a) * radius;

				Vector3 xzPos = transform * new Vector3(x, 0, z);
				lines.Position(previousXZPos);
				lines.Colour(colour);
				lines.Position(xzPos);
				lines.Colour(colour);
				previousXZPos = xzPos;
			}
		}

		public void DrawSpherePatch(Vector3 center, Vector3 up, Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour, float stepDegrees) {
			
		}

		public void DrawSpherePatch(Vector3 center, Vector3 up, Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour) {
			
		}

		public void DrawTransform(Matrix4 transform, float orthoLen) {
			Vector3 origin = transform.GetTrans();

			if (DrawCondition(origin))
				return;
			
			Quaternion orient = transform.ExtractQuaternion();

			lines.Position(origin);
			lines.Colour(ColourValue.Red);
			lines.Position(origin + (orient.XAxis * orthoLen));
			lines.Colour(ColourValue.Red);

			lines.Position(origin);
			lines.Colour(ColourValue.Green);
			lines.Position(origin + (orient.YAxis * orthoLen));
			lines.Colour(ColourValue.Green);

			lines.Position(origin);
			lines.Colour(ColourValue.Blue);
			lines.Position(origin + (orient.ZAxis * orthoLen));
			lines.Colour(ColourValue.Blue);
		}

		/// <param name="__unnamed004">alpha?</param>
		public void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, ColourValue colour, float __unnamed004) {
			if (DrawCondition(v0)) 
				return;

			triangles.Position(v0);
			triangles.Colour(colour);
			triangles.Position(v1);
			triangles.Colour(colour);
			triangles.Position(v2);
			triangles.Colour(colour);
		}

		/// <param name="__unnamed003">no idea</param>
		/// <param name="__unnamed004">no idea</param>
		/// <param name="__unnamed005">no idea</param>
		public void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 __unnamed003, Vector3 __unnamed004, Vector3 __unnamed005, ColourValue colour, float alpha) {
			if (DrawCondition(v0)) 
				return;

			DrawTriangle(v0, v1, v2, colour, alpha);
		}

		public void ReportErrorWarning(string warningString) {
			Launch.Log("[WARNING] (BulletDebugManager): " + warningString);
		}
	}
}
#endif