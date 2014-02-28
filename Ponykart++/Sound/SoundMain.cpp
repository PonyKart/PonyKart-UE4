#include "Sound/SoundMain.h"

#include <cctype>
#include <chrono>
#include <algorithm>
#include <alext.h>
#include <efx.h>
#include <sndfile.hh>
#include <vorbisfile.h>
#include <opusfile.h>
#include "Actors/Components/SoundComponent.h"
#include "Core/Cameras/CameraManager.h"
#include "Core/Cameras/KnightyCamera.h"
#include "Core/Cameras/PlayerCamera.h"
#include "Core/Options.h"
#include "Core/Pauser.h"
#include "Misc/direntSearch.h"
#include "Misc/ogreExtensions.h"
#include "Misc/alExtensions.h"
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Players/Player.h"
#include "Players/PlayerManager.h"
#include "Sound/Music/MusicSource.h"

using namespace std;
using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::Actors;
using namespace Ponykart::Core;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Players;
using namespace Ponykart::Sound;


SoundMain::SoundMain()
	: defaultReferenceDistance(1.f)
{
	log("[Loading] Creating OpenAL and SoundMain...");

	enableMusic = Options::getBool("Music");
	enableSounds = Options::getBool("Sounds");

	playerManager = LKernel::getG<PlayerManager>();
	cameraManager = LKernel::getG<CameraManager>();

	//playerManager->onPostPlayerCreation.push_back(bind(&SoundMain::onPostPlayerCreation,this));
	LevelManager::onLevelUnload.push_back(bind(&SoundMain::onLevelUnload, this, placeholders::_1));
	LevelManager::onLevelLoad.push_back(bind(&SoundMain::onLevelLoad, this, placeholders::_1));
	LKernel::getG<Pauser>()->pauseEvent.push_back(bind(&SoundMain::pauseEvent,this,placeholders::_1));

	device = alcOpenDevice(nullptr);
	context = alcCreateContext(device, nullptr);
	alcMakeContextCurrent(context);
	if (alGetError() != AL_NO_ERROR)
		throw string("Failed to initialize OpenAL!");

	alDistanceModel(AL_INVERSE_DISTANCE_CLAMPED);

	idleSoundSources.resize(32);
	alGenSources(idleSoundSources.size(), idleSoundSources.data());
	forgottenSoundSources.reserve(32);

	musicQuit = false;
	musicThread = thread([this]() {
		while (!musicQuit) {
			musicLock.lock();
			for (auto m : musicSources)
				m->pump();
			musicLock.unlock();

			this_thread::sleep_for(chrono::milliseconds(100));
		}
	});

	log("[Loading] OpenAL and SoundMain initialised.");
}


SoundMain::~SoundMain ()
{
	log("[Shutdown] OpenAL and SoundMain shutting down...");

	stopAllSources();

	musicQuit = true;
	musicThread.join();

	alDeleteSources(idleSoundSources.size(), idleSoundSources.data());

	alcMakeContextCurrent(nullptr);
	alcDestroyContext(context);
	alcCloseDevice(device);

	log("[Shutdown] OpenAL and SoundMain terminated.");
}


void SoundMain::stopAllSources ()
{
	for (auto src : activeSoundSources) {
		alSourceStop(src);
		alSourcei(src, AL_BUFFER, 0);
	}
	idleSoundSources.insert(idleSoundSources.end(), activeSoundSources.begin(), activeSoundSources.end());
	activeSoundSources.clear();
	forgottenSoundSources.clear();
	for (auto buf : buffers)
		alDeleteBuffers(1, &buf);
	buffers.clear();

	for (auto m : musicSources)
		m->destroy();

	lock_guard<mutex> musicGuard(musicLock);
	musicSources.clear();
}


float SoundMain::getDefaultReferenceDistance ()
{
	return defaultReferenceDistance;
}
float SoundMain::setDefaultReferenceDistance (float value)
{
	return defaultReferenceDistance = value;
}


ALBuffer SoundMain::loadSoundData (string filename)
{
	string path=filename;
	filename = getFilename(filename);

	auto fullpathIt = fileList.find(path);
	if (fullpathIt != fileList.end()) {
		auto ext = getFileExtension(fullpathIt->second);
		for (char &c : ext)
			c = tolower(c);

		ALBuffer buf;
		if (ext == ".wav")
			buf = loadWavData(fullpathIt->second);
		else if (ext == ".ogg")
			buf = loadVorbisData(fullpathIt->second);
		else if (ext == ".opus")
			buf = loadOpusData(fullpathIt->second);
		else
			throw string("Unsupported sound format: " + ext);

		bufferNames.insert(decltype(bufferNames)::value_type(buf, std::string(filename)));
	} else
		throw string("SoundMain::getSource: "+ path + " was not found!");
}


