using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PonykartParsers {
	/// <summary>
	/// Represents a BillboardSet { } block in a .thing file
	/// </summary>
	public class BillboardSetBlock : TokenHolder {
		public ThingDefinition Owner { get; protected set; }
		public ICollection<BillboardBlock> BillboardBlocks { get; protected set; }

		public BillboardSetBlock(ThingDefinition owner) {
			Owner = owner;
			SetUpDictionaries();
		}

		public override void SetUpDictionaries() {
			BillboardBlocks = new Collection<BillboardBlock>();
			base.SetUpDictionaries();
		}

		public override void Dispose() {
			foreach (BillboardBlock block in BillboardBlocks)
				block.Dispose();
			BillboardBlocks.Clear();
		}
	}
}
