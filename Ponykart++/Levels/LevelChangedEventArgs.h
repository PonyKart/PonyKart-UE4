#ifndef LEVELCHANGEDEVENTARGS_H_INCLUDED
#define LEVELCHANGEDEVENTARGS_H_INCLUDED

#include "Levels/Level.h"
#include "Levels/LevelChangeRequest.h"

namespace Ponykart
{
namespace Levels
{

class LevelChangedEventArgs
{
	// If you're reading this from OnLevelUnload or OnLevelPreUnload, keep in mind that the new level has not been
	// fully initialised yet, so properties such as Type and stuff will probably not be correct!.
public:
	LevelChangedEventArgs(const Level& NewLevel, const Level& OldLevel, const LevelChangeRequest& Request);
	const Level& newLevel;
	const Level& oldLevel;
	const LevelChangeRequest& request;
};

} // Levels
} // Ponykart

#endif // LEVELCHANGEDEVENTARGS_H_INCLUDED
