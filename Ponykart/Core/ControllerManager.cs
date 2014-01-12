using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MOIS;

namespace Ponykart.Core
{
	public class ControllerAxisArgument
	{
		public ControllerAxis Axis;
		public float Value;
		public float Delta;

		/*public ControllerAxisArgument( ) : base( )
		{
		}*/
	}

	public enum ControllerButtons
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
	}

	public enum ControllerAxis
	{
		LeftX
		,LeftY
		,RightX
		,RightY
		,LeftTrigger
		,RightTrigger
	}

	public class ControllerManager
	{
		
	}
}