
namespace PonykartParsers {
	/// <summary>
	/// Represents a Billboard { } block in a .thing file
	/// </summary>
	public class BillboardBlock : TokenHolder {
		public BillboardSetBlock Owner { get; protected set; }

		public BillboardBlock(BillboardSetBlock owner) {
			Owner = owner;
			SetUpDictionaries();
		}
	}
}
