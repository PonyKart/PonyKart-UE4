using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BulletSharp;
using Microsoft.Win32;
using Mogre;

namespace OgreToBulletMesh {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		CultureInfo culture = CultureInfo.InvariantCulture;
		Root root;
		string meshFilename;
		UpdateProgressBarDelegate updatePbDelegate;

		public MainWindow() {
			AreWeDoneLoading = false;

			// create the window and everything
			InitializeComponent();

			// set the window's icon to our resource
			MemoryStream iconStream = new MemoryStream();
			Properties.Resources.Icon_1.Save(iconStream);
			iconStream.Seek(0, SeekOrigin.Begin);
			this.Icon = BitmapFrame.Create(iconStream);

			updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

			// okay, done initialising
			AreWeDoneLoading = true;

			this.Closed += new System.EventHandler(OnClosed);
		}

		void OnClosed(object sender, EventArgs e) {
			if (root != null)
				root.Shutdown();
		}

		/// <summary>
		/// Give it a mesh and it'll create a BulletSharp.TriangleMesh out of it
		/// 
		/// Copied and pasted from Ponykart.Physics.OgreToBulletMesh.cs
		/// </summary>
		/// <param name="mesh">The mesh you're converting</param>
		/// <returns>A bullet trimesh</returns>
		public TriangleMesh Convert(MeshPtr mesh, Vector3 pos, Quaternion orientation, Vector3 scale) {

			// get our two main objects
			TriangleMesh BulletMesh = new TriangleMesh(true, false);

			uint vertex_count = default(uint);
			Vector3[] vertices = default(Vector3[]);
			uint index_count = default(uint);
			uint[] indices = default(uint[]);

			updatePB(35);

			GetMeshInformation(mesh, ref vertex_count, ref vertices, ref index_count, ref indices, pos, orientation, scale);

			BulletMesh.PreallocateIndexes((int) index_count);
			BulletMesh.PreallocateVertices((int) vertex_count);
			//BulletMesh.WeldingThreshold = 0.1f;

			updatePB(65);

			for (int a = 0; a < index_count; a += 3) {
				BulletMesh.AddTriangle(vertices[indices[a]], vertices[indices[a + 1]], vertices[indices[a + 2]], true);
			}

			return BulletMesh;
		}

		public ConvexHullShape ConvertToHull(MeshPtr mesh, Vector3 pos, Quaternion orientation, Vector3 scale) {
			uint vertex_count = default(uint);
			Vector3[] vertices = default(Vector3[]);
			uint index_count = default(uint);
			uint[] indices = default(uint[]);

			GetMeshInformation(mesh, ref vertex_count, ref vertices, ref index_count, ref indices, pos, orientation, scale);

			ConvexHullShape hull = new ConvexHullShape(vertices.Distinct().ToArray());

			return hull;
		}

		/// <summary>
		/// Copied and pasted from Ponykart.Physics.OgreToBulletMesh.cs
		/// </summary>
		public unsafe void GetMeshInformation(MeshPtr mesh, ref uint vertex_count, ref Vector3[] vertices, ref uint index_count, ref uint[] indices,
			Vector3 position, Quaternion orientation, Vector3 scale)
		{
			bool added_shared = false;
			uint current_offset = 0;
			uint shared_offset = 0;
			uint next_offset = 0;
			uint index_offset = 0;

			vertex_count = index_count = 0;

			for (ushort i = 0; i < mesh.NumSubMeshes; ++i) {
				SubMesh submesh = mesh.GetSubMesh(i);
				if (submesh.useSharedVertices) {
					if (!added_shared) {
						vertex_count += mesh.sharedVertexData.vertexCount;
						added_shared = true;
					}
				}
				else {
					vertex_count += submesh.vertexData.vertexCount;
				}

				index_count += submesh.indexData.indexCount;
			}

			updatePB(40);

			vertices = new Vector3[vertex_count];
			indices = new uint[index_count];
			added_shared = false;

			for (ushort i = 0; i < mesh.NumSubMeshes; ++i) {
				SubMesh submesh = mesh.GetSubMesh(i);
				VertexData vertex_data = submesh.useSharedVertices ? mesh.sharedVertexData : submesh.vertexData;

				if (!submesh.useSharedVertices || (submesh.useSharedVertices && !added_shared)) {
					if (submesh.useSharedVertices) {
						added_shared = true;
						shared_offset = current_offset;
					}

					VertexElement posElem = vertex_data.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
					HardwareVertexBufferSharedPtr vbuf = vertex_data.vertexBufferBinding.GetBuffer(posElem.Source);

					byte* vertex = (byte*) vbuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
					float* pReal;

					for (int j = 0; j < vertex_data.vertexCount; ++j, vertex += vbuf.VertexSize) {
						posElem.BaseVertexPointerToElement(vertex, &pReal);
						Vector3 pt = new Vector3(pReal[0], pReal[1], pReal[2]);
						vertices[current_offset + j] = (orientation * (pt * scale)) + position;
					}
					vbuf.Unlock();
					vbuf.Dispose();
					next_offset += vertex_data.vertexCount;
				}

				updatePB(45);

				IndexData index_data = submesh.indexData;
				uint numTris = index_data.indexCount / 3;
				HardwareIndexBufferSharedPtr ibuf = index_data.indexBuffer;

				bool use32bitindexes = (ibuf.Type == HardwareIndexBuffer.IndexType.IT_32BIT);

				uint* pLong = (uint*) ibuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
				ushort* pShort = (ushort*) pLong;
				uint offset = submesh.useSharedVertices ? shared_offset : current_offset;

				updatePB(50);

				if (use32bitindexes) {
					for (int k = 0; k < index_data.indexCount; ++k) {
						indices[index_offset++] = pLong[k] + offset;
					}
				}
				else {
					for (int k = 0; k < index_data.indexCount; ++k) {
						indices[index_offset++] = (uint) pShort[k] + (uint) offset;
					}
				}
				ibuf.Unlock();
				ibuf.Dispose();
				current_offset = next_offset;

				updatePB(55);
			}
		}

