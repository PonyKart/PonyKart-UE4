#include "pch.h"
#include "UI/LuaConsoleManager.h"

using namespace std;
using namespace Ponykart::UI;

/*
/// Adds a line of text to the console
/// @param text The text to add.
void LuaConsoleManager::addLabel(const string& text)
{
    /// TODO: BUG: Label is a Miyagi class ! Find a C++ equivalent
    Label* label = new Label {
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
*/
