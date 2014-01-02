using System;
using System.Collections.Generic;
using System.Linq;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	public class InstancedGeometryManager {
		// uses map group + mesh name as a key
		IDictionary<string, InstancedGeometry> igeoms;
		// uses map group + mesh name as a key
		IDictionary<string, IList<Transform>> transforms;
		// uses map group + mesh name as a key
		IDictionary<string, Entity> ents;

		private readonly float _instancedRegionSize = 60f;
		private readonly int _numInstancesPerBatch = 1; // maximum 80
		readonly Vector3 regionDimensions;

		// even though 80 means fewer batches, that also means less culling. So for whatever reason, one mesh per batch seems to be the fastest.

		public InstancedGeometryManager() {
			igeoms = new Dictionary<string, InstancedGeometry>();
			transforms = new Dictionary<string, IList<Transform>>();
			ents = new Dictionary<string, Entity>();
			regionDimensions = new Vector3(_instancedRegionSize, 200, _instancedRegionSize);

			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			ents.Clear();
			transforms.Clear();

			var sceneMgr = LKernel.GetG<SceneManager>();
			foreach (InstancedGeometry ig in igeoms.Values) {
				sceneMgr.DestroyInstancedGeometry(ig);
				ig.Dispose();
			}
			igeoms.Clear();
		}

		public void Add(ModelComponent mc, ThingBlock template, ModelBlock block, ThingDefinition def) {
			// if the model detail option is low and this model wants imposters, don't even make any instanced geometry of it
			if (Options.ModelDetail == ModelDetailOption.Low) {
				if (def.GetBoolProperty("Imposters", false))
					return;
			}

			var sceneMgr = LKernel.GetG<SceneManager>();

			string meshName = block.GetStringProperty("mesh", null);
			string mapRegion = template.GetStringProperty("MapRegion", string.Empty);
			string key = mapRegion + meshName;

			// create our entity if it doesn't exist
			if (!ents.ContainsKey(key)) {
				Entity ent = sceneMgr.CreateEntity(mc.Name + mc.ID, meshName);
				ent.SetMaterialName(block.GetStringProperty("Material", string.Empty));
				// then add it to our dictionary
				ents.Add(key, ent);
			}

			// get our transforms
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

			// put them in one class
			Transform trans = new Transform {
				Position = pos,
				Orientation = orient,
				Scale = sca,
			};

			// if the transforms dictionary doesn't contain the mesh yet, add a new one
			if (!transforms.ContainsKey(key)) {
				transforms.Add(key, new List<Transform>());
			}
			// then put our transform into the dictionary
			transforms[key].Add(trans);
		}

		/// <summary>
		/// 1: Create the entity
		/// 2: Create the instanced geometry
		/// 3: Add the entity 80 times or less to the instanced geometry
		/// 4: Build the instanced geometry
		/// 5: Add as much batches as you need to the geometry (650/80)
		/// 6: Go through the instanced objects of the batches and adept the position, scale and orientation
		/// 7: Release the entity (1)
		/// 
		/// http://www.ogre3d.org/forums/viewtopic.php?f=2&t=67093#p442869
		/// </summary>
		public void Build() {
			var sceneMgr = LKernel.GetG<SceneManager>();

			// first create all of the InstancedGeometry objects
			foreach (var ent in ents) {
				// its name is the mesh name
				InstancedGeometry igeom = sceneMgr.CreateInstancedGeometry(ent.Key);
				igeom.CastShadows = false;
				igeom.BatchInstanceDimensions = regionDimensions;

				// add the entities to our batch
				int numEnts = transforms[ent.Key].Count;
				int numEntsToAdd = numEnts > _numInstancesPerBatch ? _numInstancesPerBatch : numEnts;

				// add the entity ~80 times or less to the geometry
				for (int a = 0; a < numEntsToAdd; a++) {
					igeom.AddEntity(ent.Value, Vector3.ZERO);
				}

				igeom.Origin = Vector3.ZERO;
				igeom.Build();

				// number of batch instances we need is number of entities / ~80, since that's the maximum number of instances a batch can do
				for (int a = 0; a < (int) ((float) numEnts / (float) _numInstancesPerBatch); a++) {
					igeom.AddBatchInstance();
				}

				// now we need to go through each instanced object and update its transform
				int transformIndex = 0;
				var batchIterator = igeom.GetBatchInstanceIterator();
				// first we go through each batch
				foreach (InstancedGeometry.BatchInstance batch in batchIterator) {
					IList<Transform> entTransforms = transforms[ent.Key];

					// then go through each object in the batch
					var objectIterator = batch.GetObjectIterator();

					foreach (InstancedGeometry.InstancedObject obj in objectIterator) {
						if (transformIndex >= entTransforms.Count) {
							obj.Dispose();
							continue;
						}

						// get the transform we'll use for this object
						Transform t = entTransforms[transformIndex];

						// update the object with the transform
						obj.Position = t.Position;
						obj.Orientation = t.Orientation;
						obj.Scale = t.Scale;

						// then increment the index we use to get transforms
						transformIndex++;
					}
				}
				sceneMgr.DestroyEntity(ent.Value);
				ent.Value.Dispose();

				System.Console.WriteLine("Instanced Geometry: " + igeom.Name);
				igeoms.Add(ent.Key, igeom);
			}
		}

		public void ToggleVisible() {
			foreach (InstancedGeometry ig in igeoms.Values) {
				ig.SetVisible(!ig.IsVisible);
			}
		}

		/// <summary>
		/// Sets the visibility of all instanced geometry objects in the specified map region.
		/// </summary>
		/// <param name="regionName">The name of the map region. Case insensitive</param>
		/// <param name="visible">Do you want to make them visible or not?</param>
		public void SetVisibility(string regionName, bool visible) {
			var matchingGeoms = igeoms.Where(k => k.Key.StartsWith(regionName, StringComparison.CurrentCultureIgnoreCase));
			foreach (var pair in matchingGeoms) {
				pair.Value.SetVisible(visible);
			}
		}


		private class Transform {
			public Vector3 Position { get; set; }
			public Quaternion Orientation { get; set; }
			public Vector3 Scale { get; set; }
		}
	}
}
