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
	public class InputState
	{
		#region Fields

		private const int MaxInputs = 4;
		
		public readonly KeyboardState[] m_CurrentKeyboardStates;
		public readonly KeyboardState[] m_LastKeyboardStates;

		public readonly GamePadState[] m_CurrentGamePadStates;
		public readonly GamePadState[] m_LastGamePadStates;

		public readonly bool[] m_bGamePadWasConnected;

		#endregion

		#region Initialization

		/// <summary>
		/// Constructs a new input state.
		/// </summary>
		public InputState()
		{
			m_CurrentKeyboardStates = new KeyboardState[MaxInputs];
			m_LastKeyboardStates = new KeyboardState[MaxInputs];

			m_CurrentGamePadStates = new GamePadState[MaxInputs];
			m_LastGamePadStates = new GamePadState[MaxInputs];

			m_bGamePadWasConnected = new bool[MaxInputs];
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Reads the latest state of the keyboard and gamepad.
		/// </summary>
		public void Update()
		{
			for (int i = 0; i < MaxInputs; i++)
			{
				m_LastKeyboardStates[i] = m_CurrentKeyboardStates[i];
				m_CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);

				m_LastGamePadStates[i] = m_CurrentGamePadStates[i];
				m_CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

				// Keep track of whether a gamepad has ever been connected, so we can detect if it is unplugged.
				if (m_CurrentGamePadStates[i].IsConnected)
				{
					m_bGamePadWasConnected[i] = true;
				}
			}
		}

		/// <summary>
		/// Helper for checking if a key was newly pressed during this update. The
		/// controllingPlayer parameter specifies which player to read input for.
		/// If this is null, it will accept input from any player. When a keypress
		/// is detected, the output playerIndex reports which player pressed it.
		/// </summary>
		public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				// Read input from the specified player.
				playerIndex = controllingPlayer.Value;

				int i = (int)playerIndex;

				return (m_CurrentKeyboardStates[i].IsKeyDown(key) &&
						m_LastKeyboardStates[i].IsKeyUp(key));
			}
			else
			{
				// Accept input from any player.
				return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
						IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
						IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
						IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
			}
		}

		/// <summary>
		/// Helper for checking if a button was newly pressed during this update.
		/// The controllingPlayer parameter specifies which player to read input for.
		/// If this is null, it will accept input from any player. When a button press
		/// is detected, the output playerIndex reports which player pressed it.
		/// </summary>
		public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				// Read input from the specified player.
				playerIndex = controllingPlayer.Value;

				int i = (int)playerIndex;

				return (m_CurrentGamePadStates[i].IsButtonDown(button) &&
						m_LastGamePadStates[i].IsButtonUp(button));
			}
			else
			{
				// Accept input from any player.
				return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
						IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
						IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
						IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
			}
		}

		/// <summary>
		/// Checks for a "menu select" input action.
		/// The controllingPlayer parameter specifies which player to read input for.
		/// If this is null, it will accept input from any player. When the action
		/// is detected, the output playerIndex reports which player pressed it.
		/// </summary>
		public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			return 
				IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
				IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
				IsNewKeyPress(Keys.X, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
		}

		/// <summary>
		/// Checks for a "menu cancel" input action.
		/// The controllingPlayer parameter specifies which player to read input for.
		/// If this is null, it will accept input from any player. When the action
		/// is detected, the output playerIndex reports which player pressed it.
		/// </summary>
		public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			return 
				IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
				IsNewKeyPress(Keys.V, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
		}

		/// <summary>
		/// Checks for a "menu up" input action.
		/// The controllingPlayer parameter specifies which player to read
		/// input for. If this is null, it will accept input from any player.
		/// </summary>
		public bool IsMenuUp(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;

			return 
				IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
		}

		/// <summary>
		/// Checks for a "menu down" input action.
		/// The controllingPlayer parameter specifies which player to read
		/// input for. If this is null, it will accept input from any player.
		/// </summary>
		public bool IsMenuDown(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;

			return 
				IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
		}

		/// <summary>
		/// Checks for a "menu left" input action.
		/// The controllingPlayer parameter specifies which player to read
		/// input for. If this is null, it will accept input from any player.
		/// </summary>
		public bool IsMenuLeft(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;

			return 
				IsNewKeyPress(Keys.Left, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.LeftThumbstickLeft, controllingPlayer, out playerIndex);
		}

		/// <summary>
		/// Checks for a "menu Right" input action.
		/// The controllingPlayer parameter specifies which player to read
		/// input for. If this is null, it will accept input from any player.
		/// </summary>
		public bool IsMenuRight(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;

			return 
				IsNewKeyPress(Keys.Right, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.LeftThumbstickRight, controllingPlayer, out playerIndex);
		}

		/// <summary>
		/// Checks for a "pause the game" input action.
		/// The controllingPlayer parameter specifies which player to read
		/// input for. If this is null, it will accept input from any player.
		/// </summary>
		public bool IsPauseGame(PlayerIndex? controllingPlayer)
		{
			PlayerIndex playerIndex;

			return 
				IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
		}

		#endregion
	}
}