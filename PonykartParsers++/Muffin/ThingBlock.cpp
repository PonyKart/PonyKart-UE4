#include "Muffin/ThingBlock.h"

using namespace std;
using namespace Ogre;
using namespace PonykartParsers;

ThingBlock::ThingBlock(string ThingName, MuffinDefinition* Owner)
{
	thingName = ThingName;
	owner = Owner;
}

ThingBlock::ThingBlock(string ThingName, Vector3 Position)
{
	thingName = ThingName;
	vectorTokens["position"] = Position;
}

ThingBlock::ThingBlock(string ThingName, Vector3 Position, Quaternion Orientation)
{
	thingName = ThingName;
	vectorTokens["position"] = Position;
	quatTokens["orientation"] = Orientation;
}

void ThingBlock::finish()
{
	position = vectorTokens["position"];
}

string ThingBlock::getThingName() const
{
	return thingName;
}