ALBuffer SoundMain::loadWavData(string filename)
{
	SF_INFO info;
	auto wav = sf_open(filename.c_str(), SFM_READ, &info);
	if (wav == nullptr)
		throw string("Error openning wav file: " + filename);
	if (info.channels > 2) {
		sf_close(wav);
		throw string("Wav file has more than 2 channels: " + filename);
	}

	auto data = new ALshort[info.channels * info.frames];
	sf_read_short(wav, data, info.frames * info.channels);
	sf_close(wav);

	ALBuffer buf = 0;
	alGenBuffers(1, &buf);
	switch (info.channels) {
	case 1:
		alBufferData(buf, AL_FORMAT_MONO16, data, sizeof(ALshort) * info.frames, info.samplerate);
		break;
	case 2:
		alBufferData(buf, AL_FORMAT_STEREO16, data, sizeof(ALshort) * info.frames * 2, info.samplerate);
		break;
	}
	delete[] data;

	return buf;
}


ALBuffer SoundMain::loadVorbisData(string filename)
{
	OggVorbis_File vf;
	if (ov_fopen(filename.c_str(), &vf) < 0)
		throw string("Error openning vorbis file: " + filename);
	if (ov_streams(&vf) > 1)
		log("[Sound] WARNING: Vorbis file has more than 1 bitstream: " + filename);
	auto info = ov_info(&vf, -1);
	if (info->channels > 2) {
		ov_clear(&vf);
		throw string("Vorbis file has more than 2 channels: " + filename);
	}

	ogg_int64_t pcmTotal = ov_pcm_total(&vf, -1);
	ogg_int64_t dataLength = sizeof(ALshort) * pcmTotal * info->channels;
	auto data = new char[dataLength];
	int bitstream;
	long offset = 0;
	while (offset < dataLength) {
		auto result = ov_read(&vf, data + offset, dataLength - offset, false, 2, true, &bitstream);
		if (result < 0) {
			ov_clear(&vf);
			delete[] data;
			throw string("Error while decoding vorbis file: " + filename);
		}
		offset += result;
	}

	ALBuffer buf = 0;
	alGenBuffers(1, &buf);

	switch (info->channels) {
	case 1:
		alBufferData(buf, AL_FORMAT_MONO16, (ALvoid*)data, sizeof(ALshort) * pcmTotal, info->rate);
		break;
	case 2:
		alBufferData(buf, AL_FORMAT_STEREO16, (ALvoid*)data, sizeof(ALshort) * pcmTotal * 2, info->rate);
		break;
	}

	delete[] data;
	ov_clear(&vf);

	return buf;
}


ALBuffer SoundMain::loadOpusData(string filename)
{
	OggOpusFile *of;
	int err = 0;
	of = op_open_file(filename.c_str(), &err);
	if (err != 0)
		throw string("Error openning opus file: " + filename);
	if (op_link_count(of) > 1)
		log("[Sound] WARNING: Opus file has more than 1 link: " + filename);
	int channels = op_channel_count(of, -1);
	if (channels > 2) {
		op_free(of);
		throw("Opus file has more than 2 channels: " + filename);
	}

	ogg_int64_t pcmTotal = op_pcm_total(of, -1);
	ogg_int64_t dataLength = pcmTotal * channels;
	auto data = new float[dataLength];
	int link;
	long offset = 0;

	while (offset < dataLength) {
		auto result = op_read_float(of, data + offset, dataLength - offset, &link);
		if (result < 0) {
			op_free(of);
			delete[] data;
			throw string("Error while decoding opus file: " + filename);
		}
		offset += result * channels;
	}

	ALBuffer buf = 0;
	alGenBuffers(1, &buf);

	switch (channels) {
	case 1:
		alBufferData(buf, AL_FORMAT_MONO_FLOAT32, (ALvoid*)data, sizeof(ALfloat) * pcmTotal, 48000);
		break;
	case 2:
		alBufferData(buf, AL_FORMAT_STEREO_FLOAT32, (ALvoid*)data, sizeof(ALfloat) * pcmTotal * 2, 48000);
		break;
	}

	delete[] data;
	op_free(of);

	return buf;
}


