#include "Thing/ShapeBlock.h"

using namespace Ogre;
using namespace PonykartParsers;

ShapeBlock::ShapeBlock(ThingDefinition* Owner) : owner(Owner)
{
}

void ShapeBlock::finish()
{
	Quaternion quat;
	if (quatTokens.find("orientation") != quatTokens.end())
	{
		quat = quatTokens["orientation"];
		Vector3 rot = getVectorProperty("rotation", Vector3::ZERO);
		quat = globalEulerToQuat(Degree(rot.x), Degree(rot.y), Degree(rot.z));
	}

	Vector3 pos = getVectorProperty("position", Vector3::ZERO);

	transform.makeTransform(pos, Vector3::UNIT_SCALE, quat);
}

Quaternion ShapeBlock::globalEulerToQuat(Radian rotX, Radian rotY, Radian rotZ)
{
	Quaternion q1,q2,q3,q;
	q1.FromAngleAxis(rotX, Vector3::UNIT_X);
	q2.FromAngleAxis(rotY, Vector3::UNIT_Y);
	q3.FromAngleAxis(rotZ, Vector3::UNIT_Z);

	// global axes
	q = q3 * q2 * q1;
	return q;
}

const Ogre::Matrix4& ShapeBlock::getTransform()
{
	return transform;
}
