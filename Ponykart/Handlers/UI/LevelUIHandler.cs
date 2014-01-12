using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles making the UI for both the levels and the main menu (because the main menu is considered to be a level)
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LevelUIHandler {
		private Label commandsLabel;
		private GUI levelGui;

		public LevelUIHandler() {
			LevelManager.OnLevelLoad += OnLevelLoad;
			LevelManager.OnLevelPreUnload += OnLevelPreUnload;

			SetupLevelUI();
		}

		/// <summary>
		/// Make the level UI
		/// </summary>
		void SetupLevelUI() {
			this.levelGui = LKernel.GetG<UIMain>().GetGUI("level debug gui");

			// the button
			Button commandsButton = levelGui.GetControl<Button>("show/hide commands button");
			commandsButton.UserData = new UIUserData {
				ObstructsViewport = true,
			};
#if !DEBUG
			commandsButton.Visible = false;
#endif
			// subscribe to the events that change the current texture
			commandsButton.MouseDown += CommandsButton_MouseDown;

			// and the text that's toggled
			commandsLabel = levelGui.GetControl<Label>("commands label");
		}

		/// <summary>
		/// Decides which UI to show when a level is loaded
		/// </summary>
		private void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type.HasFlag(LevelType.Race)) {
				// when going from the menu to something else
				levelGui.Visible = true;
			}
		}

		/// <summary>
		/// Hide the UI when unloading a level
		/// </summary>
		private void OnLevelPreUnload(LevelChangedEventArgs eventArgs) {
			levelGui.Visible = false;
		}

		// ======================================================

		private void CommandsButton_MouseDown(object sender, MouseButtonEventArgs e) {
			if (((Button) sender).Text == "Show Commands") {
				((Button) sender).Text = "Hide Commands";
				commandsLabel.Visible = true;
			}
			else {
				((Button) sender).Text = "Show Commands";
				commandsLabel.Visible = false;
			}
		}
	}
}