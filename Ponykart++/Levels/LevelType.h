#ifndef LEVELTYPE_H_INCLUDED
#define LEVELTYPE_H_INCLUDED

namespace Ponykart
{
	/// Represents a "type" of level
	enum LevelType
	{
		All = -1, ///< Used by level handlers to say that the handler should be created on all levels
		EmptyLevel = 1, ///< Used for the "empty" level that's created when the game's first started.
		Menu = 2, ///< For menus and stuff. Race-specific stuff and players are not created here.
		Race = 4, ///< For races.
        Multi = 8 ///< Multiplayer Race
	};
} // Ponykart

#endif // LEVELTYPE_H_INCLUDED