void SoundMain::addSoundComponent(SoundComponent* sc)
{
	components.insert(sc);
}


ALSource SoundMain::activateSource ()
{
	ALSource src = 0;

	if (idleSoundSources.size() > 0) {
		src = idleSoundSources[idleSoundSources.size() - 1];
		idleSoundSources.pop_back();
	} else
		alGenSources(1, &src);

	activeSoundSources.insert(src);
	return src;
}


SoundSource SoundMain::play3D(ALBuffer sound, const Vector3& pos, bool looping, bool startPaused, bool efx)
{
#if DEBUG
	log(string("[Sound] Creating 3D sound: ") + bufferNames[sound] + " Looping: " + (looping ? "true" : "false"));
#endif

	auto src = activateSource();
	alSourcei(src, AL_BUFFER, sound);
	alSource3f(src, AL_POSITION, pos.x, pos.y, pos.z);
	alSourcei(src, AL_LOOPING, true);
	if (startPaused)
		alSourcePause(src);
	else
		alSourcePlay(src);

	// TODO: Look into EFX support.

	return SoundSource(src, [this](ALSource src) {
		int loop;
		alGetSourcei(src, AL_LOOPING, &loop);
		if (loop) {
			alSourceStop(src);
			activeSoundSources.erase(src);
			idleSoundSources.push_back(src);
		} else
			forgottenSoundSources.push_back(src);
	});
}


SoundMain::MusicSourcePtr SoundMain::PlayMusic (const string filename, bool startPaused, bool efx)
{
	auto musicSrc = new MusicSource(filename, startPaused);

	lock_guard<mutex> musicGuard(musicLock);
	musicSources.insert(musicSrc);

	return MusicSourcePtr(musicSrc, [this](MusicSource *ms) {
		lock_guard<mutex> musicGuard(musicLock);
		musicSources.erase(ms);
		delete ms;
	});
}


void SoundMain::pauseEvent(PausingState state)
{
	for (auto src : activeSoundSources)
		alSourcePause(src);
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
			std::vector<string> tmpVec = direntSearch(loc, "*.opus");
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
	// Get the address of the function we registered to the event
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

	stopAllSources();
	alListener3f(AL_POSITION, 0, 0, 0);
	alListener3f(AL_DIRECTION, 0, 0, -1);
	alListener3f(AL_VELOCITY, 0, 0, 0);

	components.clear();
}


void SoundMain::everyTenth (void *p)
{
	for (auto it = forgottenSoundSources.begin(); it != forgottenSoundSources.end();) {
		ALint state = 0;
		auto src = *it;
		alGetSourcei(src, AL_SOURCE_STATE, &state);
		if (state == AL_STOPPED)
		{
			alSourcei(src, AL_BUFFER, 0);
			idleSoundSources.push_back(src);
			activeSoundSources.erase(src);
			it = forgottenSoundSources.erase(it);
		}
		else
			it++;
	}

// TODO: Fix PlayerManager
//	if (playerManager->getMainPlayer() == nullptr)
//		return;

//	const LCamera* cam = cameraManager->getCurrentCamera();
//	const btRigidBody* body = playerManager->getMainPlayer()->getBody();

//	// Enjoy your RTTI
//	const PlayerCamera* PCam = dynamic_cast<const PlayerCamera*>(cam);
//	const KnightyCamera* KCam = dynamic_cast<const KnightyCamera*>(cam);
//	Vector3 pos, rot;
//	btVector3 vel;
//	if (PCam || KCam)
//	{
//		pos = toOgreVector3(body->getCenterOfMassPosition());
//		rot = toOgreQuaternion(body->getOrientation()).yAxis();
//		vel = body->getLinearVelocity();
//	}
//	else
//	{
//		const Quaternion& derivedOrientation = cam->getCamera()->getDerivedOrientation();
//		pos = cam->getCamera()->getDerivedPosition();
//		rot = derivedOrientation.yAxis();
//		vel = body->getLinearVelocity();
//	}

//	// TODO: BUG:? Is everyone using the same axes ?
//	alListener3f(AL_POSITION, pos.x, pos.y, pos.z);
//	alListener3f(AL_DIRECTION, rot.x, rot.y, rot.z);
//	alListener3f(AL_VELOCITY, vel.x(), vel.y(), vel.z());

	for (auto component : components) 
	{
		if (component->needUpdate)
			component->update();
	}
}
