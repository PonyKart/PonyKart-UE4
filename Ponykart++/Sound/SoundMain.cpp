#include "ik_ESoundEngineOptions.h"
#include "ik_ESoundOutputDrivers.h"
#include "irrKlang.h"
#include "Actors/Components/SoundComponent.h"
#include "Core/Cameras/CameraManager.h"
#include "Core/Cameras/KnightyCamera.h"
#include "Core/Cameras/PlayerCamera.h"
#include "Core/Options.h"
#include "Core/Pauser.h"
#include "Misc/direntSearch.h"
#include "Misc/ogreExtensions.h"
#include "Misc/irrklangExtensions.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Players/Player.h"
#include "Players/PlayerManager.h"
#include "Sound/SoundMain.h"

using namespace std;
using namespace irrklang;
using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Players;
using namespace Ponykart::Sound;

SoundMain::SoundMain()
{
	log("[Loading] Creating IrrKlang and SoundMain...");

	enableMusic = Options::getBool("Music");
	enableSounds = Options::getBool("Sounds");

	playerManager = LKernel::getG<PlayerManager>();
	cameraManager = LKernel::getG<CameraManager>();

	playerManager->onPostPlayerCreation.push_back(bind(&SoundMain::onPostPlayerCreation,this));
	LevelManager::onLevelUnload.push_back(bind(&SoundMain::onLevelUnload, this, placeholders::_1));
	LevelManager::onLevelLoad.push_back(bind(&SoundMain::onLevelLoad, this, placeholders::_1));
	LKernel::getG<Pauser>()->pauseEvent.push_back(bind(&SoundMain::pauseEvent,this,placeholders::_1));

	E_SOUND_ENGINE_OPTIONS flags = (E_SOUND_ENGINE_OPTIONS) (ESEO_DEFAULT_OPTIONS | ESEO_MUTE_IF_NOT_FOCUSED | ESEO_MULTI_THREADED);
	engine = createIrrKlangDevice(E_SOUND_OUTPUT_DRIVER::ESOD_AUTO_DETECT, flags);
	engine->setDefault3DSoundMinDistance(50);

	log("[Loading] IrrKlang and SoundMain initialised!");
}

irrklang::ISoundSource* SoundMain::getSource(std::string filename)
{
	string _path=filename;
	auto filenamePos=filename.rfind("/");
	if (filenamePos!=filename.size())
		filename=string(filename,filenamePos);

	auto fullpathIt = fileList.find(_path);
	if (fullpathIt != fileList.end())
		return engine->getSoundSource(fullpathIt->second.c_str(), true);
	else
		throw string("SoundMain::getSource: "+_path + " was not found!");
}

float SoundMain::getEngineDefault3DSoundMinDistance()
{
	return engine->getDefault3DSoundMinDistance();
}

void SoundMain::addSoundComponent(SoundComponent* sc)
{
	components.insert(sc);
}

ISound* SoundMain::play3D(ISoundSource* source, const Vector3& pos, bool looping, bool startPaused, bool sfx)
{
	log(string("[Sounds] Creating 3D sound: ") + source->getName() + " Looping: " + (looping ? "true" : "false"));

	ISound* sound = engine->play3D(source, vec3df(pos.x, pos.y, pos.z), looping, startPaused, sfx);
	sounds.push_back(sound);

	if (!enableSounds)
	{
		sound->setIsPaused(true);
		sound->setVolume(0);
	}
	else if (startPaused)
		sound->setIsPaused(true);

	return sound;
}

void SoundMain::pauseEvent(PausingState state)
{
	engine->setAllSoundsPaused(state == PausingState::Pausing);
}

void SoundMain::onPostPlayerCreation()
{
	if (LKernel::getG<LevelManager>()->isPlayableLevel())
		LKernel::onEveryUnpausedTenthOfASecondEvent.push_back(bind(&SoundMain::everyTenth,this,placeholders::_1));
}

void SoundMain::onLevelLoad(LevelChangedEventArgs* eventArgs)
{
	auto& rgMan = ResourceGroupManager::getSingleton();
	for (string group : rgMan.getResourceGroups())
	{
		if (!rgMan.isResourceGroupInitialised(group) || group == "Bootstrap")
			continue;

		auto resourceLocations = *(rgMan.listResourceLocations(group));
		for (string loc : resourceLocations)
		{
			std::vector<string> soundfiles = direntSearch(loc, "*.ogg");
			std::vector<string> tmpVec = direntSearch(loc, "*.mp3");
			std::vector<string> tmpVec2 = direntSearch(loc, "*.wav");
			soundfiles.insert(end(soundfiles), begin(tmpVec), end(tmpVec));
			soundfiles.insert(end(soundfiles), begin(tmpVec2), end(tmpVec2));

			for (string file : soundfiles)
				fileList[getFilename(file)] = file;
		}
	}
}

void SoundMain::onLevelUnload(LevelChangedEventArgs* eventArgs)
{
	// Get the adress of the function we registered to the event
	typedef void(*fPtrType)(void*);
	auto fPtr = function<void(void*)>(bind(&SoundMain::everyTenth, this, placeholders::_1)).target<fPtrType>();

	// Remove all functions of the event matching this address. Hopefully exactly one.
	bool found = false; // TODO: This is experimental. It may not work, if it does, remove the bool and the throw.
	for (auto it = begin(onEveryUnpausedTenthOfASecondEvent); it != end(onEveryUnpausedTenthOfASecondEvent); it++)
		if ((*it).target<fPtrType>() == fPtr)
		{
			onEveryUnpausedTenthOfASecondEvent.erase(it);
			found = true;
			break;
		}
	if (!found)
		throw string("SoundMain::onLevelUnload: Couldn't unregister from event onEveryUnpausedTenthOfASecondEvent");

	engine->removeAllSoundSources();
	engine->setListenerPosition(vec3df(0, 0, 0), vec3df(0, 0, -1));
	musics.clear();
	sounds.clear();
	components.clear();
}

void SoundMain::everyTenth(void* o)
{
	if (playerManager->getMainPlayer() == nullptr)
	{
		engine->update();
		return;
	}
	const LCamera* cam = cameraManager->getCurrentCamera();
	const btRigidBody* body = playerManager->getMainPlayer()->getBody();

	// Enjoy your RTTI
	const PlayerCamera* PCam = dynamic_cast<const PlayerCamera*>(cam);
	const KnightyCamera* KCam = dynamic_cast<const KnightyCamera*>(cam);
	vec3df pos, rot, vel;
	if (PCam || KCam)
	{
		pos = toSoundVector(body->getCenterOfMassPosition());
		rot = toSoundVector(toOgreQuaternion(body->getOrientation()).yAxis());
		vel = toSoundVector(body->getLinearVelocity());
	}
	else
	{
		const Quaternion& derivedOrientation = cam->getCamera()->getDerivedOrientation();
		pos = toSoundVector(cam->getCamera()->getDerivedPosition());
		rot = toSoundVector(derivedOrientation.yAxis());
		vel = toSoundVector(body->getLinearVelocity());
	}

	engine->setListenerPosition(pos,rot,vel,vec3df(0, 1, 0)); // TODO: BUG:? Is everyone (irrKlang/ogre/..) using the same axes ?

	for (auto component : components)
	{
		if (component->needUpdate)
			component->update();
	}

	engine->update();
}
