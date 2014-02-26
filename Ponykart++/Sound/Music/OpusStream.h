#ifndef OPUSSTREAM_H_INCLUDED
#define OPUSSTREAM_H_INCLUDED

#include <opusfile.h>
#include "MusicStream.h"

#define DATA_LENGTH 11520


namespace Ponykart
{
namespace Sound
{


class OpusStream : public MusicStream
{
public:
	OpusStream (std::string filename);
	virtual ~OpusStream ();

	virtual int readSegment (Extensions::ALBuffer buf);
	virtual void reset ();

private:
	OggOpusFile *of;
	int channels;
	ogg_int64_t loopStart;
	float data[DATA_LENGTH];
};


} // Sound
} // Ponykart
#endif // OPUSSTREAM_H_INCLUDED
