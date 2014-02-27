#ifndef SOUNDMAIN_H_INCLUDED
#define SOUNDMAIN_H_INCLUDED

#include <memory>
#include <thread>
#include <atomic>
#include <mutex>
#include <vector>
#include <unordered_set>
#include <unordered_map>
#include <string>
#include <al.h>
#include <alc.h>
#include <Ogre.h>
#include "Core/Pauser.h"
#include "SoundSource.h"
#include "Sound/Music/MusicSource.h"
#include "Kernel/LKernelObject.h"
#include "Misc/alExtensions.h"


namespace Ponykart
{

namespace Actors{class SoundComponent;}
namespace Core{class CameraManager;}
namespace Players{class PlayerManager;}
namespace Levels{class LevelChangedEventArgs;}

namespace Sound
{
class SoundMain : public LKernel::LKernelObject // TODO: Implement SoundMain properly
{
public:
	using MusicSourcePtr = std::unique_ptr<MusicSource, std::function<void (MusicSource*)>>;

	SoundMain(); // The sound manager class.
	~SoundMain();
	float getDefaultReferenceDistance ();
	float setDefaultReferenceDistance (float value);
	SoundSource play3D(const std::string filename, const Ogre::Vector3& pos, bool looping, bool startPaused = false, bool efx = false); // Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
	SoundSource play3D(Extensions::ALBuffer sound, const Ogre::Vector3& pos, bool looping, bool startPaused = false, bool efx = false); // Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
	MusicSourcePtr PlayMusic (const std::string filename, bool startPaused = false, bool efx = false);
	void addSoundComponent(Actors::SoundComponent* sc);
	// Getters
	ALCcontext *getContext() const;
	Extensions::ALBuffer loadSoundData(std::string filename); // Gets a sound source. The engine keeps track of all of these. Don't include the "media/sound/" bit.

private:
	Extensions::ALBuffer loadWavData (std::string filename);
	Extensions::ALBuffer loadVorbisData (std::string filename);
	Extensions::ALBuffer loadOpusData (std::string filename);
	Extensions::ALSource activateSource ();
	void pauseEvent(Core::PausingState state);
	void onPostPlayerCreation(); // Manually called from the LevelManager
	void onLevelLoad(Levels::LevelChangedEventArgs* eventArgs); // prepares file locations
	void onLevelUnload(Levels::LevelChangedEventArgs* eventArgs); // Delete all of the sound sources
	void stopAllSources ();
	void everyTenth(void* o);

	ALCdevice *device;
	ALCcontext *context;
	float defaultReferenceDistance;
	std::unordered_set<Extensions::ALBuffer> buffers;
	std::unordered_set<MusicSource*> musicSources;
	std::vector<Extensions::ALSource> idleSoundSources;
	std::unordered_set<Extensions::ALSource> activeSoundSources;
	std::vector<Extensions::ALSource> forgottenSoundSources;
	std::set<Actors::SoundComponent*> components;

	bool enableMusic;
	bool enableSounds;

	Core::CameraManager* cameraManager;
	Players::PlayerManager* playerManager;

	std::unordered_map<std::string, std::string> fileList;
	std::unordered_map<Extensions::ALBuffer, std::string> bufferNames;

	std::thread musicThread;
	std::mutex musicLock;
	std::atomic_bool musicQuit;
};

} // Sound
} // Ponykart

#endif // SOUNDMAIN_H_INCLUDED
