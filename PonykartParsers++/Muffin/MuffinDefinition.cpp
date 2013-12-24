#include "Muffin/MuffinDefinition.h"

using namespace std;
using namespace PonykartParsers;

MuffinDefinition::MuffinDefinition(string Name)
{
	name = Name;
}

void MuffinDefinition::finish()
{
	for (ThingBlock tb : thingBlocks)
		tb.finish();
}
