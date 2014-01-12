using Mogre;

namespace PonykartParsers {
	/// <summary>
	/// Represents a Shape { } block in a .thing file.
	/// </summary>
	public class ShapeBlock : TokenHolder {
		public ThingDefinition Owner { get; protected set; }
		public Matrix4 Transform { get; protected set; }

		public ShapeBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
		}

		public override void Finish() {

			Quaternion quat;
			if (!QuatTokens.TryGetValue("orientation", out quat)) {
				Vector3 rot = GetVectorProperty("rotation", Vector3.ZERO);
				quat = GlobalEulerToQuat(new Degree(rot.x), new Degree(rot.y), new Degree(rot.z));
			}

			Vector3 pos = GetVectorProperty("position", Vector3.ZERO);

			Transform = new Matrix4();
			Transform.MakeTransform(pos, Vector3.UNIT_SCALE, quat);
		}

		static Quaternion GlobalEulerToQuat(Radian rotX, Radian rotY, Radian rotZ) {
			Quaternion q1 = new Quaternion(),
					   q2 = new Quaternion(),
					   q3 = new Quaternion(),
					   q = new Quaternion();
			q1.FromAngleAxis(rotX, Vector3.UNIT_X);
			q2.FromAngleAxis(rotY, Vector3.UNIT_Y);
			q3.FromAngleAxis(rotZ, Vector3.UNIT_Z);

			// global axes
			q = q3 * q2 * q1;
			return q;
		}
	}
}
