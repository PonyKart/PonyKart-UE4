using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Mogre;
using PonykartParsers.MuffinParser;
using PonykartParsers.Properties;
using Node = PonykartParsers.MuffinParser.Node;

namespace PonykartParsers {
	public class MuffinImporter {
		private RuleInstance root;
		private static CultureInfo culture = CultureInfo.InvariantCulture;
		private Collection<string> extraFiles;

		/// <summary>
		/// Parses a .muffin file and puts it into a MuffinDefinition
		/// </summary>
		/// <param name="nameOfMuffin">The name of the muffin to load. This will be used as a filename</param>
		/// <param name="worldDef">
		/// If you've already got a muffin definition, pass it here and this method will add to it
		/// instead of making a new one.
		/// </param>
		/// <returns>A muffin definition with the stuff from the specified muffin file.</returns>
		public MuffinDefinition ParseByName(string nameOfMuffin, MuffinDefinition worldDef = null) {
			// the initial level before we start loading one is "null", so we need to avoid doing anything with that.
			if (nameOfMuffin == null) {
				MuffinDefinition emptyDef = new MuffinDefinition(string.Empty);
				emptyDef.EnumTokens["type"] = ThingEnum.EmptyLevel;
				emptyDef.Finish();
				return emptyDef;
			}

			// make the file path
			string filePath = Settings.Default.MuffinFileLocation + nameOfMuffin + Settings.Default.MuffinFileExtension;
			// if the muffin file was not found
			if (!File.Exists(filePath)) {
				throw new ArgumentException(nameOfMuffin + ".muffin not found!", "nameOfMuffin");
			}

			return ParseByFile(filePath, worldDef);
		}

		/// <summary>
		/// Parses a .muffin file and puts it into a MuffinDefinition
		/// </summary>
		/// <param name="filePath">The filepath of the muffin to load.</param>
		/// <param name="worldDef">
		/// If you've already got a muffin definition, pass it here and this method will add to it
		/// instead of making a new one.
		/// </param>
		/// <returns>A muffin definition with the stuff from the specified muffin file.</returns>
		public MuffinDefinition ParseByFile(string filePath, MuffinDefinition worldDef = null) {
			string fileContents = string.Empty;

			if (LogManager.Singleton != null)
				LogManager.Singleton.LogMessage("[MuffinImporter] Importing and parsing world: " + filePath);
			Debug.WriteLine("[MuffinImporter] Importing and parsing world: " + filePath);

			// read stuff
			using (var fileStream = File.Open(filePath, FileMode.Open)) {
				using (var reader = new StreamReader(fileStream)) {
					// for each line in the file
					while (!reader.EndOfStream) {
						fileContents += reader.ReadLine() + Environment.NewLine;
					}
					reader.Close();
				}
				fileStream.Close();
			}

			extraFiles = new Collection<string>();

			Parser p = new Parser();
			root = p.Parse(fileContents);


			if (worldDef == null)
				worldDef = new MuffinDefinition(filePath);

			Parse(worldDef);

			foreach (var file in extraFiles) {
				worldDef.ExtraFiles.Add(file);
			}

			worldDef.Finish();

			return worldDef;
		}

		/// <summary>
		/// Parses right from the root
		/// </summary>
		void Parse(MuffinDefinition worldDef) {
			for (int a = 0; a < root.Children.Length; a++) {
				Node prop = root.Children[a];
				switch (prop.Type) {
					case NodeType.Rule_Property:
						ParseProperty(worldDef, (prop as RuleInstance).Children[0] as RuleInstance);
						break;
					case NodeType.Rule_Block:
						ParseBlock(worldDef, prop as RuleInstance);
						break;
				}
			}
		}

