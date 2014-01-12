using System.Collections.Generic;
using System.Linq;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// In order to speed up rendering, we split the world into various regions (kinda like chunks in minecraft) and then make billboards for things like
	/// far-off trees. Then when we're far away from the region, we can hide the actual geometry and show these billboards instead, since billboards are
	/// much cheaper to run.
	/// Of course if you have a high-end computer you won't need any of this and can just render all of the geometry fine with no problems, but for other
	/// "decent" computers this gives us a good way of speeding things up.
	/// </summary>
	public class ImposterBillboarder {
		// region + thingName for the key
		IDictionary<string, BillboardSet> billboards;

		public ImposterBillboarder() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);

			billboards = new Dictionary<string, BillboardSet>();
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			billboards.Clear();
		}

		/// <summary>
		/// set up the billboards, but hide them all for now
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			// if it's not a race or the model detail is set to high, we don't want any imposters
			if (eventArgs.NewLevel.Type != LevelType.Race || Options.ModelDetail == ModelDetailOption.High)
				return;

			SceneManager sceneMgr = LKernel.GetG<SceneManager>();
			ThingDatabase database = LKernel.GetG<ThingDatabase>();

			// first we need to get all of things that have regions
			// this sorts all of the blocks into groups divided by their map region, excluding ones with no region
			// I fucking love linq
			var blockGroups = eventArgs.NewLevel.Definition.ThingBlocks.GroupBy(b => b.GetStringProperty("MapRegion", string.Empty))
																	   .Where(c => !string.IsNullOrEmpty(c.Key));

			// then we just iterate through each group
			foreach (var group in blockGroups) {
				// make an average position that we'll use to place the billboards
				float avgX = group.Select(b => b.Position.x).Average();
				float avgY = group.Select(b => b.Position.y).Average();
				float avgZ = group.Select(b => b.Position.z).Average();
				Vector3 averagePosition = new Vector3(avgX, avgY, avgZ);

				// make a node to attach the billboards to
				SceneNode node = sceneMgr.RootSceneNode.CreateChildSceneNode(group.Key + "BillboardNode", averagePosition);

				foreach (var block in group) {
					ThingDefinition def = database.GetThingDefinition(block.ThingName);

					// if the thing doesn't want imposters, then we leave it alone
					// examples of something that might want map regions but no billboards: bushes, flowers, grass
					if (!def.GetBoolProperty("Imposters", false))
						continue;

					string region = block.GetStringProperty("MapRegion", null);					

					BillboardSet billboardSet;
					if (!billboards.TryGetValue(region + block.ThingName, out billboardSet)) {
						// if the billboard set hasn't been created yet, create it
						billboardSet = sceneMgr.CreateBillboardSet(region + block.ThingName, (uint) group.Count());
						// set some properties
						billboardSet.SetMaterialName(def.GetStringProperty("ImposterMaterial", null));
						billboardSet.SetDefaultDimensions(def.GetFloatProperty("ImposterWidth", null), def.GetFloatProperty("ImposterHeight", null));
						billboardSet.SortingEnabled = true;
						billboardSet.BillboardType = BillboardType.BBT_ORIENTED_COMMON;
						billboardSet.CommonDirection = billboardSet.CommonUpVector = Vector3.UNIT_Y;
						billboardSet.BillboardOrigin = BillboardOrigin.BBO_BOTTOM_CENTER;
						if (Options.ModelDetail == ModelDetailOption.Medium) {
							billboardSet.RenderingDistance = 400;
							billboardSet.Visible = false;
						}
						else if (Options.ModelDetail == ModelDetailOption.Low) {
							billboardSet.RenderingDistance = 120;
							billboardSet.Visible = true;
						}
						billboardSet.CastShadows = false;

						// attach the new billboard set
						node.AttachObject(billboardSet);

						// then add it to the dictionary
						this.billboards.Add(billboardSet.Name, billboardSet);
					}

					Billboard bb = billboardSet.CreateBillboard((block.Position - averagePosition) + def.GetVectorProperty("ImposterOffset", Vector3.ZERO));
					Quaternion rectQ;
					if (def.QuatTokens.TryGetValue("impostertexturecoords", out rectQ)) {
						bb.SetTexcoordRect(rectQ.x, rectQ.y, rectQ.z, rectQ.w);
					}
				}
			}
		}

		public void SetBillboardVisibility(string nameOfBillboard, bool visible) {
			billboards[nameOfBillboard].Visible = visible;
		}

		/// <summary>
		/// Sets the visibility of all billboards in the specified map region.
		/// </summary>
		/// <param name="regionName">The name of the map region. Case sensitive.</param>
		/// <param name="visible">Do you want to make them visible or not?</param>
		public void SetVisibility(string regionName, bool visible) {
			var sceneMgr = LKernel.GetG<SceneManager>();
			if (sceneMgr.HasSceneNode(regionName + "BillboardNode"))
				sceneMgr.GetSceneNode(regionName + "BillboardNode").SetVisible(visible, true);
		}
	}
}
