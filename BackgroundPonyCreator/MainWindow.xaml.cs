using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mogre;
using MogreInWpf;
//using MOIS;
using Color = System.Drawing.Color;
using Size = System.Windows.Size;
using Vector3 = Mogre.Vector3;

namespace BackgroundPonyCreator {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static bool AreWeDoneLoading = false;
		CultureInfo culture = CultureInfo.InvariantCulture;
		bool quit = false;

		public readonly bool InitMogreAsync = true;
		public static readonly Size StartupViewportSize = new Size(400, 300);
		public readonly bool AutoUpdateViewportSize = true;

		Camera camera;

		//InputManager InputManager;
		//Keyboard InputKeyboard;
		Root root;
		SceneManager sceneMgr;

		SceneNode ponyNode;
		Entity ponyEnt, wingsEnt, hornEnt, foldedWingsEnt, eyesEnt;
		Entity[] hairEnts, maneEnts, tailEnts;
		int activeHairstyle = 0;

		AnimationState blinkState;
		AnimationState animState;
		AnimationState wingsState;
		AnimationState[] maneStates, tailStates;
		string currentAnimation;

		const int NUMBER_OF_HAIRSTYLES = 8;

		ColourValue bodyColour = new ColourValue(0.707f, 0.688f, 0.902f),
			bodyAOColour = new ColourValue(0.445f, 0.465f, 0.758f),
			eyeColour1 = new ColourValue(0.387f, 0.254f, 0.387f),
			eyeColour2 = new ColourValue(0.820f, 0.609f, 0.707f),
			eyeHighlightColour = new ColourValue(0.981f, 0.871f, 0.871f),
			hairColour1 = new ColourValue(0.934f, 0.777f, 0.449f),
			hairAOColour1 = new ColourValue(0.805f, 0.543f, 0.16f),
			hairColour2 = new ColourValue(0.917f, 0.902f, 0.563f),
			hairAOColour2 = new ColourValue(0.844f, 0.641f, 0.246f);
		string cutieMarkTexture = "cloud_and_sun.png";

		MogreImage mogreImageSource;

		// ---------------------------------------

		public MainWindow() {
			AreWeDoneLoading = false;

			// create the window and everything
			InitializeComponent();

			this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

			// set the window's icon to our resource
			MemoryStream iconStream = new MemoryStream();
			Properties.Resources.Icon_1.Save(iconStream);
			iconStream.Seek(0, SeekOrigin.Begin);
			this.Icon = BitmapFrame.Create(iconStream);

			// okay, done initialising
			AreWeDoneLoading = true;
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			InitializeImage();

			if (InitMogreAsync) {
				mogreImageSource.InitOgreAsync(System.Threading.ThreadPriority.Normal, new RoutedEventHandler(OnOgreInitComplete));
			}
			else {
				mogreImageSource.InitOgreImage();

				OnOgreInitComplete(null, null);
			}
		}
		private void OnOgreInitComplete(object sender, RoutedEventArgs args) {
			if (AutoUpdateViewportSize) {
				MogreImage.SizeChanged += new SizeChangedEventHandler(MogreImage_SizeChanged);
			}
			mogreImageSource.IsDebugOverlayVisible = true;

			//-----------------------------------------------------------------------------

			TextureManager.Singleton.DefaultNumMipmaps = 6;

			root = MogreRootManager.GetSharedRoot();
			sceneMgr = mogreImageSource.SceneManager = root.CreateSceneManager(SceneType.ST_GENERIC, "sceneMgr");
			sceneMgr.AmbientLight = new ColourValue(0.8f, 0.8f, 0.8f);

			camera = sceneMgr.CreateCamera("cam");
			camera.Position = new Vector3(0.8f, 0.8f, 0.8f);
			camera.LookAt(new Vector3(-1, 1, -1));
			camera.SetAutoTracking(true, sceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(0, 0.4f, 0)));
			camera.NearClipDistance = 0.1f;
			camera.FarClipDistance = 2000;

			mogreImageSource.ViewportDefinitions = new ViewportDefinition[] { new ViewportDefinition(camera) };

			CreateThings();
			//SetupInput();


