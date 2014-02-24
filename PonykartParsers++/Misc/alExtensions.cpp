#include <al.h>
#include "alExtensions.h"


ALfloat Extensions::alGetSourceGain (ALSource src)
{
	ALfloat vol;
	alGetSourcef(src, AL_GAIN, &vol);
	return vol;
}
