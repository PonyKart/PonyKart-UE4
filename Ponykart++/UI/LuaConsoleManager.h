#ifndef LUACONSOLEMANAGER_H_INCLUDED
#define LUACONSOLEMANAGER_H_INCLUDED

#include <string>

namespace Ponykart
{
namespace UI
{
	/// This class manages our lua console.
	/** Now we can type things into lua and make it do stuff, all without changing windows! :D
		Note that this console only outputs stuff from lua - it doesn't output anything else from the program.
		Check the log files if you want to look at *all* output. **/
	class LuaConsoleManager // TODO: Implement LuaConsoleManager
	{
	public:
		void addLabel(const std::string& text);

    private:
		int labelY;
		Panel* panel;
		TextBox* textBox;
		GUI* luaGui;
		std::string lastInput = "";
	};
} // UI
} // Ponykart

#endif // LUACONSOLEMANAGER_H_INCLUDED
