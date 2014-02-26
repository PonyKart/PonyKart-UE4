#include "OpusStream.h"

#include <al.h>
#include <alext.h>
#include "Kernel/LKernelOgre.h"


using namespace std;
using namespace Extensions;
using namespace Ponykart::Sound;
using namespace Ponykart::LKernel;


OpusStream::OpusStream (string filename)
	: MusicStream(filename)
{
	int err = 0;
	of = op_open_file(filename.c_str(), &err);
	if (err != 0)
		throw string("Error openning opus file: " + filename);
	if (op_link_count(of) > 1)
		log("[Sound] WARNING: Opus file has more than 1 link: " + filename);
	channels = op_channel_count(of, -1);
	if (channels > 2) {
		op_free(of);
		throw("Opus file has more than 2 channels: " + filename);
	}

	loopStart = -1;
	auto ot = op_tags(of, -1);
	for (int i = 0; i < ot->comments; i++) {
		auto comment = string(ot->user_comments[i], ot->comment_lengths[i]);
		for (auto &c : comment)
			c = tolower(c);
		if (comment.compare(0, 10, "loopstart=") == 0) {
			try {
				loopStart = std::stol(comment.substr(10));
			} catch (...) {
				log("[Sound] WARNING: Opus file has invalid loopstart: " + filename);
			}
			break;
		}
	}
}


OpusStream::~OpusStream ()
{
	op_free(of);
}


int OpusStream::readSegment (ALBuffer buf)
{
	int link;
	auto result = op_read_float(of, data, DATA_LENGTH, &link);
	if (result < 0) {
		op_free(of);
		throw string("Error while decoding opus file: " + filename);
	}
	else if (result == 0) {
		if (loopStart >= 0) {
			op_pcm_seek(of, loopStart);
			result = op_read_float(of, data, DATA_LENGTH, &link);
		}
		else
			return 0;
	}

	switch (channels) {
	case 1:
		alBufferData(buf, AL_FORMAT_MONO_FLOAT32, (ALvoid*)data, result * sizeof(float) * channels, 48000);
		break;
	case 2:
		alBufferData(buf, AL_FORMAT_STEREO_FLOAT32, (ALvoid*)data, result * sizeof(float) * channels, 48000);
		break;
	}

	return result;
}


void OpusStream::reset ()
{
	op_pcm_seek(of, 0);
}
