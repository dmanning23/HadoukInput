using Microsoft.Xna.Framework.Input;
using System;

namespace HadoukInput
{
	public class KeyMap
	{
		public Keys Select { get; set; } = Keys.D5;
		public Keys Start { get; set; } = Keys.Enter;
		public Keys Up { get; set; } = Keys.Up;
		public Keys Down { get; set; } = Keys.Down;
		public Keys Right { get; set; } = Keys.Right;
		public Keys Left { get; set; } = Keys.Left;
		public Keys A { get; set; } = Keys.A;
		public Keys B { get; set; } = Keys.B;
		public Keys X { get; set; } = Keys.X;
		public Keys Y { get; set; } = Keys.Y;
		public Keys LeftShoulder { get; set; } = Keys.D;
		public Keys RightShoulder { get; set; } = Keys.F;
		public Keys LeftTrigger { get; set; } = Keys.C;
		public Keys RightTrigger { get; set; } = Keys.V;

		public KeyMap(int playerIndex)
		{
			switch (playerIndex)
			{
				case 0:
					{
						Select = Keys.D5;
						Start = Keys.D1;
						Up = Keys.Up;
						Down = Keys.Down;
						Right = Keys.Right;
						Left = Keys.Left;
						A = Keys.LeftControl;
						B = Keys.LeftAlt;
						X = Keys.Space;
						Y = Keys.LeftShift;
						LeftShoulder = Keys.Z;
						RightShoulder = Keys.X;
						LeftTrigger = Keys.C;
						RightTrigger = Keys.V;
					}
					break;
				case 1:
					{
						Select = Keys.D6;
						Start = Keys.D2;
						Up = Keys.R;
						Down = Keys.F;
						Right = Keys.G;
						Left = Keys.D;
						A = Keys.A;
						B = Keys.S;
						X = Keys.Q;
						Y = Keys.W;
						LeftShoulder = Keys.I;
						RightShoulder = Keys.K;
						LeftTrigger = Keys.J;
						RightTrigger = Keys.L;
					}
					break;
				case 2:
					{
						Select = Keys.D7;
						Start = Keys.D3;
						Up = Keys.I;
						Down = Keys.K;
						Right = Keys.L;
						Left = Keys.J;
						A = Keys.RightControl;
						B = Keys.RightShift;
						X = Keys.Enter;
						Y = Keys.O;
						LeftShoulder = Keys.Z;
						RightShoulder = Keys.X;
						LeftTrigger = Keys.C;
						RightTrigger = Keys.V;
					}
					break;
				case 3:
					{
						Select = Keys.D8;
						Start = Keys.D4;
						Up = Keys.Y;
						Down = Keys.N;
						Right = Keys.U;
						Left = Keys.V;
						A = Keys.B;
						B = Keys.E;
						X = Keys.H;
						Y = Keys.M;
						LeftShoulder = Keys.Z;
						RightShoulder = Keys.X;
						LeftTrigger = Keys.C;
						RightTrigger = Keys.V;
					}
					break;
			}
		}

		public Keys ActionMap(ControllerAction action)
		{
			switch (action)
			{
				case ControllerAction.Up: return Up;
				case ControllerAction.Down: return Down;
				case ControllerAction.Right: return Right;
				case ControllerAction.Left: return Left;
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