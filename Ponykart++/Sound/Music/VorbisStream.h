#ifndef VORBISSTREAM_H_INCLUDED
#define VORBISSTREAM_H_INCLUDED

#include <array>
#include <vorbisfile.h>
#include "MusicStream.h"


namespace Ponykart
{
namespace Sound
{


class VorbisStream : public MusicStream
{
public:
	VorbisStream (const std::string &filename);
	virtual ~VorbisStream ();

	virtual int readPCMChunk (Extensions::ALBuffer buf) override;
	virtual void reset () override;

private:
	OggVorbis_File vf;
	vorbis_info *info;
	ogg_int64_t loopStart;
	std::array<char, 16384> data;
};


} // Sound
} // Ponykart
#endif // VORBISSTREAM_H_INCLUDED
