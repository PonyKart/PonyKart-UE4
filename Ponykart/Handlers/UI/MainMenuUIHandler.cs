using System;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Networking;
using Ponykart.UI;

namespace Ponykart.Handlers
{
    public enum GameTypeEnum
    {
        None,
        SinglePlayer,
        NetworkedHost,
        NetworkedClient,
    }

    /// <summary>
    /// Handler to respond to some of the main menu UI events and change between the different menus
    /// </summary>
    [Handler(HandlerScope.Global)]
    public class MainMenuUIHandler
    {
        // just keeping this as a field since I'll be using it so much
        MainMenuManager mmm;

        public MainMenuUIHandler()
        {
            mmm = LKernel.GetG<MainMenuManager>();
            GameType = GameTypeEnum.None;

            LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);

            mmm.OnGameType_SelectSinglePlayer += new MainMenuButtonPressEvent(OnGameType_SelectSinglePlayer);
            mmm.OnGameType_SelectNetworkedHost += new MainMenuButtonPressEvent(OnGameType_SelectNetworkedHost);
            mmm.OnGameType_SelectNetworkedClient += new MainMenuButtonPressEvent(OnGameType_SelectNetworkedClient);
            mmm.OnGameType_SelectOptions += new MainMenuButtonPressEvent(OnGameType_SelectOptions);

            mmm.OnHostInfo_SelectBack += new MainMenuButtonPressEvent(OnHostInfo_SelectBack);
            mmm.OnHostInfo_SelectNext += new MainMenuButtonPressEvent(OnHostInfo_SelectNext);

            mmm.OnClientInfo_SelectBack += new MainMenuButtonPressEvent(OnClientInfo_SelectBack);
            mmm.OnClientInfo_SelectNext += new MainMenuButtonPressEvent(OnClientInfo_SelectNext);

            mmm.OnLevelSelect_SelectBack += new MainMenuButtonPressEvent(OnLevelSelect_SelectBack);
            mmm.OnLevelSelect += new MainMenuLevelSelectEvent(OnLevelSelect);

            mmm.OnLobby_SelectBack += new MainMenuButtonPressEvent(OnLobby_SelectBack);
            mmm.OnLobby_SelectNext += new MainMenuButtonPressEvent(OnLobby_SelectNext);

            mmm.OnCharacterSelect_SelectBack += new MainMenuButtonPressEvent(OnCharacterSelect_SelectBack);
            // no OnCharacterSelect because we don't need to do anything
            //mmm.OnCharacterSelect += new MainMenuCharacterSelectEvent(OnCharacterSelect);

            mmm.OnOptions_SelectOK += new MainMenuButtonPressEvent(OnOptions_SelectOK);
        }

        public GameTypeEnum GameType { get; private set; }

        /// <summary>
        /// character -> previous, skipping the lobby if needed
        /// </summary>
        private void OnCharacterSelect_SelectBack(Button button, MouseButtonEventArgs eventArgs)
        {
            switch (GameType)
            {
                case GameTypeEnum.SinglePlayer:
                    SwitchGui(mmm.CharacterSelectGui, mmm.LevelSelectGui);
                    break;
                // return to the level select instead of the lobby
                case GameTypeEnum.NetworkedHost:
                    SwitchGui(mmm.CharacterSelectGui, mmm.LevelSelectGui);
                    break;
                // return to the network info gui instead of the lobby
                case GameTypeEnum.NetworkedClient:
                    mmm.NetworkClientIPTextBox.Text = "";
                    mmm.NetworkClientPasswordTextBox.Text = "";
                    mmm.NetworkClientPortTextBox.Text = "";
                    SwitchGui(mmm.CharacterSelectGui, mmm.NetworkClientGui);
                    break;
                default:
                    throw new InvalidOperationException("OnCharacterSelect_SelectBack was invoked from an invalid GUI state - how did we get here?");
            }
        }

        /// <summary>
        /// network client -> game type
        /// </summary>
        private void OnClientInfo_SelectBack(Button button, MouseButtonEventArgs eventArgs)
        {
            GameType = GameTypeEnum.None;
            SwitchGui(mmm.NetworkClientGui, mmm.GameTypeGui);
        }

        /// <summary>
        /// network client -> lobby
        /// </summary>
        private void OnClientInfo_SelectNext(Button button, MouseButtonEventArgs eventArgs)
        {
            SwitchGui(mmm.NetworkClientGui, mmm.LobbyGui);
        }

        /// <summary>
        /// game type -> network client
        /// </summary>
        private void OnGameType_SelectNetworkedClient(Button button, MouseButtonEventArgs eventArgs)
        {
            GameType = GameTypeEnum.NetworkedClient;
            mmm.NetworkClientIPTextBox.Text = "";
            mmm.NetworkClientPasswordTextBox.Text = "";
            mmm.NetworkClientPortTextBox.Text = "";
            SwitchGui(mmm.GameTypeGui, mmm.NetworkClientGui);
        }

