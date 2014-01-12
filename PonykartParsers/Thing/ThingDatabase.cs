using System.Collections.Generic;

namespace PonykartParsers {
	public class ThingDatabase {
		IDictionary<string, ThingDefinition> Definitions;

		public ThingDatabase() {
			Definitions = new Dictionary<string, ThingDefinition>();
		}

		public ThingDefinition GetThingDefinition(string name) {
			ThingDefinition def;
			if (Definitions.TryGetValue(name, out def))
				return def;
			else {
				def = new ThingImporter().Parse(name);
				Definitions.Add(name, def);
				return def;
			}
		}

		/// <summary>
		/// Disposes of everything in the thing database, including stuff like collision shapes.
		/// </summary>
		public void ClearDatabase() {
			foreach (ThingDefinition def in Definitions.Values) {
				def.Dispose();
			}
			Definitions.Clear();
		}
	}
}
