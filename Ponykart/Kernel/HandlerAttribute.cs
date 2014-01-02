using System;

namespace Ponykart {
	/// <summary>
	/// Attributes to go on handlers. Make sure LevelHandlers implement ILevelHandler!
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class HandlerAttribute : Attribute {
		public readonly HandlerScope Scope;
		public readonly LevelType LevelType;
		public readonly string[] LevelNames;

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on? Default is All</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType = LevelType.All) {
			Scope = scope;
			LevelType = levelType;
		}

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on?</param>
		/// <param name="levelName">Which level does it specifically run on?</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType, string levelName)
			: this(scope, levelType) {
			LevelNames = new string[] { levelName };
		}

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on?</param>
		/// <param name="levelName1">A level it runs on</param>
		/// <param name="levelName2">A level it runs on</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType, string levelName1, string levelName2)
			: this(scope, levelType) {
			LevelNames = new string[] { levelName1, levelName2 };
		}

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on?</param>
		/// <param name="levelName1">A level it runs on</param>
		/// <param name="levelName2">A level it runs on</param>
		/// <param name="levelName3">A level it runs on</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType, string levelName1, string levelName2, string levelName3)
			: this(scope, levelType) {
			LevelNames = new string[] { levelName1, levelName2, levelName3 };
		}

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on?</param>
		/// <param name="levelName1">A level it runs on</param>
		/// <param name="levelName2">A level it runs on</param>
		/// <param name="levelName3">A level it runs on</param>
		/// <param name="levelName4">A level it runs on</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType, string levelName1, string levelName2, string levelName3, string levelName4)
			: this(scope, levelType) {
			LevelNames = new string[] { levelName1, levelName2, levelName3, levelName4 };
		}

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on?</param>
		/// <param name="levelName1">A level it runs on</param>
		/// <param name="levelName2">A level it runs on</param>
		/// <param name="levelName3">A level it runs on</param>
		/// <param name="levelName4">A level it runs on</param>
		/// <param name="levelName5">A level it runs on</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType, string levelName1, string levelName2, string levelName3, string levelName4, string levelName5)
			: this(scope, levelType) {
			LevelNames = new string[] { levelName1, levelName2, levelName3, levelName4, levelName5 };
		}
	}

	/// <summary>
	/// Specifies whether the handler is a global or level handler
	/// </summary>
	public enum HandlerScope {
		/// <summary>
		/// Disposed on level unload, recreated on level load
		/// </summary>
		Level,
		/// <summary>
		/// Only created once in the whole lifetime of the game
		/// </summary>
		Global
	}
}
