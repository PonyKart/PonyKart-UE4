#ifndef SOUNDSOURCE_H_INCLUDED
#define SOUNDSOURCE_H_INCLUDED

#include <functional>
#include "Misc/alExtensions.h"


namespace Ponykart
{
namespace Sound
{


class SoundSource
{
public:
	SoundSource ();
	~SoundSource ();
	SoundSource (const SoundSource &) = delete;
	SoundSource &operator= (const SoundSource &) = delete;
	SoundSource (SoundSource &&other);
	SoundSource &operator= (SoundSource &&other);
	operator Extensions::ALSource () const;

private:
	SoundSource (Extensions::ALSource source, std::function<void (Extensions::ALSource)> deleter);

	Extensions::ALSource source;
	std::function<void (Extensions::ALSource)> deleter;

	friend class SoundMain;
};


} // Ponykart
} // Sound
#endif // SOUNDSOURCE_H_INCLUDED
