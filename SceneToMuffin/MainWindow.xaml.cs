using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SceneToThing;

namespace SceneToMuffin {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		DotSceneLoader DSL;
		/// <summary>
		/// The data that goes in the table
		/// </summary>
		ICollection<NodeData> Data;
		CultureInfo culture = CultureInfo.InvariantCulture;
		/// <summary>
		/// Original filename we got when we imported the .scene - this is reused when we export
		/// </summary>
		string originalFilename;

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

		/// <summary>
		/// Opens up a .scene, duh
		/// </summary>
		private void openButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".scene";
			dlg.Filter = "Ogre DotScene files|*.scene";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				originalFilename = dlg.FileName;
				OpenSceneFile(originalFilename);
			}
		}

		/// <summary>
		/// Takes the stuff from the .scene loader and puts them into our NodeData classes
		/// </summary>
		private void OpenSceneFile(string filename) {
			DSL = new DotSceneLoader();
			DSL.ParseDotScene(filename);

			Data = new Collection<NodeData>();

			foreach (Node node in DSL.Nodes) {
				NodeData data = new NodeData {
					Name = node.Name,
					// basically, if our object's name has a # in it, it's treated as a thing. Otherwise it isn't.
					UsesThing = node.Name.Contains("#"),
					ThingFile = node.Name.Contains("#") ? node.Name.Substring(0, node.Name.IndexOf("#")) : "",
					// have to split up all of these since the table doesn't know what to do with a Vector3 etc
					PosX = node.Position.x,
					PosY = node.Position.y,
					PosZ = node.Position.z,
					OrientX = node.Orientation.x,
					OrientY = node.Orientation.y,
					OrientZ = node.Orientation.z,
					OrientW = node.Orientation.w,
					ScaleX = node.Dimensions.x,
					ScaleY = node.Dimensions.y,
					ScaleZ = node.Dimensions.z,
					// only using these for re-exporting the .scene with the things removed
					Mesh = node.Entity != null ? node.Entity.Mesh : null,
					Material = node.Entity != null ? node.Entity.Material : null,
					Static = node.Entity != null ? node.Entity.Static : false,
					CastShadows = node.Entity != null ? node.Entity.CastShadows : false,
					ReceiveShadows = node.Entity != null ? node.Entity.ReceiveShadows : false,
				};

				Data.Add(data);
			}
			dataGrid.ItemsSource = Data;

			mapRegionTextBox.Text = "";
			Title = ".scene to .muffin converter - " + filename;
		}

		/// <summary>
		/// Exports out a .muffin, as well as a .scene with the things removed if the checkbox is, well, checked
		/// </summary>
		private void exportButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".muffin";
			dlg.Filter = "Lymph Muffin files|*.muffin";
			dlg.FileName = Path.ChangeExtension(originalFilename, ".muffin");

			bool? result = dlg.ShowDialog();

			if (result == true) {
				// write the .muffin
				string filename = dlg.FileName;

				var orderedData = Data.OrderBy(nd => nd.Name);

				using (var stream = File.Create(filename)) {
					using (var writer = new StreamWriter(stream)) {
						// write out this first
						if (levelTypeBox.Text != "None")
							writer.WriteLine("Type = " + levelTypeBox.Text);

						// only use the ones that use the .thing, of course
						foreach (NodeData data in orderedData.Where(d => d.UsesThing)) {
							// .thing file
							writer.WriteLine(data.ThingFile + " {");
							// position is required
							//writer.WriteLine(string.Format(culture, @"	Name = ""{0}""", data.Name));
							writer.WriteLine(string.Format(culture, @"	Position = {0}, {1}, {2}", data.PosX, data.PosY, data.PosZ));
							// orientation isn't required
							if (data.OrientX != 0 || data.OrientY != 0 || data.OrientZ != 0 || data.OrientW != 1)
								writer.WriteLine(string.Format(culture, @"	Orientation = {0}, {1}, {2}, {3}", data.OrientX, data.OrientY, data.OrientZ, data.OrientW));
							// neither is scale
							if (data.ScaleX != 1 || data.ScaleY != 1 || data.ScaleZ != 1)
								writer.WriteLine(string.Format(culture, @"	Scale = {0}, {1}, {2}", data.ScaleX, data.ScaleY, data.ScaleZ));
							if (!string.IsNullOrEmpty(mapRegionTextBox.Text))
								writer.WriteLine(string.Format(culture, @"	MapRegion = ""{0}""", mapRegionTextBox.Text));
							// don't forget this bit!
							writer.WriteLine("}");
						}
					}
				}

				if (reexportCheckBox.IsChecked == true) {
					// and then we need to remake the .scene file with the .things taken out
					filename = filename.Replace(".muffin", ".scene");

					using (var stream = File.Create(filename)) {
						using (var writer = new StreamWriter(stream)) {
							// some required scene stuff
							writer.WriteLine(
@"<scene formatVersion=""1.0"" upAxis=""y"" unitsPerMeter=""39.3701"" minOgreVersion=""1.7"" author=""Ponykart's scene to muffin converter"">
	<environment>
	</environment>
	<nodes>");
							// only use the ones that aren't going in the muffin file
							foreach (NodeData data in orderedData.Where(d => !d.UsesThing)) {
								writer.WriteLine(string.Format(culture, @"		<node name=""{0}"">", data.Name));
								writer.WriteLine(string.Format(culture, @"			<position x=""{0}"" y=""{1}"" z=""{2}"" />", data.PosX, data.PosY, data.PosZ));
								writer.WriteLine(string.Format(culture, @"			<scale x=""{0}"" y=""{1}"" z=""{2}"" />", data.ScaleX, data.ScaleY, data.ScaleZ));
								writer.WriteLine(string.Format(culture, @"			<rotation qx=""{0}"" qy=""{1}"" qz=""{2}"" qw=""{3}"" />",
									data.OrientX, data.OrientY, data.OrientZ, data.OrientW));
								writer.WriteLine(string.Format(culture, @"			<entity name=""{0}"" castShadows=""{1}"" receiveShadows=""{2}"" meshFile=""{3}"" static=""{4}"">",
									data.Name, b(data.CastShadows), b(data.ReceiveShadows), data.Mesh, b(data.Static)));
								// material
								writer.WriteLine(						@"				<subentities>");
								writer.WriteLine(string.Format(culture, @"					<subentity index=""0"" materialName=""{0}"" />", data.Material));
								// this stuff
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
				}
				MessageBox.Show("Export successful!", filename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		string b(bool squishyMarshmallowButthole) {
			return squishyMarshmallowButthole.ToString().ToLower(culture);
		}
	}
}
