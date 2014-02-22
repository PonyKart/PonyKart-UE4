#ifndef SOUNDCOMPONENT_H_INCLUDED
#define SOUNDCOMPONENT_H_INCLUDED

#include <functional>
#include <vector>
#include <string>
#include <Ogre.h>
#include <al.h>
#include "Misc/alExtensions.h"

namespace PonykartParsers
{
	class ThingBlock;
	class SoundBlock;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{

class LThing;

using SoundFrameEvent = std::vector<std::function<void (LThing*, Extensions::ALsource)>>;

class SoundComponent // TODO: Implement SoundComponent properly
{
public:
	SoundComponent(LThing* lthing, PonykartParsers::ThingBlock *thingTemplate, PonykartParsers::SoundBlock* block);
	void update();
	// Getters
	Extensions::ALsource getSound();
	std::string getName() const;

public:
	bool needUpdate;
	SoundFrameEvent onUpdate;
protected:
	Extensions::ALsource sound;
	std::string name;
private:
	Ogre::Vector3 relativePosition;
	LThing* owner;
};

} // Actors
} // Ponykart

#endif // SOUNDCOMPONENT_H_INCLUDED
