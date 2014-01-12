//#define VERBOSE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml;

namespace SceneToThing {
	/// <summary>
	/// Loads a .scene file into some makeshift Node and Entity classes
	/// </summary>
	public class DotSceneLoader {

		public List<Node> Nodes { get; set; }
		protected string sceneFileName;


		public DotSceneLoader() { }

		/// <summary>
		/// this is a filename
		/// </summary>
		/// <param name="SceneName">Filename</param>
		public void ParseDotScene(string SceneName) {
			this.sceneFileName = SceneName;
			this.Nodes = new List<Node>();

			XmlDocument XMLDoc = null;
			XmlElement XMLRoot;

			string data = "";

			using (var stream = File.Open(SceneName, FileMode.Open)) {
				using (var reader = new StreamReader(stream)) {
					while (!reader.EndOfStream) {
						data += reader.ReadLine() + "\n";
					}
					reader.Close();
				}
				stream.Close();
			}

			// Open the .scene File
			XMLDoc = new XmlDocument();
			XMLDoc.LoadXml(data);

			// Validate the File
			XMLRoot = XMLDoc.DocumentElement;
			if (XMLRoot.Name != "scene") {
				MessageBox.Show("Invalid .scene File. Missing <scene>. [File: " + sceneFileName + "]", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Process the scene
			processScene(XMLRoot);
		}

		protected float ParseFloat(String s) {
			NumberFormatInfo provider = new NumberFormatInfo();
			provider.NumberDecimalSeparator = ".";
			return float.Parse(s, provider);
		}

		protected String getAttrib(XmlElement XMLNode, String attrib) {
			return getAttrib(XMLNode, attrib, "");
		}

		protected String getAttrib(XmlElement XMLNode, String attrib, String defaultValue) {
			if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
				return XMLNode.GetAttribute(attrib);
			else
				return defaultValue;
		}

		protected bool getAttribBool(XmlElement XMLNode, String parameter) {
			return getAttribBool(XMLNode, parameter, false);
		}

		protected bool getAttribBool(XmlElement XMLNode, String attrib, bool defaultValue) {
			if (string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
				return defaultValue;

			if (XMLNode.GetAttribute(attrib) == "true")
				return true;

			return false;
		}

		protected float getAttribReal(XmlElement XMLNode, String parameter) {
			return getAttribReal(XMLNode, parameter, 0.0f);
		}

		protected float getAttribReal(XmlElement XMLNode, String attrib, float defaultValue) {
			if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
				return ParseFloat(XMLNode.GetAttribute(attrib));
			else
				return defaultValue;
		}

		protected Quaternion parseQuaternion(XmlElement XMLNode) {
			return new Quaternion(
				ParseFloat(XMLNode.GetAttribute("x")),
				ParseFloat(XMLNode.GetAttribute("y")),
				ParseFloat(XMLNode.GetAttribute("z")),
				ParseFloat(XMLNode.GetAttribute("w"))
			);
		}

		protected Quaternion parseRotation(XmlElement XMLNode) {
			return new Quaternion(
				ParseFloat(XMLNode.GetAttribute("qx")),
				ParseFloat(XMLNode.GetAttribute("qy")),
				ParseFloat(XMLNode.GetAttribute("qz")),
				ParseFloat(XMLNode.GetAttribute("qw"))
			);
		}

		protected Vector3 parseVector3(XmlElement XMLNode) {
			return new Vector3(
				ParseFloat(XMLNode.GetAttribute("x")),
				ParseFloat(XMLNode.GetAttribute("y")),
				ParseFloat(XMLNode.GetAttribute("z"))
			);
		}

		protected void processEntity(XmlElement XMLNode, Node node) {
			// Process attributes

			// Create the entity
			Entity ent = new Entity();
			ent.Mesh = getAttrib(XMLNode, "meshFile");
			ent.Name = getAttrib(XMLNode, "name");
			ent.CastShadows = getAttribBool(XMLNode, "castShadows", false);
			ent.ReceiveShadows = getAttribBool(XMLNode, "receiveShadows", true);
			ent.Static = getAttribBool(XMLNode, "static", true);
			//ent.RenderingDistance = getAttribReal(XMLNode, "renderingDistance", 0);
			//ent.Visible = getAttribBool(XMLNode, "visible", true);

			try {
				XmlElement element;
				// Process subentities (?)
				element = (XmlElement) XMLNode.SelectSingleNode("subentities");
				if (element != null) {
					element = (XmlElement) element.FirstChild;
					while (element != null) {
						ent.Material = getAttrib(element, "materialName");
						element = (XmlElement) element.NextSibling;
					}
				}
			}
			catch (Exception e) {
				MessageBox.Show("Error loading entity \"" + ent.Name + "\"! " + e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			node.Entity = ent;
			ent.Owner = node;
#if VERBOSE
			Console.WriteLine("Successfully processed entity \"" + ent.Name + "\"");
#endif
		}

		protected void processNode(XmlElement XMLNode) {
			Node node = new Node();
			node.Name = getAttrib(XMLNode, "name");

			// Process other attributes
			XmlElement element;

			// Process position (?)
			element = (XmlElement) XMLNode.SelectSingleNode("position");
			if (element != null) {
				node.Position = parseVector3(element);
			}

			// Process quaternion (?)
			element = (XmlElement) XMLNode.SelectSingleNode("quaternion");
			if (element != null) {
				node.Orientation = parseQuaternion(element);
			}

			// Process rotation (?)
			element = (XmlElement) XMLNode.SelectSingleNode("rotation");
			if (element != null) {
				node.Orientation = parseRotation(element);
			}

			// Process scale (?)
			element = (XmlElement) XMLNode.SelectSingleNode("scale");
			if (element != null) {
				node.Dimensions = parseVector3(element);
			}

			// Process entity (*)
			element = (XmlElement) XMLNode.SelectSingleNode("entity");
			if (element != null) {
				processEntity(element, node);
			}

			// Process childnodes
			element = (XmlElement) XMLNode.SelectSingleNode("node");
			while (element != null) {
				processNode(element);
				MessageBox.Show("Node with child node found! It will be parsed, but it will probably look wrong ingame! "
					+ "Make sure none of your entity nodes (the ones with the grey icons) have any parent nodes!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
				element = (XmlElement) element.NextSibling;
			}

			Nodes.Add(node);

#if VERBOSE
			Console.WriteLine("Successfully processed node \"" + node.Name + "\"");
#endif
		}

		protected void processNodes(XmlElement XMLNode) {
			XmlElement element;

			// Process node (*)
			element = (XmlElement) XMLNode.SelectSingleNode("node");
			while (element != null) {
				processNode(element);
				XmlNode nextNode = element.NextSibling;
				element = nextNode as XmlElement;
				while (element == null && nextNode != null) {
					nextNode = nextNode.NextSibling;
					element = nextNode as XmlElement;
				}
			}
		}

		protected void processScene(XmlElement XMLRoot) {
			// Process the scene parameters
			string version = getAttrib(XMLRoot, "formatVersion", "unknown");

			Console.WriteLine("==== Parsing dotScene file with version " + version + " [File: " + sceneFileName + "] ====");

			XmlElement element;

			// Process nodes (?)
			element = (XmlElement) XMLRoot.SelectSingleNode("nodes");
			if (element != null)
				processNodes(element);

			Console.WriteLine("==== Successfully parsed dotScene file with version " + version + " [File: " + sceneFileName + "] ====");
		}
	}
}
