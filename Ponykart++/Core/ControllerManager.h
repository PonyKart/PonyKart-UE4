#ifndef CONTROLLERMANAGER_H_INCLUDED
#define CONTROLLERMANAGER_H_INCLUDED

namespace Ponykart
{
namespace Core
{
	enum ControllerButtons
	{
		A
		,B
		,X
		,Y
		,RB
		,LB
		,Start
		,Back
		,LeftThumbstick
		,RightThumbstick
	};

	enum ControllerAxis
	{
		LeftX
		,LeftY
		,RightX
		,RightY
		,LeftTrigger
		,RightTrigger
	};

	struct ControllerAxisArgument
	{
		ControllerAxis Axis;
		float Value;
		float Delta;

		/*public ControllerAxisArgument( ) : base( )
		{
		}*/
	};

	class ControllerManager
	{

	};
} // Core
} // Ponykart

#endif // CONTROLLERMANAGER_H_INCLUDED
