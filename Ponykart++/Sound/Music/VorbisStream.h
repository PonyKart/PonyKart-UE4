#ifndef VORBISSTREAM_H_INCLUDED
#define VORBISSTREAM_H_INCLUDED

#include <vorbisfile.h>
#include "MusicStream.h"

#define DATA_SIZE 4096


namespace Ponykart
{
namespace Sound
{


class VorbisStream : public MusicStream
{
public:
	VorbisStream (std::string filename);
	virtual ~VorbisStream ();

	virtual int readSegment (Extensions::ALBuffer buf);
	virtual void reset ();

private:
	OggVorbis_File vf;
	vorbis_info *info;
	ogg_int64_t loopStart;
	char data[DATA_SIZE];
};


} // Sound
} // Ponykart
#endif // VORBISSTREAM_H_INCLUDED
