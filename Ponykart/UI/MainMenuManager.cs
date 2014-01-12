using System.Drawing;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Networking;

namespace Ponykart.UI {
	/// <summary>
	/// These are invoked when we press a button on the main menu. You shouldn't really do much beyond changing GUIs and storing information with these.
	/// </summary>
	public delegate void MainMenuButtonPressEvent(Button button, MouseButtonEventArgs eventArgs);
	/// <summary>
	/// These are invoked when we select a level from the main menu. Note that this does not mean we should actually start loading this level, 
	/// just that it's the level the player wants to go to when they're finished in the menu.
	/// </summary>
	public delegate void MainMenuLevelSelectEvent(Button button, MouseButtonEventArgs eventArgs, string levelSelection);
	/// <summary>
	/// These are invoked when we select a character from the main menu. Note that this does not mean we should actually start a level with this character,
	/// just that it's the character the player wants to use when they're finished in the menu.
	/// </summary>
	public delegate void MainMenuCharacterSelectEvent(Button button, MouseButtonEventArgs eventArgs, string characterSelection);

	/// <summary>
	/// This class handles all of the main menu GUI event-firing stuff.
	/// </summary>
	public class MainMenuManager {
		public readonly GUI MenuBackgroundGui, GameTypeGui, NetworkHostGui, NetworkClientGui, LevelSelectGui, CharacterSelectGui, OptionsGui, LobbyGui;
		public readonly TextBox NetworkHostPortTextBox, NetworkHostPasswordTextBox, NetworkClientPortTextBox, NetworkClientPasswordTextBox, NetworkClientIPTextBox;
		public readonly Label LobbyLabel;

		public event MainMenuButtonPressEvent OnGameType_SelectSinglePlayer, OnGameType_SelectNetworkedHost, OnGameType_SelectNetworkedClient, OnGameType_SelectOptions;
		public event MainMenuButtonPressEvent OnHostInfo_SelectNext, OnHostInfo_SelectBack;
		public event MainMenuButtonPressEvent OnClientInfo_SelectNext, OnClientInfo_SelectBack;
		public event MainMenuButtonPressEvent OnLobby_SelectNext, OnLobby_SelectBack;
		public event MainMenuButtonPressEvent OnLevelSelect_SelectBack;
		public event MainMenuLevelSelectEvent OnLevelSelect;
		public event MainMenuButtonPressEvent OnCharacterSelect_SelectBack;
		public event MainMenuCharacterSelectEvent OnCharacterSelect;
		public event MainMenuButtonPressEvent OnOptions_SelectOK;

		public MainMenuManager() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelPreUnload += new LevelEvent(OnLevelPreUnload);

			UIMain uiMain = LKernel.GetG<UIMain>();
			MenuBackgroundGui = uiMain.GetGUI("menu background gui");
			GameTypeGui = uiMain.GetGUI("menu game type gui");
			LevelSelectGui = uiMain.GetGUI("menu level select gui");
			NetworkHostGui = uiMain.GetGUI("menu host info gui");
			NetworkClientGui = uiMain.GetGUI("menu client info gui");
			LobbyGui = uiMain.GetGUI("menu lobby gui");
			CharacterSelectGui = uiMain.GetGUI("menu character select gui");
			OptionsGui = uiMain.GetGUI("menu options gui");

			// the checkers bit in the corner
			PictureBox checkersPicture = MenuBackgroundGui.GetControl<PictureBox>("checkers picture");
			checkersPicture.Bitmap = new Bitmap("media/gui/checkers.png");

			// set up events and stuff
			GameTypeGui.GetControl<Button>("game type single player button").MouseClick += (o, e) => Invoke(OnGameType_SelectSinglePlayer, o, e);
			GameTypeGui.GetControl<Button>("game type networked host button").MouseClick += (o, e) => Invoke(OnGameType_SelectNetworkedHost, o, e);
			GameTypeGui.GetControl<Button>("game type networked client button").MouseClick += (o, e) => Invoke(OnGameType_SelectNetworkedClient, o, e);
			GameTypeGui.GetControl<Button>("game type options button").MouseClick += (o, e) => Invoke(OnGameType_SelectOptions, o, e);
			GameTypeGui.GetControl<Button>("quit button").MouseClick += (o, e) => Launch.Quit = true;

			NetworkHostPortTextBox = NetworkHostGui.GetControl<TextBox>("host info port text box");
			NetworkHostPasswordTextBox = NetworkHostGui.GetControl<TextBox>("host info password text box");
            NetworkHostGui.GetControl<Button>("host info next button").MouseClick += (o, e) => Invoke(OnHostInfo_SelectNext, o, e);
			NetworkHostGui.GetControl<Button>("host info back button").MouseClick += (o, e) => Invoke(OnHostInfo_SelectBack, o, e);

