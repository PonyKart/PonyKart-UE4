using Miyagi.Common.Data;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using MOIS;
using Ponykart.Lua;
using KeyEvent = MOIS.KeyEvent;

namespace Ponykart.UI {
	/// <summary>
	/// This class manages our lua console, since now we can type things into lua and make it do stuff, all without changing windows! :D
	/// Note that this console only outputs stuff from lua - it doesn't output anything else from the program.
	/// Check the log files if you want to look at *all* output.
	/// </summary>
	public class LuaConsoleManager {
		private int labelY;
		private Panel panel;
		private TextBox textBox;
		private GUI luaGui;
		private string lastInput = "";

		public LuaConsoleManager() {
			Launch.Log("[Loading] Creating LuaConsoleManager");
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress;
			Create();

			// swallow the input if our text box has focus
			LKernel.GetG<InputSwallowerManager>().AddSwallower(() => textBox.Focused, this);
		}

		/// <summary>
		/// Runs whenever we press the key to toggle whether to show or hide the console.
		/// </summary>
		void OnKeyboardPress(KeyEvent eventArgs) {
			if (eventArgs.key == KeyCode.KC_RETURN && !luaGui.Visible) {
				Show();
			}
			else if (eventArgs.key == KeyCode.KC_UP && luaGui.Visible && !LKernel.Get<InputSwallowerManager>().IsSwallowed(this)) {
				textBox.Text = lastInput;
			}
		}

		/// <summary>
		/// Hide the console!
		/// </summary>
		public void Hide() {
			luaGui.Visible = false;
			textBox.Text = string.Empty;
		}

		/// <summary>
		/// Show the console! Also this should automatically focus on the console too, but for some reason it's not doing that.
		/// </summary>
		public void Show() {
			luaGui.Visible = true;
			textBox.Text = string.Empty;

			textBox.Focused = true;
		}

		/// <summary>
		/// I don't really need to have this in a separate method, but eh it doesn't matter
		/// </summary>
		void Create() {
			luaGui = LKernel.GetG<UIMain>().GetGUI("lua console gui");

			// make the panel
			panel = luaGui.GetControl<Panel>("lua console panel");
			panel.UserData = new UIUserData {
				ObstructsViewport = true,
			};

			// make the text box
			textBox = luaGui.GetControl<TextBox>("lua console text box");
			textBox.UserData = new UIUserData {
				ObstructsViewport = true,
			};

			// eeeeeeeeveeeeeeeeeents
			textBox.Submit += TextBoxSubmit;
		}

		/// <summary>
		/// Adds a line of text to the console
		/// </summary>
		/// <param name="text">The text to add.</param>
		public void AddLabel(string text) {
			var label = new Label {
				Location = new Point(0, labelY),
				Text = text,
				AutoSize = true,
				MaxSize = new Size(panel.Size.Width, 300),
				TextStyle = {
					ForegroundColour = Colours.White,
					WordWrap = true,
					Multiline = true,
				}
			};
			panel.Controls.Add(label);
			labelY += label.Size.Height;
			// apparently this doesn't exist any more?
			// maybe this is what's causing it to crash when we add a scrollbar
			panel.ScrollToBottom();

			// only focus the text box if it's visible, otherwise it swallows input for no reason
			if (luaGui.Visible)
				textBox.Focused = true;
		}

		/// <summary>
		/// Runs whenever we press enter after typing something into the text box.
		/// A &gt; is put in front of any lines here, to distinquish between user input and lua output.
		/// Something similar is also done for the ogre log.
		/// </summary>
		private void TextBoxSubmit(object sender, ValueEventArgs<string> vea) {
			// ignore if nothing is entered
			if (vea.Data == string.Empty)
				return;

			lastInput = vea.Data;
			AddLabel("> " + vea.Data);
			Launch.Log("[Lua] <Input> " + vea.Data);
			LKernel.GetG<LuaMain>().DoString(vea.Data);

			textBox.Text = string.Empty;

			// yet it still loses focus?
			textBox.Focused = true;
		}

		public bool IsVisible {
			get {
				return luaGui.Visible;
			}
		}
	}
}
