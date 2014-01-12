using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace SceneToThing {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		/// <summary>
		/// the window sends out selectionChanged events when it's initialising, and we don't want to respond to those. That's what this is for
		/// </summary>
		public static bool AreWeDoneLoading = false;
		DotSceneLoader DSL;
		ICollection<Block> Blocks;
		CultureInfo culture = CultureInfo.InvariantCulture;

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

		private void openButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".scene";
			dlg.Filter = "Ogre DotScene files (.scene)|*.scene";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				string filename = dlg.FileName;
				OpenSceneFile(filename);
			}
		}

		/// <summary>
		/// Opens and parses a .scene file and puts its components into the Blocks collection.
		/// </summary>
		void OpenSceneFile(string filename) {
			DSL = new DotSceneLoader();
			DSL.ParseDotScene(filename);

			Blocks = new Collection<Block>();

			// go through each of the nodes in the .scene and find which ones are shapes
			foreach (Node node in DSL.Nodes) {
				// shapes
				if (node.Entity.Mesh == "box.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Box;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Dimensions = node.Dimensions;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "sphere.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Sphere;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Radius = node.Dimensions.x / 2f;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "cylinder.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Cylinder;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Dimensions = node.Dimensions;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "cone.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Cone;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Radius = node.Dimensions.x / 2f;
					shape.Height = node.Dimensions.y;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				else if (node.Entity.Mesh == "capsule.mesh") {
					Shape shape = new Shape();
					shape.Type = ShapeTypes.Capsule;
					shape.Position = node.Position;
					shape.Orientation = node.Orientation;
					shape.Radius = node.Dimensions.x / 2f;
					shape.Height = node.Dimensions.y;
					shape.Name = node.Name;

					Blocks.Add(shape);
				}
				// not shapes
				else {
					Blocks.Add(node);
				}
			}

			sceneListBox.Items.Clear();

			// add the blocks to the list box
			foreach (var block in Blocks) {
				ListBoxItem item = new ListBoxItem();
				sceneListBox.Items.Add(block);
			}

			// update the name box
			// remove the .scene from the filename
			filename = filename.Remove(filename.IndexOf(".scene"));
			// get rid of the folder stuff before the scene name
			if (filename.Contains("/"))
				nameBox.Text = filename.Substring(filename.LastIndexOf('/') + 1);
			else if (filename.Contains("\\"))
				nameBox.Text = filename.Substring(filename.LastIndexOf('\\') + 1);
			else
				nameBox.Text = filename;

			saveButton.IsEnabled = true;
			nameBox.IsEnabled = true;
			physicsBox.IsEnabled = true;
		}

		/// <summary>
		/// Export out our blocks into a .thing file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".thing";
			dlg.Filter = "Lymph Thing files|*.thing";
			dlg.FileName = nameBox.Text + ".thing";

			bool? result = dlg.ShowDialog();

			// now we know where to put the file
			if (result == true) {
				string filename = dlg.FileName;

				using (var stream = File.Create(filename)) {
					using (var writer = new StreamWriter(stream)) {
						// write out the "overall" stuff
						writer.WriteLine("Physics = " + physicsBox.Text);
						if (physicsBox.Text != "None")
							writer.WriteLine("Mass = " + massBox.Text);
						if (collisionBox.Text != "None") {
							writer.WriteLine("CollisionGroup = " + collisionBox.Text);
							writer.WriteLine("CollidesWith = " + collisionBox.Text);
						}

						writer.WriteLine();

						if (BillboardCheckBox.IsChecked == true) {
							string[] texCoordSets = BillboardTexCoordsBox.Text.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
							string[] randTexCoordSets;
							Vector3 newCenter = new Vector3(0, 0, 0);

							if (BillboardRecenterCheckBox.IsChecked == true) {
								var nodes = Blocks.Where(b => (b as Node) != null);
								foreach (var n in nodes) {
									newCenter += n.Position;
								}
								newCenter /= nodes.Count();

								foreach (var n in nodes) {
									n.Position -= newCenter;
								}
							}

							writer.WriteLine("BillboardSet {");
							writer.WriteLine(string.Format(culture, @"	Material = ""{0}""", BillboardMaterialBox.Text));
							writer.WriteLine(string.Format(culture, @"	Type = OrientedCommon"));
							writer.WriteLine(string.Format(culture, @"	Width = {0}", BillboardSizeBox.Text));
							writer.WriteLine(string.Format(culture, @"	Height = {0}", BillboardSizeBox.Text));
							
							if (texCoordSets.Length <= 1) {
								if (texCoordSets.Length == 1)
									writer.WriteLine(string.Format(culture, @"	TextureCoords = {0}", BillboardTexCoordsBox.Text));

								foreach (Block block in Blocks) {
									Node node = block as Node;
									if (node != null) {
										writer.WriteLine("\tBillboard {");
										writer.WriteLine(string.Format(culture, @"		Position = {0}, {1}, {2}", node.Position.x, node.Position.y, node.Position.z));
										writer.WriteLine("\t}");
									}
								}
							}
							else if (texCoordSets.Length > 1) {
								int numBillboards = Blocks.Where(b => (b as Node) != null).Count();
								randTexCoordSets = new string[numBillboards];

								for (int a = 0; a < numBillboards; a++) {
									randTexCoordSets[a] = texCoordSets[a % texCoordSets.Length];
								}
								randTexCoordSets = RandomizeStrings(randTexCoordSets);

								int currentIndex = 0;
								foreach (Block block in Blocks) {
									Node node = block as Node;
									if (node != null) {
										writer.WriteLine("\tBillboard {");
										writer.WriteLine(string.Format(culture, @"		Position = {0}, {1}, {2}", node.Position.x, node.Position.y, node.Position.z));
										writer.WriteLine(string.Format(culture, @"		TextureCoords = {0}", randTexCoordSets[currentIndex]));
										writer.WriteLine("\t}");
										currentIndex++;
									}
								}
							}

							if (BillboardRecenterCheckBox.IsChecked == true)
								writer.WriteLine(newCenter);

							writer.WriteLine("}");
						}
						else {
							foreach (Block block in Blocks) {
								// write out our model blocks
								Node node = block as Node;
								if (node != null) {
									writer.WriteLine("// " + node.Name);
									writer.WriteLine("Model {");
									writer.WriteLine(string.Format(culture, @"	Mesh = ""{0}""", node.Entity.Mesh));
									writer.WriteLine(string.Format(culture, @"	Material = ""{0}""", node.Entity.Material));
									writer.WriteLine(string.Format(culture, @"	Position = {0}, {1}, {2}", node.Position.x, node.Position.y, node.Position.z));
									writer.WriteLine(string.Format(culture, @"	Orientation = {0}, {1}, {2}, {3}",
										node.Orientation.x, node.Orientation.y, node.Orientation.z, node.Orientation.w));
									writer.WriteLine(string.Format(culture, @"	Scale = {0}, {1}, {2}", node.Dimensions.x, node.Dimensions.y, node.Dimensions.z));
									writer.WriteLine(string.Format(culture, @"	CastsShadows = {0}", shad(node.Entity.CastShadows)));
									writer.WriteLine("}");
									continue;
								}

								// write out our shape blocks
								Shape shape = block as Shape;
								if (shape != null) {
									writer.WriteLine("// " + shape.Name);
									writer.WriteLine("Shape {");
									writer.WriteLine(string.Format(culture, @"	Type = {0}", shape.Type));
									writer.WriteLine(string.Format(culture, @"	Position = {0}, {1}, {2}", shape.Position.x, shape.Position.y, shape.Position.z));
									writer.WriteLine(string.Format(culture, @"	Orientation = {0}, {1}, {2}, {3}",
										shape.Orientation.x, shape.Orientation.y, shape.Orientation.z, shape.Orientation.w));

									if (shape.Type == ShapeTypes.Box || shape.Type == ShapeTypes.Cylinder) {
										writer.WriteLine(string.Format(culture, @"	Dimensions = {0}, {1}, {2}", shape.Dimensions.x, shape.Dimensions.y, shape.Dimensions.z));
									}
									else if (shape.Type == ShapeTypes.Capsule || shape.Type == ShapeTypes.Cone) {
										writer.WriteLine(string.Format(culture, @"	Height = {0}", shape.Height));
										writer.WriteLine(string.Format(culture, @"	Radius = {0}", shape.Radius));
									}
									else if (shape.Type == ShapeTypes.Sphere) {
										writer.WriteLine(string.Format(culture, @"	Radius = {0}", shape.Radius));
									}
									writer.WriteLine("}");
									continue;
								}
							}
						}
						writer.Close();
					}
					stream.Close();
				}
				MessageBox.Show("Export successful!", filename, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		string b(bool squishyMarshmallowButthole) {
			return squishyMarshmallowButthole.ToString().ToLower(culture);
		}

		string shad(bool raaaaape) {
			return raaaaape ? "Some" : "None";
		}

		/// <summary>
		/// Put all of the info from the selected block into the text boxes
		/// </summary>
		private void sceneListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			// if we have nothing selected, disable all of the boxes
			if (e.AddedItems.Count == 0) {
				positionXBox.Text = string.Empty;
				positionYBox.Text = string.Empty;
				positionZBox.Text = string.Empty;
				orientationXBox.Text = string.Empty;
				orientationYBox.Text = string.Empty;
				orientationZBox.Text = string.Empty;
				orientationWBox.Text = string.Empty;
				dimensionsXBox.Text = string.Empty;
				dimensionsYBox.Text = string.Empty;
				dimensionsZBox.Text = string.Empty;
				radiusBox.Text = string.Empty;
				heightBox.Text = string.Empty;

				typeComboBox.IsEnabled = false;
				positionXBox.IsEnabled = false;
				positionYBox.IsEnabled = false;
				positionZBox.IsEnabled = false;
				orientationXBox.IsEnabled = false;
				orientationYBox.IsEnabled = false;
				orientationZBox.IsEnabled = false;
				orientationWBox.IsEnabled = false;
				dimensionsXBox.IsEnabled = false;
				dimensionsYBox.IsEnabled = false;
				dimensionsZBox.IsEnabled = false;
				radiusBox.IsEnabled = false;
				heightBox.IsEnabled = false;
				saveChangesButton.IsEnabled = false;
				discardChangesButton.IsEnabled = false;
				return;
			}
			// otherwise enable all of them
			else {
				typeComboBox.IsEnabled = true;
				positionXBox.IsEnabled = true;
				positionYBox.IsEnabled = true;
				positionZBox.IsEnabled = true;
				orientationXBox.IsEnabled = true;
				orientationYBox.IsEnabled = true;
				orientationZBox.IsEnabled = true;
				orientationWBox.IsEnabled = true;
				dimensionsXBox.IsEnabled = true;
				dimensionsYBox.IsEnabled = true;
				dimensionsZBox.IsEnabled = true;
				radiusBox.IsEnabled = true;
				heightBox.IsEnabled = true;
				saveChangesButton.IsEnabled = true;
				discardChangesButton.IsEnabled = true;
			}

			// make sure it's actually a block
			Block item = e.AddedItems[0] as Block;
			if (item == null)
				throw new ArgumentException("Selected item was not a Block!");

			// try using the block as a node
			Node node = item as Node;
			if (node != null) {
				typeComboBox.IsEnabled = false;
				dimensionsLabel.Content = "Scale:";
				positionXBox.Text = node.Position.x + string.Empty;
				positionYBox.Text = node.Position.y + string.Empty;
				positionZBox.Text = node.Position.z + string.Empty;
				orientationXBox.Text = node.Orientation.x + string.Empty;
				orientationYBox.Text = node.Orientation.y + string.Empty;
				orientationZBox.Text = node.Orientation.z + string.Empty;
				orientationWBox.Text = node.Orientation.w + string.Empty;
				dimensionsXBox.Text = node.Dimensions.x + string.Empty;
				dimensionsYBox.Text = node.Dimensions.y + string.Empty;
				dimensionsZBox.Text = node.Dimensions.z + string.Empty;
				radiusBox.IsEnabled = false;
				heightBox.IsEnabled = false;
				return;
			}

			// otherwise try using it as a shape
			Shape shape = item as Shape;
			if (shape != null) {
				typeComboBox.SelectedIndex = (int) shape.Type;
				typeComboBox.IsEnabled = true;
				dimensionsLabel.Content = "Dimensions:";
				positionXBox.Text = shape.Position.x + string.Empty;
				positionYBox.Text = shape.Position.y + string.Empty;
				positionZBox.Text = shape.Position.z + string.Empty;
				orientationXBox.Text = shape.Orientation.x + string.Empty;
				orientationYBox.Text = shape.Orientation.y + string.Empty;
				orientationZBox.Text = shape.Orientation.z + string.Empty;
				orientationWBox.Text = shape.Orientation.w + string.Empty;

				// since all of the different shapes use different boxes, we have to disable the ones we don't want
				if (shape.Type == ShapeTypes.Box || shape.Type == ShapeTypes.Cylinder) {
					dimensionsXBox.Text = shape.Dimensions.x + string.Empty;
					dimensionsYBox.Text = shape.Dimensions.y + string.Empty;
					dimensionsZBox.Text = shape.Dimensions.z + string.Empty;
					radiusBox.Text = string.Empty;
					radiusBox.IsEnabled = false;
					heightBox.Text = string.Empty;
					heightBox.IsEnabled = false;
				}
				else if (shape.Type == ShapeTypes.Capsule || shape.Type == ShapeTypes.Cone) {
					dimensionsXBox.Text = string.Empty;
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.Text = string.Empty;
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.Text = string.Empty;
					dimensionsZBox.IsEnabled = false;
					radiusBox.Text = shape.Radius + string.Empty;
					heightBox.Text = shape.Height + string.Empty;
				}
				else if (shape.Type == ShapeTypes.Sphere) {
					dimensionsXBox.Text = string.Empty;
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.Text = string.Empty;
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.Text = string.Empty;
					dimensionsZBox.IsEnabled = false;
					radiusBox.Text = shape.Radius + string.Empty;
					heightBox.Text = string.Empty;
					heightBox.IsEnabled = false;
				}
			}
		}

		/// <summary>
		/// Take the stuff in the text boxes and update the block object
		/// </summary>
		private void saveChangesButton_Click(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			// make sure it's a block
			Block item = sceneListBox.SelectedItem as Block;
			if (item == null)
				throw new ArgumentException("Selected item was not a Block!");

			// try as a node
			Node node = item as Node;
			if (node != null) {
				try {
					node.Position = new Vector3(float.Parse(positionXBox.Text), float.Parse(positionYBox.Text), float.Parse(positionZBox.Text));
					node.Orientation = new Quaternion(float.Parse(orientationXBox.Text), float.Parse(orientationYBox.Text),
						float.Parse(orientationZBox.Text), float.Parse(orientationWBox.Text));
					node.Dimensions = new Vector3(float.Parse(dimensionsXBox.Text), float.Parse(dimensionsYBox.Text), float.Parse(dimensionsZBox.Text));
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			// try as a shape
			Shape shape = item as Shape;
			if (shape != null) {
				try {
					// new type
					shape.Type = (ShapeTypes) typeComboBox.SelectedIndex;
					// new position
					shape.Position = new Vector3(float.Parse(positionXBox.Text), float.Parse(positionYBox.Text), float.Parse(positionZBox.Text));
					// new orientation
					shape.Orientation = new Quaternion(float.Parse(orientationXBox.Text), float.Parse(orientationYBox.Text),
						float.Parse(orientationZBox.Text), float.Parse(orientationWBox.Text));
					// new dimensions/radius/height
					if (shape.Type == ShapeTypes.Box || shape.Type == ShapeTypes.Cylinder) {
						shape.Dimensions = new Vector3(float.Parse(dimensionsXBox.Text), float.Parse(dimensionsYBox.Text), float.Parse(dimensionsZBox.Text));
					}
					else if (shape.Type == ShapeTypes.Capsule || shape.Type == ShapeTypes.Cone) {
						shape.Radius = float.Parse(radiusBox.Text);
						shape.Height = float.Parse(heightBox.Text);
					}
					else if (shape.Type == ShapeTypes.Sphere) {
						shape.Radius = float.Parse(radiusBox.Text);
					}
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/// <summary>
		/// Overwrites whatever's in the text boxes with whatever the block had before
		/// </summary>
		private void discardChangesButton_Click(object sender, RoutedEventArgs e) {
			List<object> emptyList = new List<object>();
			List<object> newList = new List<object>();
			newList.Add(sceneListBox.SelectedItem);
			sceneListBox_SelectionChanged(sender, new SelectionChangedEventArgs(e.RoutedEvent, emptyList, newList));
		}

		/// <summary>
		/// If we change the type, we have to enable/disable boxes and stick new data into them.
		/// We do NOT want to modify the shape object itself!
		/// </summary>
		private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0 || !AreWeDoneLoading)
				return;

			// the new type
			ShapeTypes newType = (ShapeTypes) typeComboBox.SelectedIndex;
			try {
				// x is either the x scale or the radius
				float x = dimensionsXBox.Text != string.Empty ? float.Parse(dimensionsXBox.Text) : float.Parse(radiusBox.Text);
				// y is either the y scale or the height, or if both of those don't exist, then the radius
				float y = dimensionsYBox.Text != string.Empty ? float.Parse(dimensionsYBox.Text) : (heightBox.Text != string.Empty ? float.Parse(heightBox.Text) : float.Parse(radiusBox.Text));
				// z is either the z scale or the radius
				float z = dimensionsZBox.Text != string.Empty ? float.Parse(dimensionsZBox.Text) : float.Parse(radiusBox.Text);

				// update our boxes
				if (newType == ShapeTypes.Box || newType == ShapeTypes.Cylinder) {
					dimensionsXBox.IsEnabled = true;
					dimensionsXBox.Text = x + string.Empty;
					dimensionsYBox.IsEnabled = true;
					dimensionsYBox.Text = y + string.Empty;
					dimensionsZBox.IsEnabled = true;
					dimensionsZBox.Text = z + string.Empty;
					radiusBox.IsEnabled = false;
					heightBox.IsEnabled = false;
				}
				else if (newType == ShapeTypes.Capsule || newType == ShapeTypes.Cone) {
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.IsEnabled = false;
					radiusBox.IsEnabled = true;
					radiusBox.Text = x + string.Empty;
					heightBox.IsEnabled = true;
					heightBox.Text = y + string.Empty;
				}
				else if (newType == ShapeTypes.Sphere) {
					dimensionsXBox.IsEnabled = false;
					dimensionsYBox.IsEnabled = false;
					dimensionsZBox.IsEnabled = false;
					heightBox.IsEnabled = false;
					radiusBox.IsEnabled = true;
					radiusBox.Text = x + string.Empty;
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void physicsBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;

			if ((string) item.Content == "None") {
				massBox.IsEnabled = false;
				massBox.Text = "0";
				collisionBox.SelectedIndex = 0; // none
				collisionBox.IsEnabled = false;
			}
			else if ((string) item.Content == "Static") {
				massBox.IsEnabled = false;
				massBox.Text = "0";
				collisionBox.IsEnabled = true;
				collisionBox.SelectedIndex = 2; // environment
			}
			else if ((string) item.Content == "Kinematic") {
				massBox.IsEnabled = false;
				massBox.Text = "0";
				collisionBox.IsEnabled = true;
				collisionBox.SelectedIndex = 3; // affectors
			}
			else { // dynamic
				massBox.IsEnabled = true;
				massBox.Text = "1";
				collisionBox.IsEnabled = true;
				collisionBox.SelectedIndex = 1; // default
			}
		}

		private void BillboardCheckBox_Checked(object sender, RoutedEventArgs e) {
			BillboardMaterialBox.IsEnabled = true;
			BillboardSizeBox.IsEnabled = true;
			BillboardTexCoordsBox.IsEnabled = true;
			BillboardHelpButton.IsEnabled = true;
			BillboardRecenterCheckBox.IsEnabled = true;
		}

		private void BillboardCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			BillboardMaterialBox.IsEnabled = false;
			BillboardSizeBox.IsEnabled = false;
			BillboardTexCoordsBox.IsEnabled = false;
			BillboardHelpButton.IsEnabled = false;
			BillboardRecenterCheckBox.IsEnabled = false;
		}

		private void BillboardHelpButton_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show(
@"This box can do a couple of things:
 1) Leave blank for all billboards to use the entire texture
 2) Just enter one set of texture coordinates if all billboards in the set use the same ones
 3) Enter several texture coordinate sets separated by vertical bars | to use several different billboards. The billboards will be assigned one set at random

Texture coordinate sets should be entered like this:
	 x, x, x, x|y, y, y, y|z, z, z, z", "Texture coordinate box help", MessageBoxButton.OK);
		}

		// from http://www.dotnetperls.com/shuffle
		public static string[] RandomizeStrings(string[] arr) {
			var _random = new Random();

			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			// Add all strings from array
			// Add new random int each time
			foreach (string s in arr) {
				list.Add(new KeyValuePair<int, string>(_random.Next(), s));
			}
			// Sort the list by the random number
			var sorted = from item in list
						 orderby item.Key
						 select item;
			// Allocate new string array
			string[] result = new string[arr.Length];
			// Copy values to array
			int index = 0;
			foreach (KeyValuePair<int, string> pair in sorted) {
				result[index] = pair.Value;
				index++;
			}
			// Return copied array
			return result;
		}
	}
}
