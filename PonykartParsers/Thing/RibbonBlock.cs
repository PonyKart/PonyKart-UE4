
namespace PonykartParsers {
	/// <summary>
	/// Represents a Ribbon { } block in a .thing file
	/// </summary>
	public class RibbonBlock : TokenHolder {
		public ThingDefinition Owner { get; protected set; }

		public RibbonBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
		}
	}
}
