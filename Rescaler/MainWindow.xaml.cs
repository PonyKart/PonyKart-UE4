using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;


namespace Rescaler {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public IDictionary<float, float> Floats;
		public IDictionary<int, float> Ints;

		public MainWindow() {
			InitializeComponent();

			// set the window's icon to our resource
			MemoryStream iconStream = new MemoryStream();
			Properties.Resources.Icon_1.Save(iconStream);
			iconStream.Seek(0, SeekOrigin.Begin);
			this.Icon = BitmapFrame.Create(iconStream);

			Floats = new Dictionary<float, float>();
			Ints = new Dictionary<int, float>();
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			Floats.Clear();
			Ints.Clear();

			string text = textBox.Text;
			float rescaleAmount = float.Parse(rescaleBox.Text);

			string[] lines = text.Split('\n');
			List<string> changed = new List<string>();

			// first go find all of the numbers in the text
			foreach (string line in lines) {
				// ignore orientation/rotation properties
				if (line.Contains("Orientation") || line.Contains("Rotation") || line.Contains("TextureCoords")) {
					changed.Add(line);
					continue;
				}

				string[] nums = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

				// go through each token in the text
				foreach (string s in nums) {
					// get rid of all of the commas
					string token = s.Replace(",", "");

					if (token.Contains(".")) {
						// try parsing it as a float. If we can, then stick it in the dictionary along with its reduced one
						float result;
						if (float.TryParse(token, out result)) {
							Floats[result] = result * rescaleAmount;
						}
					}
					else {
						int result;
						if (int.TryParse(token, out result)) {
							Ints[result] = (float) result * rescaleAmount;
						}
					}
				}

				string line2 = line + "";

				// then replace all of those numbers in our text
				foreach (var item in Floats) {
					line2 = line2.Replace(" " + item.Key + ",", " " + item.Value + ",");
					line2 = line2.Replace(" " + item.Key + "\r", " " + item.Value + "\r");
				}
				foreach (var item in Ints) {
					line2 = line2.Replace(" " + item.Key + ",", " " + item.Value + ",");
					line2 = line2.Replace(" " + item.Key + "\r", " " + item.Value + "\r");
				}

				changed.Add(line2);
			}

			string newText = "";
			foreach (var i in changed) {
				newText += i + "\n";
			}
			

			textBox.Text = newText;
		}
	}
}
