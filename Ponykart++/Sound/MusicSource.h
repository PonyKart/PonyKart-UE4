#ifndef MUSICSOURCE_H_INCLUDED
#define MUSICSOURCE_H_INCLUDED

#include <string>
#include <vector>
#include <vorbisfile.h>
#include "Misc/alExtensions.h"


namespace Sound
{

class MusicSource
{
public:
	MusicSource (const std::string filename);
	~MusicSource ();

	Extensions::ALsource GetSource () const;

	void Pause ();
	void Play ();
	void Pump ();

private:
	Extensions::ALsource source;
	std::vector<Extensions::ALbuffer> buffers;
	OggVorbis_File file;
};


} // Sound

#endif // MUSICSOURCE_H_INCLUDED
