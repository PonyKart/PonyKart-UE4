#ifndef LEVELCHANGEREQUEST_H_INCLUDED
#define LEVELCHANGEREQUEST_H_INCLUDED

#include <string>
#include <vector>

namespace Ponykart
{
namespace Levels
{
/// A little struct to hold data that needs to be passed from one level to another to create specific stuff
struct LevelChangeRequest
{
	std::string newLevelName;
	std::vector<std::string> characterNames;
	bool isMultiplayer;
	// add more stuff as needed here
};

} // Levels
} // Ponykart

#endif // LEVELCHANGEREQUEST_H_INCLUDED
