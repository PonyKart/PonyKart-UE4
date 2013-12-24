#include "Actors/LThing.h"
#include "Actors/Components/SoundComponent.h"
#include "Kernel/LKernel.h"
#include "Misc/Extensions.h"
#include "Muffin/ThingBlock.h"
#include "Sound/SoundMain.h"
#include "Thing/SoundBlock.h"

using namespace std;
using namespace Ogre;
using namespace irrklang;
using namespace Ponykart;
using namespace Ponykart::Actors;
using namespace Ponykart::LKernel;
using namespace Ponykart::Sound;
using namespace PonykartParsers;

SoundComponent::SoundComponent(LThing* lthing, ThingBlock* thingTemplate, SoundBlock* block)
{
	needUpdate = false;
	name = block->getStringProperty("name", thingTemplate->getThingName());
	owner = lthing;

	auto soundMain = LKernel::getG<SoundMain>();
	ISoundSource* source = soundMain->getSource(block->getStringProperty("File"));

	bool looping = block->getBoolProperty("looping", true);
	bool sfx = block->getBoolProperty("SpecialEffects", false);
	relativePosition = block->getVectorProperty("position", Vector3::ZERO);
	bool startPaused = block->getBoolProperty("StartPaused", true);

	sound = soundMain->play3D(source, relativePosition, looping, startPaused, sfx);

	sound->setPlaybackSpeed(block->getFloatProperty("Speed", 1));
	auto volumeIt=block->getFloatTokens().find("volume");
	if (volumeIt!=block->getFloatTokens().end())
		sound->setVolume(volumeIt->second);

	sound->setMinDistance(block->getFloatProperty("mindistance", soundMain->getEngineDefault3DSoundMinDistance()));

	// TODO: effects, if we end up using any of them

	update();
	sound->setIsPaused(false);

	soundMain->addSoundComponent(this);
}

void SoundComponent::update()
{
	needUpdate = false;
	owner->setSoundsNeedUpdate(false);

	Vector3 parent = owner->getRootNode()->_getDerivedPosition();
	// update the position
	sound->setPosition(irrklang::vec3df(parent.x + relativePosition.x, parent.y + relativePosition.y, parent.z + relativePosition.z));
	if (owner->getBody() != nullptr)
		sound->setVelocity(toSoundVector(owner->getBodyLinearVelocity()));

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
		catch ()
		{
			log("[Lua] *** EXCEPTION *** caught in SoundComponent::update");
			LKernel::getG<LuaConsoleManager>()->addLabel("ERROR: Exception caught in SoundComponent::update");
		}
#endif
	}
}

irrklang::ISound* SoundComponent::getSound()
{
	return sound;
}
