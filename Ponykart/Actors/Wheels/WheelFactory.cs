using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Mogre;

namespace Ponykart.Actors {
	/// <summary>
	/// A class to read .wheel files and stick them into a wheel object
	/// </summary>
	public class WheelFactory {
		// our list of wheels
		private IDictionary<string, IDictionary<string, float>> wheels;

		static CultureInfo culture = CultureInfo.InvariantCulture;

		/// <summary>
		/// Constructor.
		/// </summary>
		public WheelFactory() {
			wheels = new Dictionary<string, IDictionary<string, float>>();

			ReadWheelsFromFiles();
		}

		/// <summary>
		/// Go through our media/wheels/ directory and find all of the wheel definitions we have, then make dictionaries out of them
		/// and add them to our one big dictionary.		
		/// </summary>
		public void ReadWheelsFromFiles() {
			// since we can run this whenever (like when we're tweaking files), we want to clear our dictionary first
			wheels.Clear();

			// get all of the filenames of the files in media/wheels/
			IEnumerable<string> files = Directory.EnumerateFiles("media/vehicles/", "*.wheel", SearchOption.AllDirectories);

			foreach (string filename in files) {
				// I forgot ogre had this functionality already built in
				ConfigFile cfile = new ConfigFile();
				cfile.Load(filename, "=", true);

				// each .wheel file can have multiple [sections]. We'll use each section name as the wheel name
				ConfigFile.SectionIterator sectionIterator = cfile.GetSectionIterator();
				while (sectionIterator.MoveNext()) {
					string wheelname = sectionIterator.CurrentKey;
					// make a dictionary
					var wheeldict = new Dictionary<string, float>();
					// go over every property in the file and add it to the dictionary, parsing it as a float
					foreach (KeyValuePair<string, string> pair in sectionIterator.Current) {
						wheeldict.Add(pair.Key, float.Parse(pair.Value, culture));
					}

					wheels[wheelname] = wheeldict;
				}

				cfile.Dispose();
				sectionIterator.Dispose();
			}
		}

		/// <summary>
		/// Creates a wheel.
		/// </summary>
		/// <param name="wheelName">
		/// The name of the wheel type you want to create. Should be the same as the filename, minus the extension. Case sensitive!
		/// </param>
		public Wheel CreateWheel(string wheelName, WheelID ID, Kart owner, Vector3 position, string meshName) {
			IDictionary<string, float> dict = wheels[wheelName];

			Wheel wheel = new Wheel(owner, position, ID, dict, meshName);

			return wheel;
		}
	}
}
