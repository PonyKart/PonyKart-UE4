#include "pch.h"
#include <random>
#include "Core/IDs.h"

using namespace Ponykart;

unsigned IDs::counter = 0;
std::function<int (void)> IDs::randomIntGen = std::bind ( std::uniform_int_distribution<int>(), std::default_random_engine() );
std::function<float (void)> IDs::randomFloatGen = std::bind ( std::uniform_real_distribution<float>(), std::default_random_engine() );

unsigned IDs::incremental()
{
	return counter++;
}

int IDs::random()
{
	return randomIntGen();
}

float IDs::randomFloat()
{
	return randomFloatGen();
}