		/// <summary>
		/// Browse for a .mesh file to convert
		/// </summary>
		private void browseMeshButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".mesh";
			dlg.Filter = "Ogre .mesh files|*.mesh";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				meshFilename = dlg.FileName;
				browseMeshLabel.Content = Path.GetFileName(meshFilename);

				convertButton.IsEnabled = true;
			}
		}

		/// <summary>
		/// have to do this if we want the progress bar to update while we're doing other stuff
		/// </summary>
		/// <param name="value"></param>
		void updatePB(double value) {
			Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
		}

		/// <summary>
		/// Converts the .mesh file to a .bullet file, opens a save dialog, and saves it
		/// </summary>
		private void convertButton_Click(object sender, RoutedEventArgs e) {
			updatePB(0);

			if (root == null) {
				root = new Root("plugins.cfg", "", "Ogre.log");
				updatePB(10);
				// need a render system for whatever reason, even though we aren't rendering anything
				var renderSystem = root.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
				root.RenderSystem = renderSystem;
				updatePB(15);

				root.Initialise(false);
				updatePB(20);
			}

			// add the .mesh file to the resource group
			ResourceGroupManager.Singleton.AddResourceLocation(Path.GetDirectoryName(meshFilename), "FileSystem");
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
			updatePB(25);
			// and finally we can load it
			MeshPtr ogremesh = MeshManager.Singleton.Load(meshFilename, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
			updatePB(30);

			Vector3 pos = new Vector3();
			pos.x = string.IsNullOrWhiteSpace(positionXTextBox.Text) ? 0 : float.Parse(positionXTextBox.Text, culture);
			pos.y = string.IsNullOrWhiteSpace(positionYTextBox.Text) ? 0 : float.Parse(positionYTextBox.Text, culture);
			pos.z = string.IsNullOrWhiteSpace(positionZTextBox.Text) ? 0 : float.Parse(positionZTextBox.Text, culture);

			Quaternion orient = new Quaternion();
			orient.x = string.IsNullOrWhiteSpace(orientationXTextBox.Text) ? 0 : float.Parse(orientationXTextBox.Text, culture);
			orient.y = string.IsNullOrWhiteSpace(orientationYTextBox.Text) ? 0 : float.Parse(orientationYTextBox.Text, culture);
			orient.z = string.IsNullOrWhiteSpace(orientationZTextBox.Text) ? 0 : float.Parse(orientationZTextBox.Text, culture);
			orient.w = string.IsNullOrWhiteSpace(orientationWTextBox.Text) ? 1 : float.Parse(orientationWTextBox.Text, culture);
			orient.Normalise();

			Vector3 scale = new Vector3();
			scale.x = string.IsNullOrWhiteSpace(scaleXTextBox.Text) ? 1 : float.Parse(scaleXTextBox.Text, culture);
			scale.y = string.IsNullOrWhiteSpace(scaleYTextBox.Text) ? 1 : float.Parse(scaleYTextBox.Text, culture);
			scale.z = string.IsNullOrWhiteSpace(scaleZTextBox.Text) ? 1 : float.Parse(scaleZTextBox.Text, culture);


			BulletSharp.DataStream stream;

			// convert it
			if (hullCheckBox.IsChecked == false) {
				// trimesh
				TriangleMesh trimesh = Convert(ogremesh, pos, orient, scale);

				updatePB(80);

				BvhTriangleMeshShape trimeshshape = new BvhTriangleMeshShape(trimesh, true, true);
				trimeshshape.BuildOptimizedBvh();

				updatePB(85);

				DefaultSerializer serializer = new DefaultSerializer();
				serializer.StartSerialization();
				trimeshshape.SerializeSingleShape(serializer);
				serializer.FinishSerialization();
				stream = serializer.LockBuffer();
			}
			else {
				// convex hull

				ConvexHullShape hull = ConvertToHull(ogremesh, pos, orient, scale);

				updatePB(85);

				DefaultSerializer serializer = new DefaultSerializer();
				serializer.StartSerialization();
				hull.SerializeSingleShape(serializer);
				serializer.FinishSerialization();
				stream = serializer.LockBuffer();
			}
			

			updatePB(90);

			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".bullet";
			dlg.Filter = "Bullet .bullet files|*.bullet";
			dlg.FileName = Path.GetFileNameWithoutExtension(meshFilename) + ".bullet";

			bool? result = dlg.ShowDialog();

			if (result == true) {
				updatePB(95);

				// export it
				using (var filestream = File.Create(dlg.FileName)) {
					stream.CopyTo(filestream);
					filestream.Close();
				}
				stream.Close();

				updatePB(100);

				MessageBox.Show("Export successful!", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}

			updatePB(0);
		}
	}

	public delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
}