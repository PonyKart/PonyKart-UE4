#ifndef WHEELFACTORY_H_INCLUDED
#define WHEELFACTORY_H_INCLUDED

#include <unordered_map>
#include <vector>
#include <string>
#include <Ogre.h>
#include "Actors/Kart.h"
#include "Actors/Wheels/Wheel.h"

namespace Ponykart
{
namespace Actors
{
// A class to read .wheel files and stick them into a wheel object
class WheelFactory
{
public:
	WheelFactory();
	void readWheelsFromFiles(); // Go through our media/wheels/ directory and find all of the wheel definitions we have, then make maps out of them and add them to our one big map
	Wheel* createWheel(std::string wheelName, WheelID ID, Kart* owner, Ogre::Vector3 position, std::string meshName); // // The name of the wheel type you want to create should be the same as the filename, minus the extension. Case sensitive.
private:
	std::unordered_map<std::string, std::unordered_map<std::string, float>> wheels; // our list of wheels
};
} // Actors
} // Ponykart

#endif // WHEELFACTORY_H_INCLUDED
