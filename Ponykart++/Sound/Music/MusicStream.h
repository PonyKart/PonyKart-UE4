#ifndef MUSICSTREAM_H_INCLUDED
#define MUSICSTREAM_H_INCLUDED

#include <string>
#include "Misc/alExtensions.h"


namespace Ponykart
{
namespace Sound
{


class MusicStream
{
public:
	MusicStream (std::string filename) : filename(filename), finishedFlag(false) { }
	virtual ~MusicStream () { }

	bool finished () { return finishedFlag; }
	virtual int readSegment (Extensions::ALBuffer buf) = 0;
	virtual void reset () = 0;

protected:
	std::string filename;
	bool finishedFlag;
};


} // Sound
} // Ponykart
#endif // MUSICSTREAM_H_INCLUDED
