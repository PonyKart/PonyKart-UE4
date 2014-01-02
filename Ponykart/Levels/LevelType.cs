using System;

namespace Ponykart {

	/// <summary>
	/// Represents a "type" of level
	/// </summary>
	[Flags]
	public enum LevelType {
		/// <summary>
		/// Used by level handlers to say that the handler should be created on all levels
		/// </summary>
		All = -1,
		/// <summary>
		/// Used for the "empty" level that's created when the game's first started.
		/// </summary>
		EmptyLevel = 1,
		/// <summary>
		/// For menus and stuff. Race-specific stuff and players are not created here.
		/// </summary>
		Menu = 2,
		/// <summary>
		/// For races.
		/// </summary>
		Race = 4,
        /// <summary>
        /// Multiplayer Race
        /// </summary>
        Multi = 8


	}
}
