#ifndef ALEXTENSIONS_H_INCLUDED
#define ALEXTENSIONS_H_INCLUDED

#include <al.h>

namespace Extensions
{
	using ALSource = ALuint;
	using ALBuffer = ALuint;

	ALfloat alGetSourceGain (ALSource src);
}

#endif // ALEXTENSIONS_H_INCLUDED
