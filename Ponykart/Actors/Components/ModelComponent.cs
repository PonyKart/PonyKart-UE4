﻿using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents an ogre node and mesh
	/// </summary>
	public class ModelComponent : LDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public long ID { get; protected set; }
		public string Name { get; protected set; }
		AnimationState Animation;

		/// <summary>
		/// Creates a model component for a Thing.
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public ModelComponent(LThing lthing, ThingBlock template, ModelBlock block, ThingDefinition def) {
			ID = IDs.New;
			var sceneMgr = LKernel.GetG<SceneManager>();

			Name = block.GetStringProperty("name", template.ThingName);

			// if we're static, set up the static geometry
			if (block.GetBoolProperty("static", false) || def.GetBoolProperty("static", false)) {
				LKernel.GetG<StaticGeometryManager>().Add(this, template, block);
				Entity = null;
			}
			else if (block.GetBoolProperty("instanced", false) || def.GetBoolProperty("instanced", false)) {
				LKernel.GetG<InstancedGeometryManager>().Add(this, template, block);
				Entity = null;
			}
			// otherwise continue as normal
			else {
				Node = lthing.RootNode.CreateChildSceneNode(Name + "Node" + ID);

				// position
				Node.Position = block.GetVectorProperty("position", Vector3.ZERO);
				// orientation
				Node.Orientation = block.GetQuatProperty("orientation", Quaternion.IDENTITY);
				// if orientation was not found, we fall back to rotation
				if (Node.Orientation == Quaternion.IDENTITY) {
					Vector3 rot = block.GetVectorProperty("rotation", Vector3.ZERO);
					if (rot != Vector3.ZERO)
						Node.Orientation = rot.DegreeVectorToGlobalQuaternion();
				}
				// scale
				Node.Scale(block.GetVectorProperty("scale", Vector3.UNIT_SCALE));

				Node.InheritScale = true;
				Node.InheritOrientation = true;
				Node.SetInitialState();

				// make our entity
				Entity = sceneMgr.CreateEntity(Name + "Entity" + ID, block.GetStringProperty("mesh", null));

				// material name
				string materialName = block.GetStringProperty("material", string.Empty);
				if (!string.IsNullOrWhiteSpace(materialName))
					Entity.SetMaterialName(materialName);

				// some other properties
				Entity.CastShadows = block.GetBoolProperty("CastsShadows", false);

				SetupAnimation(block, def);

				// then attach it to the node!
				Node.AttachObject(Entity);
			}
		}

		/// <summary>
		/// Only does simple animations for now
		/// </summary>
		protected void SetupAnimation(ModelBlock block, ThingDefinition def) {
			if (block.GetBoolProperty("animated", false)) {
				Animation = Entity.GetAnimationState(block.GetStringProperty("AnimationName", null));
				Animation.Loop = block.GetBoolProperty("AnimationLooping", true);
				Animation.Enabled = block.GetBoolProperty("AnimationEnabled", true);

				LKernel.GetG<AnimationManager>().Add(Animation);
			}
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			// stop updating the animation if we have one
			if (disposing && Animation != null) {
				LKernel.GetG<AnimationManager>().Remove(Animation);
			}

			var sceneMgr = LKernel.GetG<SceneManager>();
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (Entity != null) {
				if (valid && disposing)
					sceneMgr.DestroyEntity(Entity);
				Entity.Dispose();
				Entity = null;
			}
			if (Node != null) {
				if (valid && disposing)
					sceneMgr.DestroySceneNode(Node);
				Node.Dispose();
				Node = null;
			}

			base.Dispose(disposing);
		}

		public override string ToString() {
			return Node.Name;
		}
	}
}
