#ifndef WHEELID_H_INCLUDED
#define WHEELID_H_INCLUDED

namespace Ponykart
{
namespace Actors
{
	// Lets us keep track of the ID numbers for wheels that the BulletSharp::Vehicle uses
    enum WheelID
	{
		FrontLeft = 0,
		FrontRight = 1,
		BackLeft = 2,
		BackRight = 3
	};
} // Actors
} // Ponykart

#endif // WHEELID_H_INCLUDED
