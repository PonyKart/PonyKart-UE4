using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Resources;
using Miyagi.UI;
using Miyagi.UI.Controls;
using MiyagiDiagnostics;

namespace Ponykart.UI {
	/// <summary>
	/// this just has a few methods to load resources for the UI. Everything is static so you shouldn't need to instantiate this class
	/// </summary>
	public class UIResources {
		/// <summary>
		/// Our dictionary of fonts
		/// </summary>
		public static Dictionary<string, Font> Fonts { get; private set; }
		/// <summary>
		/// Our dictionary of skins
		/// </summary>
		public static Dictionary<string, Skin> Skins { get; private set; }

		private static readonly string _resourcesFileLocation = "media/gui/";
		private static readonly string _miyagiXMLExtension = ".mgx";

		/// <summary>
		/// Creates the fonts and the skins we will use
		/// </summary>
		public static void CreateResources(MiyagiSystem system) {
			CreateFonts(system);
			CreateSkins();

			// then fix the fonts so they work correctly
			MiyagiHelper.SetupFonts(
#if DEBUG
				true,
#else
				false,
#endif
				system, Fonts, Skins);

			CreateCursor(system.GUIManager);
		}

		/// <summary>
		/// Creates all the fonts
		/// </summary>
		private static void CreateFonts(MiyagiSystem system) {
			var files = Directory.EnumerateFiles(_resourcesFileLocation + "Fonts", "*" + _miyagiXMLExtension, SearchOption.AllDirectories);

			var fonts = new List<Font>();

			foreach (string file in files) {
				fonts.AddRange(ImageFont.CreateFromXml(file, system));
			}

			Fonts = fonts.ToDictionary(f => f.Name);

			// set BlueHighway as default font
			Font.Default = Fonts["BlueHighway"];
		}

		/// <summary>
		/// Creates all the skins we will use
		/// </summary>
		private static void CreateSkins() {
			// get all of our .mgx files
			var files = Directory.EnumerateFiles(_resourcesFileLocation + "Skins", "*" + _miyagiXMLExtension, SearchOption.AllDirectories);

			var skins = new List<Skin>();

			foreach (string file in files) {
				skins.AddRange(Skin.CreateFromXml(file));
			}

			// done
			Skins = skins.ToDictionary(s => s.Name);
		}

		/// <summary>
		/// Loads up our GUI from an xml file
		/// </summary>
		private static void CreateFromSerialized(MiyagiSystem system) {
			system.SerializationManager.ImportFromFile(_resourcesFileLocation + "cerealized" + _miyagiXMLExtension);

			// the XML only gives us SkinNames and FontNames, so now we have to get them to all use the correct skins/fonts
			foreach (var control in system.GUIManager.AllControls) {
				SkinnedControl sc = control as SkinnedControl;
				if (sc != null) {
					sc.Skin = Skins[sc.SkinName];
				}

				Label l = control as Label;
				if (l != null) {
					l.TextStyle.Font = Fonts[l.TextStyle.FontName];
				}
			}
		}

		/// <summary>
		/// Creates the cursor
		/// </summary>
		public static void CreateCursor(GUIManager guiMgr) {
			guiMgr.Cursor = new Cursor(Skins["CursorSkin"], new Size(16, 16), Point.Empty);
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeLeft, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeTop, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeTopLeft, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.ResizeTopRight, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.TextInput, new Point(8, 8));
			guiMgr.Cursor.SetHotspot(CursorMode.BlockDrop, new Point(8, 8));
		}
	}
}