using Mogre;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents a physics collision shape
	/// </summary>
	public class ShapeComponent : LDisposable {
		public Matrix4 Transform { get; set; }

		public ThingEnum Type { get; private set; }
		public Vector3 Dimensions { get; private set; }
		public float Radius { get; private set; }
		public float Height { get; private set; }
		public string Mesh { get; private set; }

		public float MinHeight { get; private set; }
		public float MaxHeight { get; private set; }
		public int Width { get; private set; }
		public int Length { get; private set; }

		/// <summary>
		/// For physics
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="block">The block we're creating this component from</param>
		public ShapeComponent(LThing lthing, ShapeBlock block) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Transform = block.Transform;

			Type = block.GetEnumProperty("type", null);
			switch (Type) {
				case ThingEnum.Box:
				case ThingEnum.Cylinder:
					Dimensions = block.GetVectorProperty("dimensions", null) / 2f;
					break;
				case ThingEnum.Capsule:
				case ThingEnum.Cone:
					Height = block.GetFloatProperty("height", null);
					Radius = block.GetFloatProperty("radius", null);
					break;
				case ThingEnum.Sphere:
					Radius = block.GetFloatProperty("radius", null);
					break;
				case ThingEnum.Heightmap:
					MinHeight = block.GetFloatProperty("MinHeight", null);
					MaxHeight = block.GetFloatProperty("MaxHeight", null);
					Width = (int) block.GetFloatProperty("Width", null);
					Length = (int) block.GetFloatProperty("Length", null);
					Mesh = block.GetStringProperty("mesh", null);
					Dimensions = block.GetVectorProperty("dimensions", null) / 2f;
					break;
				case ThingEnum.Hull:
				case ThingEnum.Mesh:
					Mesh = block.GetStringProperty("mesh", null);
					break;
			}
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			base.Dispose(disposing);
		}
	}
}