using System.Linq;
using Miyagi.Common.Events;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Networking;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This handler responds to level and character selection events from the main menu, holds on to them, then loads the appropriate level with the right character.
	/// 
	/// 
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class MainMenuMultiplayerHandler {
		// just keeping this as a field since I'll be using it so much
		MainMenuManager mmm;
		NetworkManager netMgr;
		string _levelSelection;
        Label LobbyLabel;
		public string LevelSelection {
			set {
				_levelSelection = value;
			}
		}
		string characterSelection;

		public MainMenuMultiplayerHandler() {
			mmm = LKernel.GetG<MainMenuManager>();
			netMgr = LKernel.GetG<NetworkManager>();

			mmm.OnLevelSelect += new MainMenuLevelSelectEvent(OnLevelSelect);
			mmm.OnCharacterSelect += new MainMenuCharacterSelectEvent(OnCharacterSelect);
			mmm.OnHostInfo_SelectNext += new MainMenuButtonPressEvent(OnHostInfo_SelectNext);
			mmm.OnClientInfo_SelectNext += new MainMenuButtonPressEvent(OnClientInfo_SelectNext);
            mmm.OnLobby_SelectNext += new MainMenuButtonPressEvent(OnLobbyForward);
            mmm.OnLobby_SelectBack += new MainMenuButtonPressEvent(OnLobbyBack); 
            mmm.OnLevelSelect_SelectBack += new MainMenuButtonPressEvent(OnLevelSelect_SelectBack);
            var LobbyGUI= LKernel.Get<UIMain>().GetGUI("menu lobby gui");
            LobbyLabel = LobbyGUI.GetControl<Label>("lobby label");
		}

		/// <summary>
		/// Since character selection at the moment is the final stage in the menus, this loads the new level based on the previous		
        /// level selection and current character selection
		/// </summary>
		void OnCharacterSelect(Button button, MouseButtonEventArgs eventArgs, string characterSelection) {
			//This will need to do other things at some point.
            //Like what, past Elision? You're so fucking helpful.
			this.characterSelection = characterSelection;
            if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedHost) {
                var localplayer = (from p in netMgr.Players where p.local select p).First();
                localplayer.SetSelection(characterSelection);
                string[] characters = new string[netMgr.Players.Count];
                foreach (NetworkEntity p in netMgr.Players) {
                    characters[p._GlobalID] = p.Selection ?? "Twilight Sparkle";
                }
                LevelChangeRequest request = new LevelChangeRequest() {
                    NewLevelName = _levelSelection,
                    CharacterNames = characters,
                    IsMultiplayer = true,
                };
                LKernel.GetG<LevelManager>().LoadLevel(request);
                netMgr.ForEachConnection(c => c.SendPacket(Commands.StartGame, _levelSelection, false));

            } else if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedClient) {
                try {
                    var MainPlayer = (from p in netMgr.Players where p.local select p).First();
                    netMgr.SingleConnection.SendPacket(Commands.RequestPlayerChange, MainPlayer.SerializeChange("Selection", characterSelection));
                } finally { }
            }
		}

		/// <summary>
		/// Called to initiate a host network thread
		/// </summary>
		void OnHostInfo_SelectNext(Button button, MouseButtonEventArgs eventArgs) {
			netMgr.InitManager(int.Parse(mmm.NetworkHostPortTextBox.Text),
							   mmm.NetworkHostPasswordTextBox.Text);
			netMgr.StartThread(100);
            LobbyLabel.Text = "You are now the host. Feel free to proceed through these menus at your leisure. Once you select a character, the round starts.\n";
		}

		/// <summary>
		/// Called to initiate a client network thread
		/// </summary>
		void OnClientInfo_SelectNext(Button button, MouseButtonEventArgs eventArgs) {

			netMgr.InitManager(int.Parse(mmm.NetworkClientPortTextBox.Text),
							   mmm.NetworkClientPasswordTextBox.Text,
                               mmm.NetworkClientIPTextBox.Text);
            netMgr.StartThread(100);

			netMgr.SingleConnection.SendPacket(Commands.Connect, mmm.NetworkClientPasswordTextBox.Text);
            LobbyLabel.Text = "You are now a client. Please wait to continue until the host has connected...\n";
		}
		/// <summary>
		/// Saves the chosen level for later
		/// </summary>
		void OnLevelSelect(Button button, MouseButtonEventArgs eventArgs, string levelSelection) {
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedHost) {
				this.LevelSelection = levelSelection;
				netMgr.ForEachConnection(c => c.SendPacket(Commands.SelectLevel, levelSelection));
			}
		}

        /// <summary>
        /// Attempts to make a new player
        /// </summary>
        /// <param name="button"></param>
        /// <param name="eventArgs"></param>
        void OnLobbyForward(Button button, MouseButtonEventArgs eventArgs) {
            if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedClient) {
                netMgr.SingleConnection.SendPacket(Commands.RequestPlayer);
            }

        }

        /// <summary>
        /// Attempts to make a new player
        /// </summary>
        /// <param name="button"></param>
        /// <param name="eventArgs"></param>
        void OnLobbyBack(Button button, MouseButtonEventArgs eventArgs) {
            if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedClient) {
                foreach (NetworkEntity ne in netMgr.Players) {
                    if (ne.local) {
                        netMgr.SingleConnection.SendPacket(Commands.LeaveGame, ne.GlobalID.ToString() );
                        netMgr.Players.Remove(ne);
                    }
                }
                netMgr.SingleConnection.CloseConnection();
                netMgr.StopThread();
            }
        }

        void OnLevelSelect_SelectBack(Button button, MouseButtonEventArgs eventArgs) {
            if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedHost) {
                netMgr.ForEachConnection((c) => c.CloseConnection());
                netMgr.StopThread();
            }
        }

		// why is this in here? :U 
		public void Start_Game() {
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedClient) {

                var characters = new string[netMgr.Players.Count];
                foreach (NetworkEntity p in netMgr.Players) {
                    characters[p._GlobalID] = p.Selection ?? "Twilight Sparkle";
                }

				LevelChangeRequest request = new LevelChangeRequest() {
					NewLevelName = _levelSelection,
					CharacterNames = characters,
                    IsMultiplayer = true,
				};
				LKernel.GetG<LevelManager>().LoadLevel(request);
			}
		}
	}
}