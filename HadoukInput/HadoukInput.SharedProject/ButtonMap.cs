using Microsoft.Xna.Framework.Input;
using System;

namespace HadoukInput
{
	/// <summary>
	/// This class is used to map buttons.
	/// </summary>
	public class ButtonMap
	{
		public Buttons A { get; set; } = Buttons.A;
		public Buttons B { get; set; } = Buttons.B;
		public Buttons X { get; set; } = Buttons.X;
		public Buttons Y { get; set; } = Buttons.Y;
		public Buttons LeftShoulder { get; set; } = Buttons.LeftShoulder;
		public Buttons RightShoulder { get; set; } = Buttons.RightShoulder;
		public Buttons LeftTrigger { get; set; } = Buttons.LeftTrigger;
		public Buttons RightTrigger { get; set; } = Buttons.RightTrigger;

		public Buttons ActionMap(ControllerAction action)
		{
			switch (action)
			{
				case ControllerAction.A: return A;
				case ControllerAction.B: return B;
				case ControllerAction.X: return X;
				case ControllerAction.Y: return Y;
				case ControllerAction.LShoulder: return LeftShoulder;
				case ControllerAction.RShoulder: return RightShoulder;
				case ControllerAction.LTrigger: return LeftTrigger;
				case ControllerAction.RTrigger: return RightTrigger;
				default: throw new IndexOutOfRangeException();
			}
		}
	}
}
