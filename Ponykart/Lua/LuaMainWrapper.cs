using LuaNetInterface;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class LuaMainWrapper {

		public LuaMainWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("doFile", "Runs a lua script.", "string file - The filename. If it doesn't start with media/scripts/, it's added automagically.")]
		public void DoFile(string file) {
			LKernel.GetG<LuaMain>().DoFile(file);
		}

		
		[LuaFunction("quit", "Quits the lua VM.")]
		public void Quit() {
			LKernel.GetG<LuaMain>().Quit();
		}

		
		[LuaFunction("restart", "Shuts down the Lua VM and starts it again.")]
		public void Restart() {
			LKernel.GetG<LuaMain>().Restart();
		}

		
		[LuaFunction("print", "Prints something.", "string s - the string to print")]
		public void Print(string s) {
			LKernel.GetG<LuaMain>().Print(s);
		}

		[LuaFunction("helpcmd", "Show help for a given command or package", "string command - Package to get help of.")]
		public void GetCommandHelp(string command) {
			LKernel.GetG<LuaMain>().GetCommandHelp(command);
		}

		[LuaFunction("help", "List available commands.")]
		public void GetHelp() {
			LKernel.GetG<LuaMain>().GetHelp();
		}
	}
}
