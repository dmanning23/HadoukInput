using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HadoukInput
{
	/// <summary>
	/// Helper for reading input from keyboard and gamepad. This class tracks both
	/// the current and previous state of both input devices, and implements query
	/// methods for high level input actions such as "move up through the menu"
	/// or "pause the game".
	/// </summary>
	public interface IInputState
	{
		bool IsConnected(int controllerIndex);

		bool CheckControllers { get; set; }

		float DeadZone { get; }
		float DeadZoneSquared { get; }

		Vector2 LeftThumbstick(int controllerIndex);
		Vector2 RightThumbstick(int controllerIndex);

		KeyboardState CurrentKeyboardState { get; }

		KeyboardState LastKeyboardState { get; }

		bool DPadUp(int controllerIndex);
		bool DPadDown(int controllerIndex);
		bool DPadLeft(int controllerIndex);
		bool DPadRight(int controllerIndex);

		void Update();

		bool ButtonDown(int playerIndex, Buttons button);

		bool PrevButtonDown(int playerIndex, Buttons button);
	}
}