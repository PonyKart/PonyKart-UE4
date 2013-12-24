#ifndef SOUNDCOMPONENT_H_INCLUDED
#define SOUNDCOMPONENT_H_INCLUDED

#include <functional>
#include <vector>
#include <string>
#include <Ogre.h>
#include <ik_ISound.h>

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

using SoundFrameEvent = std::vector<std::function<void (LThing*, irrklang::ISound*)>>;

class SoundComponent // TODO: Implement SoundComponent properly
{
public:
	SoundComponent(LThing* lthing, PonykartParsers::ThingBlock *thingTemplate, PonykartParsers::SoundBlock* block); // lthing:The Thing this component is attached to. block:The block we're creating this component from
	void update(); // Update's the sound's position (and velocity if the owner has a Body). Called from Ponykart::Physics::OgreMotionState.
	// Getters
	irrklang::ISound* getSound();
	std::string getName() const;

public:
	bool needUpdate;
	SoundFrameEvent onUpdate;
protected:
	irrklang::ISound* sound;
	std::string name;
private:
	Ogre::Vector3 relativePosition;
	LThing* owner;
};

} // Actors
} // Ponykart

#endif // SOUNDCOMPONENT_H_INCLUDED
