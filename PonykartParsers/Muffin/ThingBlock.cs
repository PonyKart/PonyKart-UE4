using Mogre;

namespace PonykartParsers {
	/// <summary>
	/// These represent each Thing in the .muffin files
	/// </summary>
	public class ThingBlock : TokenHolder {
		/// <summary>
		/// The name of the .thing file this corresponds with
		/// </summary>
		public string ThingName { get; private set; }
		public MuffinDefinition Owner { get; private set; }
		public Vector3 Position { get; private set; }

		public ThingBlock(string thingName, MuffinDefinition owner) {
			ThingName = thingName;
			Owner = owner;
			SetUpDictionaries();
		}

		public ThingBlock(string thingName, Vector3 position) {
			ThingName = thingName;
			SetUpDictionaries();
			VectorTokens["position"] = position;
		}

		public ThingBlock(string thingName, Vector3 position, Quaternion orientation) {
			ThingName = thingName;
			SetUpDictionaries();
			VectorTokens["position"] = position;
			QuatTokens["orientation"] = orientation;
		}

		public override void Finish() {
			Position = VectorTokens["position"];
		}
	}
}
