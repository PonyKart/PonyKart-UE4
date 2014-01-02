
namespace SceneToMuffin {
	/// <summary>
	/// Represents data for each node from the .scene file, also put into a collection and represented on-screen in the table
	/// </summary>
	public class NodeData {
		public string Name { get; set; }
		public bool UsesThing { get; set; }
		public string ThingFile { get; set; }
		public float PosX { get; set; }
		public float PosY { get; set; }
		public float PosZ { get; set; }
		public float OrientX { get; set; }
		public float OrientY { get; set; }
		public float OrientZ { get; set; }
		public float OrientW { get; set; }
		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }
		public string Mesh { get; set; }
		public string Material { get; set; }
		public bool Static { get; set; }
		public bool CastShadows { get; set; }
		public bool ReceiveShadows { get; set; }
	}
}
