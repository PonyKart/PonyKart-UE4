
namespace PonykartParsers {
	/// <summary>
	/// Represents a Sound { } block in the .thing file
	/// </summary>
	public class SoundBlock : TokenHolder {
		public ThingDefinition Owner { get; protected set; }

		public SoundBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
		}
	}
}
