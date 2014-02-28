#ifndef OPUSSTREAM_H_INCLUDED
#define OPUSSTREAM_H_INCLUDED

#include <array>
#include <opusfile.h>
#include "MusicStream.h"


namespace Ponykart
{
namespace Sound
{


class OpusStream : public MusicStream
{
public:
	OpusStream (const std::string &filename);
	virtual ~OpusStream ();

	virtual int readSegment (Extensions::ALBuffer buf);
	virtual void reset ();

private:
	OggOpusFile *of;
	int channels;
	ogg_int64_t loopStart;
	std::array<float, 11520> data;
};


} // Sound
} // Ponykart
#endif // OPUSSTREAM_H_INCLUDED
