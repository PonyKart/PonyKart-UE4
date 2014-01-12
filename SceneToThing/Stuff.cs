
namespace SceneToThing {
	/// <summary>
	/// just so we can cast Nodes and Shapes to the same type for the listbox
	/// </summary>
	public abstract class Block {
		public Vector3 Position { get; set; }
		public Quaternion Orientation { get; set; }
		/// <summary>
		/// Scale, for nodes
		/// </summary>
		public Vector3 Dimensions { get; set; }
	}

	/// <summary>
	/// Representation of Mogre.SceneNode
	/// </summary>
	public class Node : Block {
		public string Name { get; set; }
		public Entity Entity { get; set; }

		public override string ToString() {
			return Name + " (" + Entity.Mesh + ")";
		}
	}

	/// <summary>
	/// Representation of Mogre.Entity
	/// </summary>
	public class Entity {
		public Node Owner { get; set; }
		public string Mesh { get; set; }
		public string Material { get; set; }
		public bool CastShadows { get; set; }
		public bool ReceiveShadows { get; set; }
		public string Name { get; set; }
		public bool Static { get; set; }
	}

	/// <summary>
	/// Representation of Mogre.Vector3
	/// </summary>
	public struct Vector3 {
		public float x;
		public float y;
		public float z;

		public Vector3(float X, float Y, float Z) {
			this.x = X;
			this.y = Y;
			this.z = Z;
		}

		public override string ToString() {
			return "{" + x + ", " + y + ", " + z + "}";
		}

		public static Vector3 operator +(Vector3 one, Vector3 two) {
			return new Vector3(one.x + two.x, one.y + two.y, one.z + two.z);
		}

		public static Vector3 operator /(Vector3 one, float scalar) {
			return new Vector3(one.x / scalar, one.y / scalar, one.z / scalar);
		}

		public static Vector3 operator -(Vector3 one, Vector3 two) {
			return new Vector3(one.x - two.x, one.y - two.y, one.z - two.z);
		}
	}

	/// <summary>
	/// Representation of Mogre.Quaternion
	/// </summary>
	public class Quaternion {
		public float x;
		public float y;
		public float z;
		public float w;

		public Quaternion(float X, float Y, float Z, float W) {
			this.x = X;
			this.y = Y;
			this.z = Z;
			this.w = W;
		}

		public override string ToString() {
			return "{" + x + ", " + y + ", " + z + ", " + w + "}";
		}
	}

	/// <summary>
	/// Representation of BulletSharp.CollisionShape
	/// </summary>
	public class Shape : Block {
		public ShapeTypes Type { get; set; }
		public float Height { get; set; }
		public float Radius { get; set; }
		public string Name { get; set; }

		public override string ToString() {
			return Name + " (" + Type + ")";
		}
	}

	/// <summary>
	/// For the different CollisionShape types
	/// 
	/// Casting these to an int gives you the matching index in the type combo box
	/// </summary>
	public enum ShapeTypes {
		Box = 0, 
		Sphere = 1,
		Capsule = 2,
		Cylinder = 3,
		Cone = 4
	}
}
