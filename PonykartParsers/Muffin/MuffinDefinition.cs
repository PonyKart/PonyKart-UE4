using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PonykartParsers {
	/// <summary>
	/// Represents a .muffin file
	/// </summary>
	public class MuffinDefinition : TokenHolder {
		public string Name { get; private set; }
		public ICollection<ThingBlock> ThingBlocks { get; protected set; }
		/// <summary>
		/// Other .muffin files this one should load.
		/// </summary>
		/// <see cref="Ponykart.Levels.Level.ReadMuffin()"/>
		public ICollection<string> ExtraFiles { get; set; }

		public MuffinDefinition(string name) {
			Name = name;
			SetUpDictionaries();
		}

		public override void SetUpDictionaries() {
			base.SetUpDictionaries();
			ThingBlocks = new Collection<ThingBlock>();
			ExtraFiles = new Collection<string>();
		}

		public override void Finish() {
			foreach (ThingBlock tb in ThingBlocks)
				tb.Finish();
		}

		public override void Dispose() {
			foreach (ThingBlock tb in ThingBlocks)
				tb.Dispose();

			base.Dispose();
		}
	}
}
