#include <al.h>
#include "Actors/LThing.h"
#include "Actors/Components/SoundComponent.h"
#include "Kernel/LKernel.h"
#include "Sound/SoundMain.h"
#include "Thing/Blocks/SoundBlock.h"
#include "Thing/Blocks/ThingBlock.h"
#include "UI/LuaConsoleManager.h"

#if DEBUG
#include "Kernel/LKernelOgre.h"
#endif

using namespace std;
using namespace Ogre;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::LKernel;
using namespace Ponykart::Sound;
using namespace PonykartParsers;
using namespace Extensions;

/** @param lthing The Thing this component is attached to.
	@param block The block we're creating this component from */
SoundComponent::SoundComponent(LThing* lthing, ThingBlock* thingTemplate, SoundBlock* block)
{
	needUpdate = false;
	name = block->getStringProperty("name", thingTemplate->getThingName());
	owner = lthing;

	auto soundMain = LKernel::getG<SoundMain>();
	auto soundData = soundMain->loadSoundData(block->getStringProperty("File"));

	bool looping = block->getBoolProperty("looping", true);
	bool sfx = block->getBoolProperty("SpecialEffects", false);
	relativePosition = block->getVectorProperty("position", Vector3::ZERO);
	bool startPaused = block->getBoolProperty("StartPaused", true);

	sound = soundMain->play3D(soundData, relativePosition, looping, startPaused, sfx);

	alSourcef(sound, AL_PITCH, block->getFloatProperty("Speed", 1));
	auto volumeIt=block->getFloatTokens().find("volume");
	if (volumeIt!=block->getFloatTokens().end())
		alSourcef(sound, AL_GAIN, volumeIt->second);

	alSourcef(sound, AL_REFERENCE_DISTANCE, block->getFloatProperty("mindistance", soundMain->getDefaultReferenceDistance()));

	// TODO: effects, if we end up using any of them

	update();
	ALint state;
	alGetSourcei(sound, AL_SOURCE_STATE, &state);
	if (state == AL_PAUSED)
		alSourcePlay(sound);

	soundMain->addSoundComponent(this);
}

/// Update's the sound's position (and velocity if the owner has a Body).
/** Called from Ponykart::Physics::OgreMotionState. */
void SoundComponent::update()
{
	needUpdate = false;
	owner->setSoundsNeedUpdate(false);

	Vector3 parent = owner->getRootNode()->_getDerivedPosition();
	// update the position
	alSource3f(sound, AL_POSITION, parent.x + relativePosition.x, parent.y + relativePosition.y, parent.z + relativePosition.z);
	if (owner->getBody() != nullptr)
	{
		auto vel = owner->getBodyLinearVelocity();
		alSource3f(sound, AL_VELOCITY, vel.x, vel.y, vel.z);
	}

	if (onUpdate.size()) // run the OnUpdate methods
	{
#if DEBUG
		try
		{
#endif
		for (auto event : onUpdate)
			event(owner, sound);
#if DEBUG
		}
		catch (...)
		{
			log("[Lua] *** EXCEPTION *** caught in SoundComponent::update");
			//LKernel::getG<UI::LuaConsoleManager>()->addLabel("ERROR: Exception caught in SoundComponent::update"); // TODO: Implement LuaConsoleManger::addLabel, then uncomment
		}
#endif
	}
}

SoundSource &SoundComponent::getSound()
{
	return sound;
}
