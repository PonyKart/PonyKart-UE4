using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Mogre;
using PonykartParsers;

namespace MuffinToScene {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		CultureInfo culture = CultureInfo.InvariantCulture;
		/// <summary>
		/// Original filename we got when we imported the .scene - this is reused when we export
		/// </summary>
		string originalFilename;
		MuffinDefinition definition;

		public MainWindow() {
			AreWeDoneLoading = false;

			// create the window and everything
			InitializeComponent();

			// set the window's icon to our resource
			MemoryStream iconStream = new MemoryStream();
			Properties.Resources.Icon_1.Save(iconStream);
			iconStream.Seek(0, SeekOrigin.Begin);
			this.Icon = BitmapFrame.Create(iconStream);

			// okay, done initialising
			AreWeDoneLoading = true;
		}

		private void importButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".muffin";
			dlg.Filter = "Lymph Muffin files|*.muffin";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				originalFilename = dlg.FileName;

				definition = new MuffinImporter().ParseByFile(dlg.FileName);

				MessageBox.Show("Import successful!", originalFilename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void exportButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".scene";
			dlg.Filter = "Ogre DotScene files|*.scene";
			dlg.FileName = originalFilename.Substring(originalFilename.LastIndexOf("\\") + 1).Replace(".muffin", ".scene");

			bool? result = dlg.ShowDialog();

			if (result == true) {
				// write the .scene
				string filename = dlg.FileName;

				using (var stream = File.Create(filename)) {
					using (var writer = new StreamWriter(stream)) {
						// some required scene stuff
						writer.WriteLine(
@"<scene formatVersion=""1.0"" upAxis=""y"" unitsPerMeter=""39.3701"" minOgreVersion=""1.7"" author=""Ponykart's muffin to scene converter"">
	<environment>
		<colourAmbient r=""0.8"" g=""0.8"" b=""0.8"" />
	</environment>
	<nodes>");
						foreach (ThingBlock block in definition.ThingBlocks) {
							writer.WriteLine(string.Format(culture, @"		<node name=""{0}#{1}"">", block.ThingName, newID));
							Vector3 pos = block.GetVectorProperty("position", null);
							writer.WriteLine(string.Format(culture, @"			<position x=""{0}"" y=""{1}"" z=""{2}"" />", pos.x, pos.y, pos.z));
							Quaternion orient = block.GetQuatProperty("orientation", Quaternion.IDENTITY);
							writer.WriteLine(string.Format(culture, @"			<rotation qx=""{0}"" qy=""{1}"" qz=""{2}"" qw=""{3}"" />", orient.x, orient.y, orient.z, orient.w));
							Vector3 sca = block.GetVectorProperty("scale", Vector3.UNIT_SCALE);
							writer.WriteLine(string.Format(culture, @"			<scale x=""{0}"" y=""{1}"" z=""{2}"" />", sca.x, sca.y, sca.z));
							writer.WriteLine(string.Format(culture, @"			<entity name=""{0}"" meshFile=""{0}.mesh"">", block.ThingName));
							writer.WriteLine(						@"				<subentities>");
							writer.WriteLine(string.Format(culture, @"					<subentity index=""0"" materialName=""{0}"" />", block.ThingName));
							writer.WriteLine(
@"				</subentities>
			</entity>
		</node>");
						}

						// last bit
						writer.WriteLine("\t</nodes>");
						writer.WriteLine("</scene>");
					}
				}

				MessageBox.Show("Export successful!", filename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		string b(bool squishyMarshmallowButthole) {
			return squishyMarshmallowButthole.ToString().ToLower(culture);
		}

		long id = 0;
		long newID {
			get {
				return id++;
			}
		}
	}
}
