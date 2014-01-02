using System.Collections.Generic;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	public class StaticGeometryManager {
		IDictionary<string, StaticGeometry> sgeoms;
		IDictionary<string, Entity> ents;
		private readonly float _staticRegionSize = 4f;
		private readonly Vector3 regionDimensions;

		public StaticGeometryManager() {
			ents = new Dictionary<string, Entity>();
			sgeoms = new Dictionary<string, StaticGeometry>();
			regionDimensions = new Vector3(_staticRegionSize, 200, _staticRegionSize);

			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			ents.Clear();

			var sceneMgr = LKernel.GetG<SceneManager>();
			foreach (StaticGeometry sg in sgeoms.Values) {
				if (sg != null) {
					sceneMgr.DestroyStaticGeometry(sg);
					sg.Dispose();
				}
			}
			sgeoms.Clear();
		}

		/// <summary>
		/// Adds all of the geometry used by a model component to the static geometry.
		/// This is used by the ModelComponent.
		/// </summary>
		public void Add(ModelComponent mc, ThingBlock template, ModelBlock block, ThingDefinition def) {
			// if the model detail option is low and this model wants imposters, don't even make any static geometry of it
			if (Options.ModelDetail == ModelDetailOption.Low) {
				if (def.GetBoolProperty("Imposters", false))
					return;
			}

			var sceneMgr = LKernel.GetG<SceneManager>();

			string meshName = block.GetStringProperty("mesh", null);
			// map region goes first
			string sgeomName = template.GetStringProperty("MapRegion", "(Default)");
			// static group can override map region
			sgeomName = block.GetStringProperty("StaticGroup", sgeomName);
			Entity ent;

			// get our entity if it already exists
			if (!ents.TryGetValue(meshName, out ent)) {
				// getting the entity was not successful, so we have to create it
				ent = sceneMgr.CreateEntity(meshName + mc.ID, meshName);
				string material;
				if (block.StringTokens.TryGetValue("material", out material))
					ent.SetMaterialName(material);

				ents.Add(meshName, ent);
			}

			Vector3 pos;
			// two ways to get the position
			// inherit it from the lthing, the default (if we were using nodes, this would be the default too)
			if (block.GetBoolProperty("InheritOrientation", true)) {
				pos = (mc.Owner.SpawnOrientation * block.GetVectorProperty("position", Vector3.ZERO)) + template.VectorTokens["position"];
			}
			// or we can choose not to inherit it for whatever reason
			else {
				pos = block.GetVectorProperty("position", Vector3.ZERO) + template.VectorTokens["position"];
			}
			Quaternion orient = block.GetQuatProperty("orientation", Quaternion.IDENTITY) * template.GetQuatProperty("orientation", Quaternion.IDENTITY);
			Vector3 sca = block.GetVectorProperty("scale", Vector3.UNIT_SCALE);

			StaticGeometry sg;
			if (!sgeoms.TryGetValue(sgeomName, out sg)) {
				sg = LKernel.GetG<SceneManager>().CreateStaticGeometry(sgeomName);

				sg.RegionDimensions = regionDimensions;
				sg.RenderingDistance = 300 / 5f;

				sgeoms.Add(sgeomName, sg);
			}

			sg.AddEntity(ent, pos, orient, sca);
		}

		/// <summary>
		/// builds the geometry. Is called after everything else has been created.
		/// </summary>
		public void Build() {
			foreach (StaticGeometry sg in sgeoms.Values) {
				System.Console.WriteLine("Building static geometry: " + sg.Name);
				sg.Build();
			}

			var sceneMgr = LKernel.Get<SceneManager>();
			foreach (Entity e in ents.Values) {
				sceneMgr.DestroyEntity(e);
				e.Dispose();
			}
		}

		public void ToggleVisible() {
			foreach (StaticGeometry sg in sgeoms.Values) {
				sg.SetVisible(!sg.IsVisible);
			}
		}

		/// <summary>
		/// Sets the visibility of all static geometry objects in the specified map region.
		/// </summary>
		/// <param name="regionName">The name of the map region. Case sensitive</param>
		/// <param name="visible">Do you want to make them visible or not?</param>
		public void SetVisibility(string regionName, bool visible) {
			StaticGeometry sg;
			if (sgeoms.TryGetValue(regionName, out sg)) {
				if (sg.IsVisible != visible)
					sg.SetVisible(visible);
			}
			else
				Launch.Log("[ERROR] Tried to change the visibility of the static geometry with name \"" + regionName + "\", but it doesn't exist!");
		}
	}
}
