#include "VorbisStream.h"

#include <string>
#include <cstring>
#include <algorithm>
#include <al.h>
#include "Kernel/LKernelOgre.h"

using namespace std;
using namespace Extensions;
using namespace Ponykart::Sound;
using namespace Ponykart::LKernel;


VorbisStream::VorbisStream (string filename)
	: MusicStream(filename)
{
	if (ov_fopen(filename.c_str(), &vf) < 0)
		throw string("Error openning vorbis file: " + filename);
	if (ov_streams(&vf) > 1)
		log("[Sound] WARNING: Vorbis file has more than 1 bitstream: " + filename);
	info = ov_info(&vf, -1);
	if (info->channels > 2) {
		ov_clear(&vf);
		throw string("Vorbis file has more than 2 channels: " + filename);
	}

	loopStart = -1;
	auto vc = ov_comment(&vf, -1);
	for (int i = 0; i < vc->comments; i++) {
		auto comment = string(vc->user_comments[i], vc->comment_lengths[i]);
		for (auto &c : comment)
			c = tolower(c);
		if (comment.compare(0, 10, "loopstart=") == 0) {
			try {
				loopStart = std::stol(comment.substr(10));
			} catch (...) {
				log("[Sound] WARNING: Vorbis file has invalid loopstart: " + filename);
			}
			break;
		}
	}
}


VorbisStream::~VorbisStream ()
{
	ov_clear(&vf);
}


int VorbisStream::readSegment (ALBuffer buf)
{
	int bitstream;
	auto result = ov_read(&vf, data, DATA_SIZE, false, 2, true, &bitstream);
	if (result < 0) {
		ov_clear(&vf);
		throw string("Error while decoding vorbis file: " + filename);
	}
	else if (result == 0) {
		if (loopStart >= 0) {
			ov_pcm_seek(&vf, loopStart);
			result = ov_read(&vf, data, DATA_SIZE, false, 2, true, &bitstream);
		} else
			return 0;
	}

	switch (info->channels) {
	case 1:
		alBufferData(buf, AL_FORMAT_MONO16, (ALvoid*)data, result, info->rate);
		break;
	case 2:
		alBufferData(buf, AL_FORMAT_STEREO16, (ALvoid*)data, result, info->rate);
		break;
	}

	return result >> (1 + (info->channels - 1));
}


void VorbisStream::reset ()
{
	ov_pcm_seek(&vf, 0);
}
