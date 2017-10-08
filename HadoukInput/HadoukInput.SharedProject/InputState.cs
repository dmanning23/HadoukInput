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

		/// <summary>
		/// The trigger dead zone.
		/// </summary>
		private const float TriggerDeadZone = 0.25f;

		public readonly GamePadState[] _currentGamePadStates;

		public readonly GamePadState[] _lastGamePadStates;

		public readonly bool[] _gamePadWasConnected;

		/// <summary>
		/// the radius of the controller thumbstick dead zone
		/// </summary>
		private float _deadZone;

		/// <summary>
		/// the square of the thumbstick dead zone
		/// </summary>
		private float _deadZoneSquared;

		#endregion

		#region Properties

		public KeyboardState CurrentKeyboardState { get; private set; }

		public KeyboardState LastKeyboardState { get; private set; }

		/// <summary>
		/// Gets or sets the size of the thumbstick dead zone.
		/// To square off the dead zone, set the DeadZoneType to Axial and set this to 0.5f
		/// </summary>
		/// <value>The size of the dead zone.</value>
		public float DeadZone
		{
			get { return _deadZone; }
			set
			{
				_deadZone = value;
				_deadZoneSquared = _deadZone * _deadZone;
			}
		}

		/// <summary>
		/// Gets the dead zone squared.
		/// </summary>
		/// <value>The dead zone squared.</value>
		public float DeadZoneSquared
		{
			get { return _deadZoneSquared; }
		}

		public bool CheckControllers { get; set; }

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Constructs a new input state.
		/// </summary>
		public InputState()
		{
			CheckControllers = true;
			CurrentKeyboardState = new KeyboardState();
			LastKeyboardState = new KeyboardState();

			_currentGamePadStates = new GamePadState[MaxInputs];
			_lastGamePadStates = new GamePadState[MaxInputs];

			_gamePadWasConnected = new bool[MaxInputs];
			for (int i = 0; i < MaxInputs; i++)
			{
				_gamePadWasConnected[i] = false;
			}

			DeadZone = 0.27f;
		}

		#endregion //Initialization

		#region Public Methods

		/// <summary>
		/// Reads the latest state of the keyboard and gamepad.
		/// </summary>
		public virtual void Update()
		{
			LastKeyboardState = CurrentKeyboardState;
			CurrentKeyboardState = Keyboard.GetState();

			if (CheckControllers)
			{
				for (int i = 0; i < MaxInputs; i++)
				{
					_lastGamePadStates[i] = _currentGamePadStates[i];
					_currentGamePadStates[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.None);

					// Keep track of whether a gamepad has ever been connected, so we can detect if it is unplugged.
					if (_currentGamePadStates[i].IsConnected)
					{
						_gamePadWasConnected[i] = true;
					}
				}
			}
		}

		/// <summary>
		/// Helper for checking if a key was newly pressed during this update. The
		/// controllingPlayer parameter specifies which player to read input for.
		/// If this is null, it will accept input from any player. When a keypress
		/// is detected, the output playerIndex reports which player pressed it.
		/// </summary>
		public bool IsNewKeyPress(Keys key)
		{
			// Read input from the specified player.
			return (CurrentKeyboardState.IsKeyDown(key) &&
					LastKeyboardState.IsKeyUp(key));
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
				return (ButtonDown(playerIndex, button) &&
				        !PrevButtonDown(playerIndex, button));
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
		/// Checks for a "pause the game" input action.
		/// The controllingPlayer parameter specifies which player to read
		/// input for. If this is null, it will accept input from any player.
		/// </summary>
		public bool IsPauseGame(PlayerIndex? controllingPlayer)
		{
			//blah throwaway variable
			PlayerIndex playerIndex;

			return
				IsNewKeyPress(Keys.Escape) ||
				IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
		}

		#region Button Press Methods

		/// <summary>
		/// Check if the is button down.
		/// </summary>
		/// <returns><c>true</c>, if is button is currently active, <c>false</c> otherwise.</returns>
		/// <param name="myPlayer">My player.</param>
		/// <param name="button">Button.</param>
		public bool ButtonDown(PlayerIndex myPlayer, Buttons button)
		{
			return ButtonDown((int)myPlayer, button);
		}

		/// <summary>
		/// Check if the wass button down last time
		/// </summary>
		/// <returns><c>true</c>, if is button is currently active, <c>false</c> otherwise.</returns>
		/// <param name="myPlayer">My player.</param>
		/// <param name="button">Button.</param>
		public bool PrevButtonDown(PlayerIndex myPlayer, Buttons button)
		{
			//Get the game pad state
			return PrevButtonDown((int)myPlayer, button);
		}

		/// <summary>
		/// Check if the is button down.
		/// </summary>
		/// <returns><c>true</c>, if down was buttoned, <c>false</c> otherwise.</returns>
		/// <param name="iPlayerIndex">I player index.</param>
		/// <param name="button">Button.</param>
		public bool ButtonDown(int iPlayerIndex, Buttons button)
		{
			//check that button on that gamepad
			return CheckButton(_currentGamePadStates[iPlayerIndex], button);
		}

		/// <summary>
		/// Check if the wass button down last time
		/// </summary>
		/// <returns><c>true</c>, if button down was previoused, <c>false</c> otherwise.</returns>
		/// <param name="iPlayerIndex">I player index.</param>
		/// <param name="button">Button.</param>
		public bool PrevButtonDown(int iPlayerIndex, Buttons button)
		{
			//check that button on that gamepad
			return CheckButton(_lastGamePadStates[iPlayerIndex], button);
		}

		/// <summary>
		/// Given a game pad state and a button, check if the button is down on that gamepadstate
		/// </summary>
		/// <returns><c>true</c>, if button was checked, <c>false</c> otherwise.</returns>
		/// <param name="myGamePad">My game pad.</param>
		/// <param name="button">Button.</param>
		private bool CheckButton(GamePadState myGamePad, Buttons button)
		{
			switch (button)
			{
				case Buttons.DPadUp:
				{
					return ButtonState.Pressed == myGamePad.DPad.Up;
				}
				case Buttons.DPadDown:
				{
					//don't do down if a horizontal direction is held
					return (!CheckButton(myGamePad, Buttons.DPadLeft) &&
					        !CheckButton(myGamePad, Buttons.DPadRight) &&
					        (ButtonState.Pressed == myGamePad.DPad.Down));
				}
				case Buttons.DPadLeft:
				{
					//don't do horizontal if up direction is held
					return (!CheckButton(myGamePad, Buttons.DPadUp) &&
					        (ButtonState.Pressed == myGamePad.DPad.Left));
				}
				case Buttons.DPadRight:
				{
					//don't do horizontal if up direction is held
					return (!CheckButton(myGamePad, Buttons.DPadUp) &&
					        (ButtonState.Pressed == myGamePad.DPad.Right));
				}
				case Buttons.Start:
				{
					return ButtonState.Pressed == myGamePad.Buttons.Start;
				}
				case Buttons.Back:
				{
					return ButtonState.Pressed == myGamePad.Buttons.Back;
				}
				case Buttons.LeftStick:
				{
					return ButtonState.Pressed == myGamePad.Buttons.LeftStick;
				}
				case Buttons.RightStick:
				{
					return ButtonState.Pressed == myGamePad.Buttons.RightStick;
				}
				case Buttons.LeftShoulder:
				{
					return ButtonState.Pressed == myGamePad.Buttons.LeftShoulder;
				}
				case Buttons.RightShoulder:
				{
					return ButtonState.Pressed == myGamePad.Buttons.RightShoulder;
				}
				case Buttons.BigButton:
				{
					return ButtonState.Pressed == myGamePad.Buttons.BigButton;
				}
				case Buttons.A:
				{
					return ButtonState.Pressed == myGamePad.Buttons.A;
				}
				case Buttons.B:
				{
					return ButtonState.Pressed == myGamePad.Buttons.B;
				}
				case Buttons.X:
				{
					return ButtonState.Pressed == myGamePad.Buttons.X;
				}
				case Buttons.Y:
				{
					return ButtonState.Pressed == myGamePad.Buttons.Y;
				}
				case Buttons.RightTrigger:
				{
					return myGamePad.Triggers.Right > TriggerDeadZone;
				}
				case Buttons.LeftTrigger:
				{
					return myGamePad.Triggers.Left > TriggerDeadZone;
				}
				case Buttons.LeftThumbstickUp:
				{
					return myGamePad.ThumbSticks.Left.Y < -DeadZone;
				}
				case Buttons.LeftThumbstickDown:
				{
					//don't do down if a horizontal direction is held
					return (!CheckButton(myGamePad, Buttons.LeftThumbstickLeft) &&
					        !CheckButton(myGamePad, Buttons.LeftThumbstickRight) &&
					        (myGamePad.ThumbSticks.Left.Y > DeadZone));
				}
				case Buttons.LeftThumbstickLeft:
				{
					//don't do horizontal if up direction is held
					return (!CheckButton(myGamePad, Buttons.LeftThumbstickUp) &&
					        (myGamePad.ThumbSticks.Left.X < -DeadZone));
				}
				case Buttons.LeftThumbstickRight:
				{
					//don't do horizontal if up direction is held
					return (!CheckButton(myGamePad, Buttons.LeftThumbstickUp) &&
					        (myGamePad.ThumbSticks.Left.X > DeadZone));
				}
				case Buttons.RightThumbstickUp:
				{
					return myGamePad.ThumbSticks.Right.Y < -DeadZone;
				}
				case Buttons.RightThumbstickDown:
				{
					//don't do down if a horizontal direction is held
					return (!CheckButton(myGamePad, Buttons.RightThumbstickLeft) &&
							!CheckButton(myGamePad, Buttons.RightThumbstickRight) &&
							 (myGamePad.ThumbSticks.Right.Y > DeadZone));
				}
				case Buttons.RightThumbstickLeft:
				{
					//don't do horizontal if up direction is held
					return (!CheckButton(myGamePad, Buttons.RightThumbstickUp) &&
							(myGamePad.ThumbSticks.Right.X < -DeadZone));
				}
				case Buttons.RightThumbstickRight:
				{
					//don't do horizontal if up direction is held
					return (!CheckButton(myGamePad, Buttons.RightThumbstickUp) &&
							(myGamePad.ThumbSticks.Right.X > DeadZone));
				}
				default:
				{
					return false;
				}
			}
		}

		#endregion //Button Press Methods

		#region Menu Methods

		/// <summary>
		/// Checks for a "menu select" input action.
		/// The controllingPlayer parameter specifies which player to read input for.
		/// If this is null, it will accept input from any player. When the action
		/// is detected, the output playerIndex reports which player pressed it.
		/// </summary>
		public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			//default to player 1 in case of keyboard
			playerIndex = PlayerIndex.One;

			return
				IsNewKeyPress(Keys.Space) ||
				IsNewKeyPress(Keys.Enter) ||
				IsNewKeyPress(Keys.Z) ||
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
			//default to player 1 in case of keyboard
			playerIndex = PlayerIndex.One;

			return
				IsNewKeyPress(Keys.Escape) ||
				IsNewKeyPress(Keys.X) ||
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
				IsNewKeyPress(Keys.Up) ||
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
				IsNewKeyPress(Keys.Down) ||
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
				IsNewKeyPress(Keys.Left) ||
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
				IsNewKeyPress(Keys.Right) ||
				IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out playerIndex) ||
				IsNewButtonPress(Buttons.LeftThumbstickRight, controllingPlayer, out playerIndex);
		}

		#endregion //Menu Methods

		#endregion //Public Methods
	}
}