			NetworkClientIPTextBox = NetworkClientGui.GetControl<TextBox>("client info IP text box");
			NetworkClientPortTextBox = NetworkClientGui.GetControl<TextBox>("client info port text box");
			NetworkClientPasswordTextBox = NetworkClientGui.GetControl<TextBox>("client info password text box");
            NetworkClientGui.GetControl<Button>("client info next button").MouseClick += (o, e) => Invoke(OnClientInfo_SelectNext, o, e);
            NetworkClientGui.GetControl<Button>("client info back button").MouseClick += (o, e) => Invoke(OnClientInfo_SelectBack, o, e);

			LobbyLabel = LobbyGui.GetControl<Label>("lobby label");
			LobbyGui.GetControl<Button>("lobby next button").MouseClick += (o, e) => Invoke(OnLobby_SelectNext, o, e);
			LobbyGui.GetControl<Button>("lobby back button").MouseClick += (o, e) => Invoke(OnLobby_SelectBack, o, e);

			LevelSelectGui.GetControl<Button>("level select flat button").MouseClick += (o, e) => Invoke(OnLevelSelect, o, e, "flat");
			LevelSelectGui.GetControl<Button>("level select testlevel button").MouseClick += (o, e) => Invoke(OnLevelSelect, o, e, "testlevel");
			LevelSelectGui.GetControl<Button>("level select SAA button").MouseClick += (o, e) => Invoke(OnLevelSelect, o, e, "SweetAppleAcres");
			LevelSelectGui.GetControl<Button>("level select WTW button").MouseClick += (o, e) => Invoke(OnLevelSelect, o, e, "WhitetailWoods");
            LevelSelectGui.GetControl<Button>("level select TestAI button").MouseClick += (o, e) => Invoke(OnLevelSelect, o, e, "TestAI");
            LevelSelectGui.GetControl<Button>("level select roulette button").MouseClick += (o, e) => Invoke(OnLevelSelect, o, e, "roulette");
			LevelSelectGui.GetControl<Button>("level select back button").MouseClick += (o, e) => Invoke(OnLevelSelect_SelectBack, o, e);

			CharacterSelectGui.GetControl<Button>("character select TS button").MouseClick += (o, e) => Invoke(OnCharacterSelect, o, e, "Twilight Sparkle");
			CharacterSelectGui.GetControl<Button>("character select RD button").MouseClick += (o, e) => Invoke(OnCharacterSelect, o, e, "Rainbow Dash");
			CharacterSelectGui.GetControl<Button>("character select AJ button").MouseClick += (o, e) => Invoke(OnCharacterSelect, o, e, "Applejack");
			CharacterSelectGui.GetControl<Button>("character select PP button").MouseClick += (o, e) => Invoke(OnCharacterSelect, o, e, "Pinkie Pie");
			CharacterSelectGui.GetControl<Button>("character select FS button").MouseClick += (o, e) => Invoke(OnCharacterSelect, o, e, "Fluttershy");
			CharacterSelectGui.GetControl<Button>("character select rarity button").MouseClick += (o, e) => Invoke(OnCharacterSelect, o, e, "Rarity");
			CharacterSelectGui.GetControl<Button>("character select back button").MouseClick += (o, e) => Invoke(OnCharacterSelect_SelectBack, o, e);

			OptionsGui.GetControl<Button>("options ok button").MouseClick += (o, e) => Invoke(OnOptions_SelectOK, o, e);
		}

		void OnLevelPreUnload(LevelChangedEventArgs eventArgs) {
			if (eventArgs.OldLevel.Type == LevelType.Menu) {
				MenuBackgroundGui.Visible = false;
				GameTypeGui.Visible = NetworkHostGui.Visible = NetworkClientGui.Visible = LevelSelectGui.Visible
					= CharacterSelectGui.Visible = OptionsGui.Visible = LobbyGui.Visible = false;
			}
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type == LevelType.Menu) {
				MenuBackgroundGui.Visible = true;
				GameTypeGui.Visible = true;
			}
		}

		/// <summary>
		/// helper
		/// </summary>
		void Invoke(MainMenuButtonPressEvent e, object sender, MouseButtonEventArgs args) {
			if (e != null && (sender as Button) != null)
				e.Invoke(sender as Button, args);
		}
		/// <summary>
		/// helper
		/// </summary>
		void Invoke(MainMenuLevelSelectEvent e, object sender, MouseButtonEventArgs args, string levelSelection) {
			if (e != null && (sender as Button) != null)
				e.Invoke(sender as Button, args, levelSelection);
		}
		/// <summary>
		/// helper
		/// </summary>
		void Invoke(MainMenuCharacterSelectEvent e, object sender, MouseButtonEventArgs args, string characterSelection) {
			if (e != null && (sender as Button) != null)
				e.Invoke(sender as Button, args, characterSelection);
		}
	}
}
