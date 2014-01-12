using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BulletSharp;
using Mogre;

namespace Ponykart.Physics {
	public class PhysicsMaterialFactory {
		IDictionary<string, PhysicsMaterial> materials;
		PhysicsMaterial defaultMat;

		static CultureInfo culture = CultureInfo.InvariantCulture;

		public PhysicsMaterialFactory() {
			materials = new Dictionary<string, PhysicsMaterial>();
			defaultMat = new PhysicsMaterial();

			ReadMaterialsFromFiles();
		}

		/// <summary>
		/// Go through our media/physicsmaterials/ directory and find all of the material definitions we have, then make objects out
		/// of them and add them to our dictionary.
		/// </summary>
		public void ReadMaterialsFromFiles() {
			// since we can run this whenever (like when we're tweaking files), we want to clear this first
			materials.Clear();

			// get all of the filenames of the files in media/physicsmaterials
			IEnumerable<string> files = Directory.EnumerateFiles("media/physicsmaterials/", "*.physmat");

			foreach (string filename in files) {
				// rev up those files
				ConfigFile cfile = new ConfigFile();
				cfile.Load(filename, "=", true);

				ConfigFile.SectionIterator sectionIterator = cfile.GetSectionIterator();
				while (sectionIterator.MoveNext()) {
					string matname = sectionIterator.CurrentKey;

					PhysicsMaterial mat = new PhysicsMaterial {
						Friction = float.Parse(cfile.GetSetting("Friction", matname, PhysicsMaterial.DEFAULT_FRICTION.ToString()), culture),
						Bounciness = float.Parse(cfile.GetSetting("Bounciness", matname, PhysicsMaterial.DEFAULT_BOUNCINESS.ToString()), culture),
						AngularDamping = float.Parse(cfile.GetSetting("AngularDamping", matname, PhysicsMaterial.DEFAULT_ANGULAR_DAMPING.ToString()), culture),
						LinearDamping = float.Parse(cfile.GetSetting("LinearDamping", matname, PhysicsMaterial.DEFAULT_LINEAR_DAMPING.ToString()), culture),
					};

					materials[matname] = mat;
				}

				cfile.Dispose();
				sectionIterator.Dispose();
			}
		}

		/// <summary>
		/// Gets a material from the dictionary.
		/// </summary>
		/// <returns>If the material with that name was not found, this just returns the default material.</returns>
		public PhysicsMaterial GetMaterial(string materialName) {
			PhysicsMaterial mat;
			if (materials.TryGetValue(materialName, out mat))
				return mat;
			else if (materialName == "Default")
				return defaultMat;
			else {
				Launch.Log("[PhysicsMaterialFactory] Material \"" + materialName + "\" did not exist! Applying default...");
				return defaultMat;
			}
		}

		/// <summary>
		/// Only applies friction and bounciness. Use a RigidBodyConstructionInfo if you want to set the damping.
		/// </summary>
		public void ApplyMaterial(RigidBody body, string material) {
			PhysicsMaterial mat = GetMaterial(material);

			body.Friction = mat.Friction;
			body.Restitution = mat.Bounciness;
		}

		/// <summary>
		/// Applies friction, bounciness, angular damping, and linear damping
		/// </summary>
		public void ApplyMaterial(RigidBodyConstructionInfo info, string material) {
			PhysicsMaterial mat = GetMaterial(material);

			info.Friction = mat.Friction;
			info.Restitution = mat.Bounciness;
			info.AngularDamping = mat.AngularDamping;
			info.LinearDamping = mat.LinearDamping;
		}
	}
}
