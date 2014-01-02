using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Mogre;
using PonykartParsers.ThingParser;
using Node = PonykartParsers.ThingParser.Node;

namespace PonykartParsers {
	public class ThingImporter {
		private RuleInstance root;
		private static CultureInfo culture = CultureInfo.InvariantCulture;
		private static IDictionary<string, string> fileList;
#if !DEBUG
		private static bool hasPreparedFileList = false;
#endif
		private static void PrepareFileList() {
#if !DEBUG
			if (!hasPreparedFileList) {
#endif
				fileList = new Dictionary<string, string>();

				foreach (string group in ResourceGroupManager.Singleton.GetResourceGroups().Where(s => ResourceGroupManager.Singleton.IsResourceGroupInitialised(s))) {
					if (group == "Bootstrap")
						continue;

					var resourceLocations = ResourceGroupManager.Singleton.ListResourceLocations(group);

					foreach (string loc in resourceLocations) {
						var scripts = Directory.EnumerateFiles(loc, "*.thing", SearchOption.TopDirectoryOnly);

						foreach (string file in scripts) {
							fileList[Path.GetFileNameWithoutExtension(file)] = file;
						}
					}
				}
#if !DEBUG
				hasPreparedFileList = true;
			}
#endif
		}

		public ThingDefinition Parse(string nameOfThing) {
			PrepareFileList();

			string fileContents = string.Empty;

			// make the file path
			// this just searches for "media/things/foo.thing"
			string filePath;
			// this searches subfolders for .things
			if (!fileList.TryGetValue(nameOfThing, out filePath)) {
				throw new FileNotFoundException(nameOfThing + ".thing does not exist!", nameOfThing);
			}

			LogManager.Singleton.LogMessage("[ThingImporter] Importing and parsing thing: " + filePath);
			Debug.WriteLine("[ThingImporter] Importing and parsing thing: " + filePath);

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

			Parser p = new Parser();
			root = p.Parse(fileContents);


			ThingDefinition thingDef = new ThingDefinition(nameOfThing);

			Parse(thingDef);

			thingDef.Finish();

			return thingDef;
		}

		/// <summary>
		/// Parses right from the root
		/// </summary>
		void Parse(ThingDefinition thingDef) {
			for (int a = 0; a < root.Children.Length; a++) {
				Node prop = root.Children[a];
				switch (prop.Type) {
					case NodeType.Rule_Property:
						ParseProperty(thingDef, (prop as RuleInstance).Children[0] as RuleInstance);
						break;
					case NodeType.Rule_Shape:
						ParseShape(thingDef, prop as RuleInstance);
						break;
					case NodeType.Rule_Model:
						ParseModel(thingDef, prop as RuleInstance);
						break;
					case NodeType.Rule_Ribbon:
						ParseRibbon(thingDef, prop as RuleInstance);
						break;
					case NodeType.Rule_BillboardSet:
						ParseBillboardSet(thingDef, prop as RuleInstance);
						break;
					case NodeType.Rule_Sound:
						ParseSound(thingDef, prop as RuleInstance);
						break;
				}
			}
		}

		/// <summary>
		/// Takes a property and calls the appropriate parser method depending on its type
		/// </summary>
		void ParseProperty(TokenHolder holder, RuleInstance prop) {
			string propName = GetNameFromProperty(prop).ToLower(culture);
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
		/// Parse an enum property. The value must exist in ThingEnum, but it is not case sensitive.
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
		/// Note that the .thing format uses xyzw but ogre uses wxyz!
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
		void ParseShape(ThingDefinition thingDef, RuleInstance block) {
			ShapeBlock shapeBlock = new ShapeBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(shapeBlock, rule.Children[0] as RuleInstance);
			}

			thingDef.ShapeBlocks.Add(shapeBlock);
		}

		/// <summary>
		/// Model blocks
		/// </summary>
		void ParseModel(ThingDefinition thingDef, RuleInstance block) {
			ModelBlock modelBlock = new ModelBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(modelBlock, rule.Children[0] as RuleInstance);
			}

			thingDef.ModelBlocks.Add(modelBlock);
		}

		/// <summary>
		/// Ribbon blocks
		/// </summary>
		void ParseRibbon(ThingDefinition thingDef, RuleInstance block) {
			RibbonBlock ribbonBlock = new RibbonBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(ribbonBlock, rule.Children[0] as RuleInstance);
			}

			thingDef.RibbonBlocks.Add(ribbonBlock);
		}

		/// <summary>
		/// BillboardSet blocks
		/// </summary>
		void ParseBillboardSet(ThingDefinition thingDef, RuleInstance block) {
			BillboardSetBlock billboardSetBlock = new BillboardSetBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(billboardSetBlock, rule.Children[0] as RuleInstance);
				else if (rule.Type == NodeType.Rule_Billboard)
					ParseBillboard(billboardSetBlock, rule);
			}

			thingDef.BillboardSetBlocks.Add(billboardSetBlock);
		}

		/// <summary>
		/// Billboard blocks
		/// </summary>
		/// <param name="bbSet">We pass the bbset block, not the whole thingDefinition</param>
		void ParseBillboard(BillboardSetBlock bbSet, RuleInstance block) {
			BillboardBlock billboardBlock = new BillboardBlock(bbSet);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(billboardBlock, rule.Children[0] as RuleInstance);
			}

			bbSet.BillboardBlocks.Add(billboardBlock);
		}

		/// <summary>
		/// Sound blocks
		/// </summary>
		void ParseSound(ThingDefinition thingDef, RuleInstance block) {
			SoundBlock soundBlock = new SoundBlock(thingDef);

			for (int a = 2; a < block.Children.Length - 1; a++) {
				RuleInstance rule = block.Children[a] as RuleInstance;
				if (rule.Type == NodeType.Rule_Property)
					ParseProperty(soundBlock, rule.Children[0] as RuleInstance);
			}

			thingDef.SoundBlocks.Add(soundBlock);
		}
	}
}