			mogreImageSource.Root.FrameStarted += FrameStarted;
		}
		private void InitializeImage() {
			mogreImageSource = new MogreImage();
			mogreImageSource.CreateDefaultScene = false;
			mogreImageSource.ViewportSize = PreferredMogreViewportSize;
			MogreImage.Source = mogreImageSource;
		}
		public Size PreferredMogreViewportSize {
			get {
				if (MogreImage.ActualHeight == 0 || MogreImage.ActualWidth == 0) {
					return StartupViewportSize;
				}
				return new Size(MogreImage.ActualWidth, MogreImage.ActualHeight);
			}
		}
		void MogreImage_SizeChanged(object sender, SizeChangedEventArgs e) {
			mogreImageSource.ViewportSize = PreferredMogreViewportSize;
		}

		/// <summary>
		/// create a bunch of crap
		/// </summary>
		void CreateThings() {

			Light light = mogreImageSource.SceneManager.CreateLight("sun2");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(-0.1f, -1, 0.1f);
			light.Direction.Normalise();
			light.DiffuseColour = new ColourValue(1f, 1f, 1f);
			light.SpecularColour = new ColourValue(1f, 1f, 1f);
			light.Position = new Vector3(0, 10, 0);
			light.CastShadows = true;


			ponyNode = sceneMgr.RootSceneNode.CreateChildSceneNode();
			ponyEnt = sceneMgr.CreateEntity("BgPonyColtBody.mesh");
			ponyNode.AttachObject(ponyEnt);
			ponyNode.Position = new Vector3(0, 0, 0);
			ponyEnt.SetMaterialName("BgPony");

			wingsEnt = sceneMgr.CreateEntity("BgPonyColtWings.mesh");
			wingsEnt.SetMaterialName("BgPonyWings");

			hornEnt = sceneMgr.CreateEntity("BgPonyHorn.mesh");
			hornEnt.SetMaterialName("BgPonyHorn");

			eyesEnt = sceneMgr.CreateEntity("BgPonyColtEyes.mesh");
			eyesEnt.SetMaterialName("BgPonyEyes");

			foldedWingsEnt = sceneMgr.CreateEntity("BgPonyColtWingsFolded.mesh");
			foldedWingsEnt.SetMaterialName("BgPonyWingsFolded");

			hairEnts = new Entity[NUMBER_OF_HAIRSTYLES];
			maneEnts = new Entity[NUMBER_OF_HAIRSTYLES];
			tailEnts = new Entity[NUMBER_OF_HAIRSTYLES];

			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				hairEnts[a] = sceneMgr.CreateEntity("BgPonyHair" + (a + 1) + ".mesh");
				hairEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));

				maneEnts[a] = sceneMgr.CreateEntity("BgPonyMane" + (a + 1) + ".mesh");
				maneEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));

				tailEnts[a] = sceneMgr.CreateEntity("BgPonyTail" + (a + 1) + ".mesh");
				tailEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
			}

			wingsEnt.Visible = false;
			foldedWingsEnt.Visible = false;
			hornEnt.Visible = false;

			for (int a = 1; a < NUMBER_OF_HAIRSTYLES; a++) {
				hairEnts[a].Visible = false;
				maneEnts[a].Visible = false;
				tailEnts[a].Visible = false;
			}

			AdjustBodyColour(bodyColour);
			AdjustBodyAOColour(bodyAOColour);
			AdjustHairColour1(hairColour1);
			AdjustHairColour2(hairColour2);
			AdjustHairAOColour1(hairAOColour1);
			AdjustHairAOColour2(hairAOColour2);

			// attach stuff

			ponyEnt.AttachObjectToBone("Eyes", eyesEnt);
			ponyEnt.AttachObjectToBone("Horn", hornEnt);
			ponyEnt.AttachObjectToBone("Wings", wingsEnt);
			ponyEnt.AttachObjectToBone("Wings", foldedWingsEnt);
			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				ponyEnt.AttachObjectToBone("Hair", hairEnts[a]);
				ponyEnt.AttachObjectToBone("Mane", maneEnts[a]);
				ponyEnt.AttachObjectToBone("Tail", tailEnts[a]);
			}

			// setup animations

			Skeleton skeleton = ponyEnt.Skeleton;
			skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;
			// set up the blink animation state with some stuff
			blinkState = ponyEnt.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1;

			blinkState.CreateBlendMask(skeleton.NumBones, 0f);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);

			animState = ponyEnt.GetAnimationState("Stand1");
			wingsState = wingsEnt.GetAnimationState("Flap1");
			maneStates = new AnimationState[NUMBER_OF_HAIRSTYLES];
			tailStates = new AnimationState[NUMBER_OF_HAIRSTYLES];
			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				maneStates[a] = maneEnts[a].GetAnimationState("Stand1");
				tailStates[a] = tailEnts[a].GetAnimationState("Stand1");
				maneStates[a].Enabled = tailStates[a].Enabled = true;
				maneStates[a].Loop = tailStates[a].Loop = true;
			}
			currentAnimation = "Stand1";

			animState.Enabled = wingsState.Enabled = true;
			animState.Loop = wingsState.Loop = true;
		}

		void PlayAnimation(string animName) {
			currentAnimation = animName;
			{
				AnimationState newState = ponyEnt.GetAnimationState(animName);

				animState.Enabled = false;
				newState.Enabled = true;
				newState.Loop = true;

				animState = newState;
			}
			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				{
					AnimationState newState;
					if (maneEnts[a].Skeleton.HasAnimation(animName))
						newState = maneEnts[a].GetAnimationState(animName);
					else
						newState = maneEnts[a].GetAnimationState("Stand1");

					maneStates[a].Enabled = false;
					newState.Enabled = true;
					newState.Loop = true;

					maneStates[a] = newState;
				}
				{
					AnimationState newState;
					if (tailEnts[a].Skeleton.HasAnimation(animName))
						newState = tailEnts[a].GetAnimationState(animName);
					else
						newState = tailEnts[a].GetAnimationState("Stand1");

					tailStates[a].Enabled = false;
					newState.Enabled = true;
					newState.Loop = true;

					tailStates[a] = newState;
				}
			}
		}

		#region Color adjusting methods
		void AdjustBodyColour(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
			var ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			mat = MaterialManager.Singleton.GetByName("BgPonyHorn");
			ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			mat = MaterialManager.Singleton.GetByName("BgPonyWings");
			ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			mat = MaterialManager.Singleton.GetByName("BgPonyWingsFolded");
			ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			ps.SetNamedConstant("BodyColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

			bodyColourButton.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustBodyAOColour(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
			var psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			var psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);
			

			mat = MaterialManager.Singleton.GetByName("BgPonyHorn");
			psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


			mat = MaterialManager.Singleton.GetByName("BgPonyWings");
			psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


			mat = MaterialManager.Singleton.GetByName("BgPonyWingsFolded");
			psAO = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
			psAO.SetNamedConstant("AOColour", colour);
			mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psAO);

			psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			psEdge.SetNamedConstant("OutlineColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);

			bodyShadingButton.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustEyeColour1(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyEyes");
			var ps = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			ps.SetNamedConstant("TopIrisColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);

			eyeColour1Button.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustEyeColour2(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyEyes");
			var ps = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			ps.SetNamedConstant("BottomIrisColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);

			eyeColour2Button.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustEyeHighlightColour(ColourValue colour) {
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyEyes");
			var ps = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
			ps.SetNamedConstant("HighlightColour", colour);
			mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);

			eyeHighlightButton.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustHairColour1(ColourValue colour) {
			for (int a = 1; a <= NUMBER_OF_HAIRSTYLES; a++) {

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Single_" + a);
				var ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				ps.SetNamedConstant("HairColour", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

				mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				ps.SetNamedConstant("HairColour1", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);
			}
			hairColour1Button.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustHairAOColour1(ColourValue colour) {
			for (int a = 1; a <= NUMBER_OF_HAIRSTYLES; a++) {

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Single_" + a);
				var psCol = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				psCol.SetNamedConstant("AOColour", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psCol);

				var psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
				psEdge.SetNamedConstant("OutlineColour", colour);
				mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);


				mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				psCol = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				psCol.SetNamedConstant("AOColour1", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psCol);
			}
			hairAOColour1Button.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustHairColour2(ColourValue colour) {
			for (int a = 1; a <= NUMBER_OF_HAIRSTYLES; a++) {

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				var ps = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				ps.SetNamedConstant("HairColour2", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);
			}
			hairColour2Button.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}

		void AdjustHairAOColour2(ColourValue colour) {
			for (int a = 1; a <= NUMBER_OF_HAIRSTYLES; a++) {

				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + a);
				var psCol = mat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
				psCol.SetNamedConstant("AOColour2", colour);
				mat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(psCol);

				var psEdge = mat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
				psEdge.SetNamedConstant("OutlineColour", colour);
				mat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(psEdge);
			}
			hairAOColour2Button.Background = new SolidColorBrush(colour.ToWindowsMediaColor());
		}
		#endregion


		readonly Quaternion rotQuat = new Quaternion(new Degree(0.5f), Vector3.UNIT_Y);
		bool FrameStarted(FrameEvent evt) {
			if (quit)
				return false;

			//InputKeyboard.Capture();

			// rotate our pone
			if (ponyNode != null) {
				ponyNode.Rotate(new Quaternion(new Degree(16.6666f * evt.timeSinceLastFrame), Vector3.UNIT_Y));

				animState.AddTime(evt.timeSinceLastFrame);
				blinkState.AddTime(evt.timeSinceLastFrame);
				wingsState.AddTime(evt.timeSinceLastFrame);
				for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
					maneStates[a].AddTime(evt.timeSinceLastFrame);
					tailStates[a].AddTime(evt.timeSinceLastFrame);
				}
			}

			return !quit;
		}

		protected override void OnClosed(EventArgs e) {
			quit = true;

			base.OnClosed(e);
		}


		#region click color changing buttons
		private void bodyColourButton_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = bodyColour.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				bodyColour = dialog.Color.ToMogre();
				AdjustBodyColour(bodyColour);
			}
		}

		private void bodyShadingButton_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = bodyAOColour.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				bodyAOColour = dialog.Color.ToMogre();
				AdjustBodyAOColour(bodyAOColour);
			}
		}

		private void eyeColour1Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = eyeColour1.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				eyeColour1 = dialog.Color.ToMogre();
				AdjustEyeColour1(eyeColour1);
			}
		}

		private void eyeColour2Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = eyeColour2.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				eyeColour2 = dialog.Color.ToMogre();
				AdjustEyeColour2(eyeColour2);
			}
		}

		private void eyeHighlightButton_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = eyeHighlightColour.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				eyeHighlightColour = dialog.Color.ToMogre();
				AdjustEyeHighlightColour(eyeHighlightColour);
			}
		}

		private void hairColour1Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairColour1.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairColour1 = dialog.Color.ToMogre();
				AdjustHairColour1(hairColour1);
			}
		}

		private void hairAOColour1Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairAOColour1.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairAOColour1 = dialog.Color.ToMogre();
				AdjustHairAOColour1(hairAOColour1);
			}
		}

		private void hairColour2Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairColour2.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairColour2 = dialog.Color.ToMogre();
				AdjustHairColour2(hairColour2);
			}
		}

		private void hairAOColour2Button_Click(object sender, RoutedEventArgs e) {
			ColorDialog dialog = new ColorDialog();
			dialog.Color = hairAOColour2.ToWindows();
			dialog.FullOpen = true;

			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				hairAOColour2 = dialog.Color.ToMogre();
				AdjustHairAOColour2(hairAOColour2);
			}
		}
		#endregion

		#region horn and wings visibility
		private void hornCheckBox_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hornEnt.Visible = true;
		}

		private void hornCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hornEnt.Visible = false;
		}

		private void wingsCheckBox_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			wingsEnt.Visible = true;
		}

		private void wingsCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			wingsEnt.Visible = false;
		}

		private void foldedWingsCheckBox_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			foldedWingsEnt.Visible = true;
		}

		private void foldedWingsCheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			foldedWingsEnt.Visible = false;
		}
		#endregion

		#region one or two hair colors radio buttons
		private void oneHairColourRadioButton_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hairColour2Button.IsEnabled = false;
			hairAOColour2Button.IsEnabled = false;

			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				hairEnts[a].SetMaterialName("BgPonyHair_Single_" + (a + 1));
				maneEnts[a].SetMaterialName("BgPonyHair_Single_" + (a + 1));
				tailEnts[a].SetMaterialName("BgPonyHair_Single_" + (a + 1));
			}
		}

		private void twoHairColoursRadioButton_Checked(object sender, RoutedEventArgs e) {
			if (!AreWeDoneLoading)
				return;

			hairColour2Button.IsEnabled = true;
			hairAOColour2Button.IsEnabled = true;

			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				hairEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
				maneEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
				tailEnts[a].SetMaterialName("BgPonyHair_Double_" + (a + 1));
			}
		}
		#endregion

		/// <summary>
		/// change the visibility of the different hairstyles
		/// </summary>
		private void hairStyleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0 || !AreWeDoneLoading)
				return;

			ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
			string text = (string) item.Content;

			int hairID = int.Parse("" + text[9]) - 1;

			for (int a = 0; a < NUMBER_OF_HAIRSTYLES; a++) {
				if (a == hairID) {
					hairEnts[a].Visible = true;
					maneEnts[a].Visible = true;
					tailEnts[a].Visible = true;
				}
				else {
					hairEnts[a].Visible = false;
					maneEnts[a].Visible = false;
					tailEnts[a].Visible = false;
				}
			}
			activeHairstyle = hairID;
		}

		/// <summary>
		/// Change the cutie mark
		/// </summary>
		private void cutieMarkButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "\u2588\u2588\u2588 MAKE SURE THE IMAGE IS IN THE SAME FOLDER AS THE OTHER CUTIE MARKS! \u2588\u2588\u2588";
			dialog.InitialDirectory = "\\materials\\cutiemarks\\";
			dialog.DefaultExt = ".png";
			dialog.Filter = "PNG Images|*.png";
			dialog.Multiselect = false;

			var result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK) {
				MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
				mat.GetTechnique(0).GetPass(1).GetTextureUnitState(1).SetTextureName(Path.GetFileName(dialog.FileName));

				cutieMarkTexture = Path.GetFileName(dialog.FileName);
			}
		}

		private void animationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0 || !AreWeDoneLoading)
				return;

			ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
			string text = (string) item.Content;

			PlayAnimation(text);
		}

		private void randomizeButton_Click(object sender, RoutedEventArgs e) {
			Random random = new Random();

			// colours
			ColourValue randomBody = MakeRandomColour(random);
			AdjustBodyColour(randomBody);
			AdjustBodyAOColour(randomBody * 0.7f);

			AdjustEyeColour1(MakeRandomColour(random));
			ColourValue randomEyes = MakeRandomColour(random);
			AdjustEyeColour2(randomEyes);
			AdjustEyeHighlightColour(ColourValue.White - (randomEyes * 0.2f));

			ColourValue hair1 = MakeRandomColour(random);
			ColourValue hair2 = MakeRandomColour(random);
			AdjustHairColour1(hair1);
			AdjustHairAOColour1(hair1 * 0.7f);
			AdjustHairColour2(hair2);
			AdjustHairAOColour2(hair2 * 0.7f);

			// hairstyles
			int hairID;
			do {
				hairID = random.Next(6);
			} while (hairID == 3);

			for (int a = 0; a < 6; a++) {
				if (a == 3) continue;

				if (a == hairID) {
					hairEnts[a].Visible = true;
					maneEnts[a].Visible = true;
					tailEnts[a].Visible = true;
				}
				else {
					hairEnts[a].Visible = false;
					maneEnts[a].Visible = false;
					tailEnts[a].Visible = false;
				}
			}
			hairStyleComboBox.SelectedItem = hairStyleComboBox.Items[hairID];
			activeHairstyle = hairID;

			// horn
			if (RandomBool(random)) {
				// unicorn
				if (RandomBool(random)) {
					hornEnt.Visible = true;
					wingsEnt.Visible = false;
					foldedWingsEnt.Visible = false;

					hornCheckBox.IsChecked = true;
					wingsCheckBox.IsChecked = false;
					foldedWingsCheckBox.IsChecked = false;
				}
				// earth pone
				else {
					hornEnt.Visible = false;
					wingsEnt.Visible = false;
					foldedWingsEnt.Visible = false;

					hornCheckBox.IsChecked = false;
					wingsCheckBox.IsChecked = false;
					foldedWingsCheckBox.IsChecked = false;
				}
			}
			// no horn
			else {
				// pegasus with spread wings
				if (RandomBool(random)) {
					hornEnt.Visible = false;
					wingsEnt.Visible = true;
					foldedWingsEnt.Visible = false;

					hornCheckBox.IsChecked = false;
					wingsCheckBox.IsChecked = true;
					foldedWingsCheckBox.IsChecked = false;
				}
				// pegasus with folded wings
				else {
					hornEnt.Visible = false;
					wingsEnt.Visible = false;
					foldedWingsEnt.Visible = true;

					hornCheckBox.IsChecked = false;
					wingsCheckBox.IsChecked = false;
					foldedWingsCheckBox.IsChecked = true;
				}
			}

			// hair colors
			if (RandomBool(random)) {
				oneHairColourRadioButton_Checked(null, null);
				oneHairColourRadioButton.IsChecked = true;
			}
			else {
				twoHairColoursRadioButton_Checked(null, null);
				twoHairColoursRadioButton.IsChecked = true;
			}

			// animation
			int randomAnim = random.Next(14);
			PlayAnimation((string) (animationComboBox.Items[randomAnim] as ComboBoxItem).Content);
			animationComboBox.SelectedItem = animationComboBox.Items[randomAnim];

			// random cutie mark
			string cm = GetRandomCutieMark(random);
			MaterialPtr mat = MaterialManager.Singleton.GetByName("BgPony");
			mat.GetTechnique(0).GetPass(1).GetTextureUnitState(1).SetTextureName(cm);
		}

		private ColourValue MakeRandomColour(Random random) {
			return new ColourValue((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
		}

		private bool RandomBool(Random random) {
			return random.Next(2) == 1;
		}

		private string GetRandomCutieMark(Random random) {
			string file = null;
			try {
				var di = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + "\\materials\\cutiemarks");
				var rgFiles = di.GetFiles("*.png");
				int fileCount = rgFiles.Count();
				if (fileCount > 0) {
					int x = random.Next( 0, fileCount );
					file = rgFiles.ElementAt(x).Name;
				}
			}
			catch {}
			return file;
		}

		private void exportButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Export background pony";
			dialog.DefaultExt = ".bgp";
			dialog.Filter = "Background pony file|*.bgp";

			var result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK) {
				string name = Path.GetFileNameWithoutExtension(dialog.FileName);

				using (var writer = File.Exists(dialog.FileName) ? File.AppendText(dialog.FileName) : File.CreateText(dialog.FileName)) {
					writer.WriteLine();
					writer.Write("\"" + name + "\" ");
					writer.Write(activeHairstyle + 1);

					if (hornCheckBox.IsChecked == true)
						writer.Write(" unicorn ");
					else if (foldedWingsCheckBox.IsChecked == true)
						writer.Write(" pegasus ");
					else if (wingsCheckBox.IsChecked == true)
						writer.Write(" flyingpegasus ");
					else
						writer.Write(" earth ");

					writer.Write(bodyColour.r + " " + bodyColour.g + " " + bodyColour.b + " ");
					writer.Write(bodyAOColour.r + " " + bodyAOColour.g + " " + bodyAOColour.b + " ");
					writer.Write(eyeColour1.r + " " + eyeColour1.g + " " + eyeColour1.b + " ");
					writer.Write(eyeColour2.r + " " + eyeColour2.g + " " + eyeColour2.b + " ");
					writer.Write(eyeHighlightColour.r + " " + eyeHighlightColour.g + " " + eyeHighlightColour.b + " ");
					writer.Write("\"" + cutieMarkTexture + "\" ");

					bool twoHairs = twoHairColoursRadioButton.IsChecked.Value;
					writer.Write(twoHairs.ToString().ToLower() + " ");
					if (twoHairs) {
						writer.Write(hairColour1.r + " " + hairColour1.g + " " + hairColour1.b + " ");
						writer.Write(hairAOColour1.r + " " + hairAOColour1.g + " " + hairAOColour1.b + " ");
						writer.Write(hairColour2.r + " " + hairColour2.g + " " + hairColour2.b + " ");
						writer.Write(hairAOColour2.r + " " + hairAOColour2.g + " " + hairAOColour2.b + " ");

						writer.Write(2);
					}
					else {
						writer.Write(hairColour1.r + " " + hairColour1.g + " " + hairColour1.b + " ");
						writer.Write(hairAOColour1.r + " " + hairAOColour1.g + " " + hairAOColour1.b + " ");
					}

					writer.Flush();
					writer.Close();
				}
			}
		}
	}

	public static class Extensions {
		public static ColourValue ToMogre(this Color col) {
			return new ColourValue(col.R / 255f, col.G / 255f, col.B / 255f);
		}

		public static Color ToWindows(this ColourValue col) {
			return Color.FromArgb((int) (col.r * 255), (int) (col.g * 255), (int) (col.b * 255));
		}

		public static System.Windows.Media.Color ToWindowsMediaColor(this ColourValue col) {
			return System.Windows.Media.Color.FromRgb((byte) (col.r * 255), (byte) (col.g * 255), (byte) (col.b * 255));
		}

		public static System.Windows.Media.Color ToWindowsMediaColor(this System.Drawing.Color col) {
			return System.Windows.Media.Color.FromRgb(col.R, col.G, col.B);
		}
	}
}
