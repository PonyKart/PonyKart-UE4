#include "Physics/Materials/PhysicsMaterial.h"

using namespace Ponykart::Physics;

const float PhysicsMaterial::DEFAULT_FRICTION=0.3f;
const float PhysicsMaterial::DEFAULT_BOUNCINESS=0.3f;
const float PhysicsMaterial::DEFAULT_ANGULAR_DAMPING=0.3f;
const float PhysicsMaterial::DEFAULT_LINEAR_DAMPING=0.3f;

PhysicsMaterial::PhysicsMaterial(float Friction, float Bounciness,float AngularDamping, float LinearDamping)
 : friction(Friction), bounciness(Bounciness), angularDamping(AngularDamping), linearDamping(LinearDamping)
{
}