		/// <summary>
		/// Takes a property and calls the appropriate parser method depending on its type
		/// </summary>
		void ParseProperty(TokenHolder holder, RuleInstance prop) {
			string propName = GetNameFromProperty(prop).ToLower(culture);
			// if we have some extra files, load these too
			if (propName == "loadfile")
				extraFiles.Add(ParseStringProperty(prop));
			// otherwise continue as normal
			else {
				switch (prop.Type) {
					case NodeType.Rule_StringProperty:
						holder.StringTokens[propName] = ParseStringProperty(prop);
						break;
					case NodeType.Rule_BoolProperty:
						holder.BoolTokens[propName] = ParseBoolProperty(prop);
						break;
					case NodeType.Rule_EnumProperty:
						holder.EnumTokens[propName] = ParseEnumProperty(prop);
						break;
					case NodeType.Rule_NumericProperty:
						holder.FloatTokens[propName] = ParseFloatProperty(prop);
						break;
					case NodeType.Rule_Vec3Property:
						holder.VectorTokens[propName] = ParseVectorProperty(prop);
						break;
					case NodeType.Rule_QuatProperty:
						holder.QuatTokens[propName] = ParseQuatProperty(prop);
						break;
				}
			}
		}

		/// <summary>
		/// Gets a name out of a property. It's in its own method because it's used all the time.
		/// </summary>
		string GetNameFromProperty(RuleInstance prop) {
			RuleInstance nameRule = prop.Children[0] as RuleInstance;
			Token nameTok = nameRule.Children[0] as Token;
			return nameTok.Image;
		}

		/// <summary>
		/// Parse a string property
		/// </summary>
		string ParseStringProperty(RuleInstance prop) {
			Token valTok = prop.Children[2] as Token;
			string val = valTok.Image;

			// need to substring because we get "\"foo\"" from the file, and we don't want extra quotes
			return val.Substring(1, val.Length - 2);
		}

		/// <summary>
		/// Parse a bool property
		/// </summary>
		bool ParseBoolProperty(RuleInstance prop) {
			Token valTok = prop.Children[2] as Token;
			if (valTok.Type == NodeType.Tok_KeyTrue)
				return true;
			else if (valTok.Type == NodeType.Tok_KeyFalse)
				return false;
			else
				throw new ArgumentException("Boolean property is not true or false! (How did we even get to this point?)", "prop");
		}

		/// <summary>
		/// Parse an enum property. The value must exist in MuffinEnum, but it is not case sensitive.
		/// </summary>
		ThingEnum ParseEnumProperty(RuleInstance prop) {
			RuleInstance valRule = prop.Children[2] as RuleInstance;
			Token valTok = valRule.Children[0] as Token;
			ThingEnum te;
			if (Enum.TryParse<ThingEnum>(valTok.Image, true, out te))
				return te;
			else
				throw new ApplicationException("Unable to parse Enum property!");
		}

		/// <summary>
		/// Parse a float property
		/// </summary>
		float ParseFloatProperty(RuleInstance prop) {
			Token tok = prop.Children[2] as Token;
			float f = float.Parse(tok.Image, culture);

			return f;
		}

		/// <summary>
		/// Parse a vector property, i.e. a triplet of floats separated by commas
		/// </summary>
		Vector3 ParseVectorProperty(RuleInstance prop) {
			Token tok1 = prop.Children[2] as Token;
			Token tok2 = prop.Children[4] as Token;
			Token tok3 = prop.Children[6] as Token;

			float x = float.Parse(tok1.Image, culture);
			float y = float.Parse(tok2.Image, culture);
			float z = float.Parse(tok3.Image, culture);

			return new Vector3(x, y, z);
		}

		/// <summary>
		/// Parse a quaternion property, i.e. a quartet of floats separated by commas.
		/// 
		/// Note that the .scene format uses xyzw but ogre uses wxyz!
		/// </summary>
		Quaternion ParseQuatProperty(RuleInstance prop) {
			Token tok1 = prop.Children[2] as Token;
			Token tok2 = prop.Children[4] as Token;
			Token tok3 = prop.Children[6] as Token;
			Token tok4 = prop.Children[8] as Token;

			float x = float.Parse(tok1.Image, culture);
			float y = float.Parse(tok2.Image, culture);
			float z = float.Parse(tok3.Image, culture);
			float w = float.Parse(tok4.Image, culture);

			return new Quaternion(w, x, y, z);
		}

		/// <summary>
		/// Shape blocks
		/// </summary>
		void ParseBlock(MuffinDefinition worldDef, RuleInstance block) {
			Token nameTok = (block.Children[0] as RuleInstance).Children[0] as Token;

			ThingBlock thingBlock = new ThingBlock(nameTok.Image, worldDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(thingBlock, rule.Children[0] as RuleInstance);
			}
			worldDef.ThingBlocks.Add(thingBlock);
		}
	}
}
