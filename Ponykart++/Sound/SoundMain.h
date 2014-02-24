#ifndef SOUNDMAIN_H_INCLUDED
#define SOUNDMAIN_H_INCLUDED

#include <vector>
#include <deque>
#include <unordered_set>
#include <unordered_map>
#include <string>
#include <al.h>
#include <alc.h>
#include "Core/Pauser.h"
#include "Misc/alExtensions.h"


namespace Ponykart
{

namespace Actors{class SoundComponent;}
namespace Core{class CameraManager;}
namespace Players{class PlayerManager;}
namespace Levels{class LevelChangedEventArgs;}

namespace Sound
{

class SoundMain // TODO: Implement SoundMain properly
{
public:
	SoundMain(); // The sound manager class.
	~SoundMain();
	float getDefaultReferenceDistance ();
	float setDefaultReferenceDistance (float value);
	Extensions::ALSource play3D(std::string filename, const Ogre::Vector3& pos, bool looping, bool startPaused = false, bool sfx = false); // Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
	Extensions::ALSource play3D(Extensions::ALBuffer sound, const Ogre::Vector3& pos, bool looping, bool startPaused = false, bool sfx = false); // Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
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
	void everyTenth(void* o);
private:
	ALCcontext *context;
	float defaultReferenceDistance;
	std::vector<Extensions::ALBuffer> buffers;
	std::vector<Extensions::ALSource> musicSources;
	std::vector<Extensions::ALSource> idleSources;
	std::vector<Extensions::ALSource> activeSources;
	std::set<Actors::SoundComponent*> components;

	bool enableMusic;
	bool enableSounds;

	Core::CameraManager* cameraManager;
	Players::PlayerManager* playerManager;

	std::unordered_map<std::string, std::string> fileList;
	std::unordered_map<Extensions::ALBuffer, std::string> bufferNames;
};

} // Sound
} // Ponykart

#endif // SOUNDMAIN_H_INCLUDED
