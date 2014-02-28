#include "pch.h"
#include "SoundSource.h"

using namespace std;
using namespace Ponykart::Sound;
using namespace Extensions;


SoundSource::SoundSource ()
	: source(-1), deleter([](ALSource src) { })
{
}

SoundSource::SoundSource (ALSource source, function<void (ALSource)> deleter)
	: source(source), deleter(deleter)
{
}

SoundSource::SoundSource (SoundSource &&other)
{
	*this = move(other);
}
SoundSource &SoundSource::operator= (SoundSource &&other)
{
	deleter(source);

	source = other.source;
	deleter = other.deleter;

	other.source = -1;
	other.deleter = [](ALSource src) { };

	return *this;
}


SoundSource::~SoundSource ()
{
	deleter(source);
}


SoundSource::operator ALSource () const
{
	return source;
}
