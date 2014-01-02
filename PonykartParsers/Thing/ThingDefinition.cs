using System.Collections.Generic;

namespace PonykartParsers {
	/// <summary>
	/// Represents a .thing file - these are one per file and not one per LThing object!
	/// </summary>
	public class ThingDefinition : TokenHolder {
		public string Name { get; protected set; }
		public IList<ShapeBlock> ShapeBlocks { get; protected set; }
		public IList<ModelBlock> ModelBlocks { get; protected set; }
		public IList<RibbonBlock> RibbonBlocks { get; protected set; }
		public IList<BillboardSetBlock> BillboardSetBlocks { get; protected set; }
		public IList<SoundBlock> SoundBlocks { get; protected set; }

		public ThingDefinition(string name) {
			Name = name;
			SetUpDictionaries();
		}

		public override void SetUpDictionaries() {
			base.SetUpDictionaries();
			ShapeBlocks = new List<ShapeBlock>();
			ModelBlocks = new List<ModelBlock>();
			RibbonBlocks = new List<RibbonBlock>();
			BillboardSetBlocks = new List<BillboardSetBlock>();
			SoundBlocks = new List<SoundBlock>();
		}

		/// <summary>
		/// Must be called after you're done importing everything into the dictionaries
		/// </summary>
		public override void Finish() {
			foreach (ShapeBlock sb in ShapeBlocks)
				sb.Finish();
			foreach (ModelBlock mb in ModelBlocks)
				mb.Finish();
			foreach (RibbonBlock rb in RibbonBlocks)
				rb.Finish();
			foreach (BillboardSetBlock bb in BillboardSetBlocks)
				bb.Finish();
			foreach (SoundBlock sb in SoundBlocks)
				sb.Finish();
		}

		public override void Dispose() {
			foreach (ShapeBlock sb in ShapeBlocks)
				sb.Dispose();
			foreach (ModelBlock mb in ModelBlocks)
				mb.Dispose();
			foreach (RibbonBlock rb in RibbonBlocks)
				rb.Dispose();
			foreach (BillboardSetBlock bb in BillboardSetBlocks)
				bb.Dispose();
			foreach (SoundBlock sb in SoundBlocks)
				sb.Dispose();

			base.Dispose();
		}
	}
}
