using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SceneToMuffin;
using SceneToThing;

namespace SceneToTriggerRegion {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		DotSceneLoader DSL;
		IList<NodeData> Data;
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

		private void button1_Click(object sender, RoutedEventArgs e) {
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

			Data = new List<NodeData>();

			foreach (Node node in DSL.Nodes) {
				NodeData data = new NodeData {
					// we'll use this as the trigger ID
					Name = node.Name.Contains("_")
						? node.Name.Substring(13, node.Name.IndexOf("_") - 13)
						: node.Name.Substring(13),
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
				};
				// and we'll use this as the ID it goes to
				data.ThingFile = node.Name.Contains("_")
						? node.Name.Substring(node.Name.IndexOf("_") + 1)
						: "" + (int.Parse(data.Name, culture) + 1);

				Data.Add(data);
			}
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".tr";
			dlg.Filter = "Trigger Region Files|*.tr";
			dlg.FileName = Path.ChangeExtension(originalFilename, ".tr");

			bool? result = dlg.ShowDialog();

			if (result == true) {
				string filename = dlg.FileName;
				var orderedData = Data.OrderBy(n => n.Name);
				/*
				 * threshold
				 * id idTO height width posX posY posZ orientX orientY orientZ orientW
				 */

				using (var stream = File.Create(filename)) {
					using (StreamWriter writer = new StreamWriter(stream)) {
						// threshold
						writer.WriteLine(textBox1.Text);

						foreach (var tr in orderedData) {
							writer.WriteLine(string.Format(culture, @"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
								int.Parse(tr.Name, culture), int.Parse(tr.ThingFile, culture),
								tr.ScaleY, tr.ScaleZ,
								tr.PosX, tr.PosY, tr.PosZ,
								tr.OrientX, tr.OrientY, tr.OrientZ, tr.OrientW));
						}
					}
				}

				MessageBox.Show("Export successful!", filename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
	}
}
