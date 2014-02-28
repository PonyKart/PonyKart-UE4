#ifndef MUSICSOURCE_H_INCLUDED
#define MUSICSOURCE_H_INCLUDED

#include <atomic>
#include <mutex>
#include <string>
#include <array>
#include "MusicStream.h"
#include "Misc/alExtensions.h"


namespace Ponykart
{
namespace Sound
{
class SoundMain;


class MusicSource
{
public:
	MusicSource (const std::string &filename, bool startPaused = false);
	MusicSource (const MusicSource &) = delete;
	MusicSource &operator= (const MusicSource &) = delete;

	Extensions::ALSource GetSource () const;

	void pause ();
	bool play ();
	void stop ();
	bool finished ();
	void destroy ();

private:
	~MusicSource ();

	void clear ();
	void fill ();
	void pump ();

	std::mutex pumpLock;
	Extensions::ALSource source;
	std::array<Extensions::ALBuffer, 4> buffers;
	int bufCursor;
	MusicStream *stream;

	friend class SoundMain;
};


} // Sound
} // Ponykart
#endif // MUSICSOURCE_H_INCLUDED
