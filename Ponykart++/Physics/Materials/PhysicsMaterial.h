#ifndef PHYSICSMATERIAL_H_INCLUDED
#define PHYSICSMATERIAL_H_INCLUDED

namespace Ponykart
{
namespace Physics
{
	struct PhysicsMaterial
	{
	public:
		PhysicsMaterial(float Friction = DEFAULT_FRICTION, float Bounciness = DEFAULT_BOUNCINESS,
			float AngularDamping = DEFAULT_ANGULAR_DAMPING, float LinearDamping = DEFAULT_LINEAR_DAMPING);
	public:
		static const float DEFAULT_FRICTION;
		static const float DEFAULT_BOUNCINESS;
		static const float DEFAULT_ANGULAR_DAMPING;
		static const float DEFAULT_LINEAR_DAMPING;

		float friction;
		float bounciness;
		float angularDamping;
		float linearDamping;
	};
} // Physics
} // Ponykart

#endif // PHYSICSMATERIAL_H_INCLUDED
