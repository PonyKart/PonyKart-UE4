
namespace PonykartParsers {
	/// <summary>
	/// Represents a Model { } block in a .thing file
	/// </summary>
	public class ModelBlock : TokenHolder {
		public ThingDefinition Owner { get; protected set; }

		public ModelBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
		}
	}
}