        /// <summary>
        /// game type -> network host
        /// </summary>
        private void OnGameType_SelectNetworkedHost(Button button, MouseButtonEventArgs eventArgs)
        {
            GameType = GameTypeEnum.NetworkedHost;
            mmm.NetworkHostPasswordTextBox.Text = "";
            mmm.NetworkHostPortTextBox.Text = "";
            SwitchGui(mmm.GameTypeGui, mmm.NetworkHostGui);
        }

        /// <summary>
        /// game type -> options
        /// </summary>
        private void OnGameType_SelectOptions(Button button, MouseButtonEventArgs eventArgs)
        {
            SwitchGui(mmm.GameTypeGui, mmm.OptionsGui);
        }

        /// <summary>
        /// game type -> level select
        /// </summary>
        private void OnGameType_SelectSinglePlayer(Button button, MouseButtonEventArgs eventArgs)
        {
            GameType = GameTypeEnum.SinglePlayer;
            SwitchGui(mmm.GameTypeGui, mmm.LevelSelectGui);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// network host -> game type
        /// </summary>
        private void OnHostInfo_SelectBack(Button button, MouseButtonEventArgs eventArgs)
        {
            GameType = GameTypeEnum.None;
            SwitchGui(mmm.NetworkHostGui, mmm.GameTypeGui);
        }

        /// <summary>
        /// network host -> level select
        /// </summary>
        private void OnHostInfo_SelectNext(Button button, MouseButtonEventArgs eventArgs)
        {
            SwitchGui(mmm.NetworkHostGui, mmm.LevelSelectGui);
        }

        private void OnLevelLoad(LevelChangedEventArgs eventArgs)
        {
            // don't need to unhide/hide any GUIs, since that's all done in the MainMenuManager
            if (eventArgs.NewLevel.Type == LevelType.Menu)
                GameType = GameTypeEnum.None;
        }

        /// <summary>
        /// level select -> next
        /// </summary>
        private void OnLevelSelect(Button button, MouseButtonEventArgs eventArgs, string levelSelection)
        {
            switch (GameType)
            {
                case GameTypeEnum.SinglePlayer:
                    SwitchGui(mmm.LevelSelectGui, mmm.CharacterSelectGui);
                    break;
                case GameTypeEnum.NetworkedHost:
                    SwitchGui(mmm.LevelSelectGui, mmm.LobbyGui);
                    break;
                default:
                    throw new InvalidOperationException("OnLevelSelect was invoked from an invalid GUI state - how did we get here?");
            }
        }

        /// <summary>
        /// level select -> previous
        /// </summary>
        private void OnLevelSelect_SelectBack(Button button, MouseButtonEventArgs eventArgs)
        {
            switch (GameType)
            {
                case GameTypeEnum.SinglePlayer:
                    GameType = GameTypeEnum.None;
                    SwitchGui(mmm.LevelSelectGui, mmm.GameTypeGui);
                    break;
                case GameTypeEnum.NetworkedHost:
                    mmm.NetworkHostPasswordTextBox.Text = "";
                    mmm.NetworkHostPortTextBox.Text = "";
                    LKernel.GetG<NetworkManager>().StopThread();
                    SwitchGui(mmm.LevelSelectGui, mmm.NetworkHostGui);
                    break;
                default:
                    throw new InvalidOperationException("OnLevelSelect_SelectBack was invoked from an invalid GUI state - how did we get here?");
            }
        }

        /// <summary>
        /// lobby -> previous
        /// </summary>
        private void OnLobby_SelectBack(Button button, MouseButtonEventArgs eventArgs)
        {
            switch (GameType)
            {
                case GameTypeEnum.NetworkedHost:
                    SwitchGui(mmm.LobbyGui, mmm.LevelSelectGui);
                    break;
                case GameTypeEnum.NetworkedClient:
                    mmm.NetworkClientIPTextBox.Text = "";
                    mmm.NetworkClientPasswordTextBox.Text = "";
                    mmm.NetworkClientPortTextBox.Text = "";
                    LKernel.GetG<NetworkManager>().StopThread();
                    SwitchGui(mmm.LobbyGui, mmm.NetworkClientGui);
                    break;
                default:
                    throw new InvalidOperationException("OnLobby_SelectBack was invoked from an invalid GUI state - how did we get here?");
            }
        }

        /// <summary>
        /// lobby -> character
        /// </summary>
        private void OnLobby_SelectNext(Button button, MouseButtonEventArgs eventArgs)
        {
            SwitchGui(mmm.LobbyGui, mmm.CharacterSelectGui);
        }

        /// <summary>
        /// options -> game type
        /// </summary>
        private void OnOptions_SelectOK(Button button, MouseButtonEventArgs eventArgs)
        {
            SwitchGui(mmm.OptionsGui, mmm.GameTypeGui);
        }

        /// <summary>
        /// Switches betweek two GUIs. In the future we could add some effects here if we wanted.
        /// </summary>
        /// <param name="oldGui">The previous gui you want to hide</param>
        /// <param name="newGui">The next gui you want to show</param>
        private void SwitchGui(GUI oldGui, GUI newGui)
        {
            oldGui.Visible = false;
            newGui.Visible = true;
        }
    }
}