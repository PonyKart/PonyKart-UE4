using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents an ogre ribbon
	/// </summary>
	public class RibbonComponent : LDisposable {
		public uint ID { get; protected set; }
		public string Name { get; protected set; }
		/// <summary>
		/// The ribbon emitter
		/// </summary>
		public RibbonTrail Ribbon { get; private set; }
		/// <summary>
		/// The SceneNode that the ribbon is attached to
		/// </summary>
		public SceneNode RibbonNode { get; private set; }
		protected SceneNode TrackedRibbonNode;

		/// <summary>
		/// For ribbons!
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public RibbonComponent(LThing lthing, ThingBlock template, RibbonBlock block) {
			ID = IDs.Incremental;
			var sceneMgr = LKernel.GetG<SceneManager>();

			Name = block.GetStringProperty("name", template.ThingName);

			// if ribbons are disabled, don't bother creating anything
			if (!Options.GetBool("Ribbons"))
				return;

			Ribbon = LKernel.GetG<SceneManager>().CreateRibbonTrail(Name + ID + "Ribbon");

			// set up some properties
			Ribbon.SetMaterialName(block.GetStringProperty("material", "ribbon"));
			Ribbon.TrailLength = block.GetFloatProperty("length", 5f);
			Ribbon.MaxChainElements = (uint) block.GetFloatProperty("elements", 10f);
			Ribbon.SetInitialWidth(0, block.GetFloatProperty("width", 1f));
			Ribbon.SetInitialColour(0, block.GetQuatProperty("colour", new Quaternion(1, 1, 1, 1)).ToColourValue());
			Ribbon.SetColourChange(0, block.GetQuatProperty("colourchange", new Quaternion(0, 0, 0, 3)).ToColourValue());
			Ribbon.SetWidthChange(0, block.GetFloatProperty("widthchange", 1f));

			// attach it to the node
			RibbonNode = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode(Name + ID + "RibbonNode");
			TrackedRibbonNode = lthing.RootNode.CreateChildSceneNode(Name + ID + "TrackedRibbonNode");
			Ribbon.AddNode(TrackedRibbonNode);
			RibbonNode.AttachObject(Ribbon);

			TrackedRibbonNode.Position = block.GetVectorProperty("position", null);
		}

		public override string ToString() {
			return Name + ID + "Ribbon";
		}

		/// <summary>
		/// clean up
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			var sceneMgr = LKernel.GetG<SceneManager>();
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (Options.GetBool("Ribbons") && Ribbon != null && RibbonNode != null) {
				if (disposing) {
					//RibbonNode.DetachObject(Ribbon);
					//foreach (SceneNode n in Ribbon.GetNodeIterator())
					//	Ribbon.RemoveNode(n);
					if (valid) {
						sceneMgr.DestroyRibbonTrail(Ribbon);
						sceneMgr.DestroySceneNode(RibbonNode);
						sceneMgr.DestroySceneNode(TrackedRibbonNode);
					}
				}
				Ribbon.Dispose();
				Ribbon = null;
				RibbonNode.Dispose();
				RibbonNode = null;
			}

			base.Dispose(disposing);
		}
	}
}
