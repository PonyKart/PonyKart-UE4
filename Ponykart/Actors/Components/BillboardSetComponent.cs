using Mogre;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents a billboard set, aka a collection of billboards that share the same material, direction, etc.
	/// </summary>
	public class BillboardSetComponent : LDisposable {
		public uint ID { get; protected set; }
		public string Name { get; protected set; }
		public BillboardSet BillboardSet { get; protected set; }
		private SceneNode LocalNode { get; set; }

		/// <summary>
		/// Woo billboards! These are pretty dissimilar from ogre entities - they only have a size and material, and no mesh or anything
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public BillboardSetComponent(LThing lthing, ThingBlock template, BillboardSetBlock block) {
			ID = IDs.Incremental;
			Name = block.GetStringProperty("name", template.ThingName);

			var sceneMgr = LKernel.GetG<SceneManager>();

			// set it up
			BillboardSet = sceneMgr.CreateBillboardSet(Name + ID + "BillboardSet", (uint) block.BillboardBlocks.Count);
			BillboardSet.SetMaterialName(block.GetStringProperty("material", null));
			BillboardSet.CastShadows = false; //block.GetBoolProperty("CastsShadows", false);
			BillboardSet.SetDefaultDimensions(block.GetFloatProperty("width", 1), block.GetFloatProperty("height", 1));

			// billboard type
			ThingEnum type = block.GetEnumProperty("Type", ThingEnum.Point);
			switch (type) {
				case ThingEnum.Point:
					BillboardSet.BillboardType = BillboardType.BBT_POINT; break;
				case ThingEnum.OrientedCommon:
					BillboardSet.BillboardType = BillboardType.BBT_ORIENTED_COMMON; break;
				case ThingEnum.OrientedSelf:
					BillboardSet.BillboardType = BillboardType.BBT_ORIENTED_SELF; break;
				case ThingEnum.PerpendicularCommon:
					BillboardSet.BillboardType = BillboardType.BBT_PERPENDICULAR_COMMON; break;
				case ThingEnum.PerpendicularSelf:
					BillboardSet.BillboardType = BillboardType.BBT_PERPENDICULAR_SELF; break;
			}

			Vector3 vec;
			if (block.VectorTokens.TryGetValue("upvector", out vec))
				BillboardSet.CommonUpVector = vec;
			// sort transparent stuff
			BillboardSet.SortingEnabled = block.GetBoolProperty("Sort", true);
			// make them point the right way
			BillboardSet.CommonDirection = block.GetVectorProperty("Direction", Vector3.UNIT_Y);
			// rotation type
			BillboardSet.BillboardRotationType = block.GetBoolProperty("UseVertexRotation", false) ? BillboardRotationType.BBR_VERTEX : BillboardRotationType.BBR_TEXCOORD;
			// origin
			ThingEnum originToken = block.GetEnumProperty("Origin", ThingEnum.Center);
			switch (originToken) {
				case ThingEnum.TopLeft:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_TOP_LEFT; break;
				case ThingEnum.TopCenter:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_TOP_CENTER; break;
				case ThingEnum.TopRight:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_TOP_RIGHT; break;
				case ThingEnum.CenterLeft:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_CENTER_LEFT; break;
				case ThingEnum.Center:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_CENTER; break;
				case ThingEnum.CenterRight:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_CENTER_RIGHT; break;
				case ThingEnum.BottomLeft:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_BOTTOM_LEFT; break;
				case ThingEnum.BottomCenter:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_BOTTOM_CENTER; break;
				case ThingEnum.BottomRight:
					BillboardSet.BillboardOrigin = BillboardOrigin.BBO_BOTTOM_RIGHT; break;
			}

			BillboardSet.RenderingDistance = block.GetFloatProperty("RenderingDistance", 120);

			// texture coordinates
			Quaternion rectQ;
			if (block.QuatTokens.TryGetValue("texturecoords", out rectQ)) {
				unsafe {
					var rect = new FloatRect(rectQ.x, rectQ.y, rectQ.z, rectQ.w);
					BillboardSet.SetTextureCoords(&rect, 1);
				}
			}

			// stacks/slices
			if (block.FloatTokens.ContainsKey("texturestacks") && block.FloatTokens.ContainsKey("textureslices")) {
				BillboardSet.SetTextureStacksAndSlices((byte) block.GetFloatProperty("TextureStacks", 1), (byte) block.GetFloatProperty("TextureSlices", 1));
			}

			// and then go through each billboard block and create a billboard from it
			foreach (BillboardBlock bbblock in block.BillboardBlocks)
				CreateBillboard(bbblock);


			// setup attachment, if it needs one
			if (block.GetBoolProperty("Attached", false)) {
				string boneName = block.GetStringProperty("AttachBone", null);
				int modelComponentID = (int) block.GetFloatProperty("AttachComponentID", null);
				Quaternion offsetQuat = block.GetQuatProperty("AttachOffsetOrientation", Quaternion.IDENTITY);
				Vector3 offsetVec = block.GetVectorProperty("AttachOffsetPosition", Vector3.ZERO);

				lthing.ModelComponents[modelComponentID].Entity.AttachObjectToBone(boneName, BillboardSet, offsetQuat, offsetVec);
			}
			// if not, just attach it to the root node
			else {
				Vector3 pos = block.GetVectorProperty("Position", Vector3.ZERO);

				SceneNode attachNode;
				if (pos != Vector3.ZERO) {
					LocalNode = lthing.RootNode.CreateChildSceneNode(pos);
					attachNode = LocalNode;
				}
				else {
					attachNode = lthing.RootNode;
				}

				attachNode.AttachObject(BillboardSet);
			}
		}

		/// <summary>
		/// Make one billboard from each billboard block.
		/// </summary>
		void CreateBillboard(BillboardBlock block) {
			// make our billboard
			Billboard bb = BillboardSet.CreateBillboard(block.GetVectorProperty("Position", null));
			// set its color if it has one
			Quaternion quat;
			if (block.QuatTokens.TryGetValue("colour", out quat))
				bb.Colour = quat.ToColourValue();
			// and a rotation
			bb.Rotation = new Degree(block.GetFloatProperty("Rotation", 0));

			Quaternion rectQ;
			if (block.QuatTokens.TryGetValue("texturecoords", out rectQ)) {
				bb.SetTexcoordRect(rectQ.x, rectQ.y, rectQ.z, rectQ.w);
			}

			// it's best to not do this unless we really need to since it makes it less efficient
			float height, width;
			if (block.FloatTokens.TryGetValue("width", out width) && block.FloatTokens.TryGetValue("height", out height))
				bb.SetDimensions(width, height);
		}

		public override string ToString() {
			return Name + ID + "BillboardSet";
		}

		/// <summary>
		/// clean up our billboard set
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			var sceneMgr = LKernel.GetG<SceneManager>();
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (BillboardSet != null) {
				if (valid && disposing)
					sceneMgr.DestroyBillboardSet(BillboardSet);
				BillboardSet.Dispose();
				BillboardSet = null;
			}

			if (LocalNode != null) {
				if (valid && disposing)
					sceneMgr.DestroySceneNode(LocalNode);
				LocalNode.Dispose();
				LocalNode = null;
			}

			base.Dispose(disposing);
		}
	}
}
