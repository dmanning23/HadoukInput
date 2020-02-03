using MatrixExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace HadoukInput
{
	/// <summary>
	/// This is a class that wraps around a raw controller object and abstracts the input a little bit
	/// </summary>
	public class ControllerWrapper : IControllerWrapper
	{
		#region Properties

		/// <summary>
		/// button mappings for all 4 controllers
		/// These map a controller action to a button on a controller.
		/// These can be changed to do button remapping...
		/// TODO: add a function to do button remapping:  should take an action & button, reset the actions mapped to that same button
		/// </summary>
		static public List<ButtonMap> ButtonMaps { get; private set; }

		/// <summary>
		/// key mappings for all 4 controllers
		/// These map a controller action to a key on the keyboard.
		/// These can be changed to do key remapping...
		/// </summary>
		static public List<KeyMap> KeyMaps { get; private set; }

		/// <summary>
		/// If this is a gamepad input, which gamepad is it?
		/// </summary>
		public int GamePadIndex { get; set; }

		/// <summary>
		/// Gets the controller sticks.
		/// </summary>
		/// <value>The controller sticks.</value>
		public ThumbsticksWrapper Thumbsticks { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="HadoukInput.ControllerWrapper"/> also uses keyboard.
		/// </summary>
		/// <value><c>true</c> if use keyboard; otherwise, <c>false</c>.</value>
		public bool UseKeyboard { get; set; }

		public bool ControllerPluggedIn { get; set; }

		/// <summary>
		/// Check if any direction is currently held...
		/// added because the thumbstick direction is just 0.0 if neutral stick
		/// </summary>
		public bool AnyDirectionHeld
		{
			get
			{
				return Thumbsticks.LeftThumbstick.Direction.LengthSquared() != 0.0f;
			}
		}

		///<summary>
		///flags for which actions are pressed
		///one flag for each controller action
		///</summary>
		private bool[] ControllerActionPress { get; set; }

		/// <summary>
		/// list of the directions and if they are being held down
		/// only 4 flags, for each direction
		/// </summary>
		private bool[] ControllerActionHeld { get; set; }

		/// <summary>
		/// list of flags for which actions have been released
		/// only 4 flags, for each direction
		/// </summary>
		private bool[] ControllerActionRelease { get; set; }

		#endregion //Properties

		#region Methods

		#region Initialization / Cleanup

		/// <summary>
		/// Initializes the <see cref="HadoukInput.ControllerWrapper"/> class.
		/// Thereare a few variables in Monogame that screw stuff up... set them here
		/// </summary>
		static ControllerWrapper()
		{
			ButtonMaps = new List<ButtonMap>();
			KeyMaps = new List<KeyMap>();
			for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
			{
				ButtonMaps.Add(new ButtonMap());
				KeyMaps.Add(new KeyMap(i));
			}
		}

		/// <summary>
		///	hello, standard constructor!
		/// </summary>
		/// <param name="iGamePadIndex">If this isn't a keyboard, which gamepad index it should use.</param>
		public ControllerWrapper(int? playerIndex, bool useKeyboard = false)
		{
			Thumbsticks = new ThumbsticksWrapper(this);

			UseKeyboard = useKeyboard;

			if (playerIndex.HasValue)
			{
				GamePadIndex = playerIndex.Value;
			}

			ControllerActionPress = new bool[(int)ControllerAction.NumControllerActions];
			ControllerActionHeld = new bool[(int)ControllerAction.NumControllerActions];
			ControllerActionRelease = new bool[(int)ControllerAction.NumControllerActions];

			//initialize input states
			ResetController();
		}

		/// <summary>
		/// Reset all the controls to null
		/// </summary>
		public void ResetController()
		{
			Thumbsticks.Reset();
			for (int i = 0; i < (int)ControllerAction.NumControllerActions; i++)
			{
				ControllerActionPress[i] = false;
				ControllerActionHeld[i] = false;
				ControllerActionRelease[i] = false;
			}
		}

		#endregion //Initialization / Cleanup

		/// <summary>
		/// update the current state of this controller interface
		/// </summary>
		/// <param name="inputState">current state of all the input in the system</param>
		public virtual void Update(IInputState inputState)
		{
			//check if the controller is plugged in
			ControllerPluggedIn = inputState.IsConnected(GamePadIndex);

			//update the thumbstick
			Thumbsticks.UpdateThumbsticks(inputState, GamePadIndex);

			for (ControllerAction j = 0; j < ControllerAction.NumControllerActions; j++)
			{
				//update which buttons were presses this frame
				ControllerActionPress[(int)j] = CheckControllerActionPress(inputState, GamePadIndex, j);

				//update which directions are held this frame
				ControllerActionHeld[(int)j] = CheckControllerActionHeld(inputState, GamePadIndex, j);

				//update which dircetions are released this frame
				ControllerActionRelease[(int)j] = CheckControllerActionReleased(inputState, GamePadIndex, j);
			}
		}

		/// <summary>
		/// Get the keyboard key that is mapped to an action
		/// </summary>
		/// <param name="gamePadIndex"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public Keys MappedKey(int gamePadIndex, ControllerAction action)
		{
			return KeyMaps[gamePadIndex].ActionMap(action);
		}

		/// <summary>
		/// Check for a specific keystroke
		/// only used for button press keystrokes
		/// </summary>
		/// <param name="keystroke">the keystroke to check for</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke keystroke)
		{
			switch (keystroke)
			{
				case EKeystroke.Up:
					{
						return ControllerActionPress[(int)ControllerAction.Up];
					}
				case EKeystroke.Down:
					{
						return ControllerActionPress[(int)ControllerAction.Down];
					}
				case EKeystroke.Back:
					{
						return ControllerActionPress[(int)ControllerAction.Left];
					}
				case EKeystroke.Forward:
					{
						return ControllerActionPress[(int)ControllerAction.Right];
					}

				case EKeystroke.A:
					{
						return ControllerActionPress[(int)ControllerAction.A];
					}
				case EKeystroke.B:
					{
						return ControllerActionPress[(int)ControllerAction.B];
					}
				case EKeystroke.X:
					{
						return ControllerActionPress[(int)ControllerAction.X];
					}
				case EKeystroke.Y:
					{
						return ControllerActionPress[(int)ControllerAction.Y];
					}
				case EKeystroke.LShoulder:
					{
						return ControllerActionPress[(int)ControllerAction.LShoulder];
					}
				case EKeystroke.RShoulder:
					{
						return ControllerActionPress[(int)ControllerAction.RShoulder];
					}
				case EKeystroke.LTrigger:
					{
						return ControllerActionPress[(int)ControllerAction.LTrigger];
					}
				case EKeystroke.RTrigger:
					{
						return ControllerActionPress[(int)ControllerAction.RTrigger];
					}

				//CHECK BUTTONS RELEASED

				case EKeystroke.ARelease:
					{
						return ControllerActionRelease[(int)ControllerAction.A];
					}
				case EKeystroke.BRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.B];
					}
				case EKeystroke.XRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.X];
					}
				case EKeystroke.YRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.Y];
					}
				case EKeystroke.LShoulderRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.LShoulder];
					}
				case EKeystroke.RShoulderRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.RShoulder];
					}
				case EKeystroke.LTriggerRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.LTrigger];
					}
				case EKeystroke.RTriggerRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.RTrigger];
					}

				default:
					{
						//you used the wrong CheckKeystroke method?
						//you passed in one of the direction+button keystrokes?
						return false;
					}
			}
		}

		/// <summary>
		/// Check for a specific keystroke
		/// only used for button press keystrokes
		/// </summary>
		/// <param name="keystroke">the keystroke to check for</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystrokeHeld(EKeystroke keystroke)
		{
			switch (keystroke)
			{
				case EKeystroke.Up:
					{
						return ControllerActionHeld[(int)ControllerAction.Up];
					}
				case EKeystroke.Down:
					{
						return ControllerActionHeld[(int)ControllerAction.Down];
					}
				case EKeystroke.Back:
					{
						return ControllerActionHeld[(int)ControllerAction.Left];
					}
				case EKeystroke.Forward:
					{
						return ControllerActionHeld[(int)ControllerAction.Right];
					}

				case EKeystroke.A:
					{
						return ControllerActionHeld[(int)ControllerAction.A];
					}
				case EKeystroke.B:
					{
						return ControllerActionHeld[(int)ControllerAction.B];
					}
				case EKeystroke.X:
					{
						return ControllerActionHeld[(int)ControllerAction.X];
					}
				case EKeystroke.Y:
					{
						return ControllerActionHeld[(int)ControllerAction.Y];
					}
				case EKeystroke.LShoulder:
					{
						return ControllerActionHeld[(int)ControllerAction.LShoulder];
					}
				case EKeystroke.RShoulder:
					{
						return ControllerActionHeld[(int)ControllerAction.RShoulder];
					}
				case EKeystroke.LTrigger:
					{
						return ControllerActionHeld[(int)ControllerAction.LTrigger];
					}
				case EKeystroke.RTrigger:
					{
						return ControllerActionHeld[(int)ControllerAction.RTrigger];
					}

				//CHECK BUTTONS RELEASED

				case EKeystroke.ARelease:
					{
						return ControllerActionRelease[(int)ControllerAction.A];
					}
				case EKeystroke.BRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.B];
					}
				case EKeystroke.XRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.X];
					}
				case EKeystroke.YRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.Y];
					}
				case EKeystroke.LShoulderRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.LShoulder];
					}
				case EKeystroke.RShoulderRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.RShoulder];
					}
				case EKeystroke.LTriggerRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.LTrigger];
					}
				case EKeystroke.RTriggerRelease:
					{
						return ControllerActionRelease[(int)ControllerAction.RTrigger];
					}

				default:
					{
						//you used the wrong CheckKeystroke method?
						//you passed in one of the direction+button keystrokes?
						return false;
					}
			}
		}

		public static Vector2 UpVect(bool flipped, Vector2 direction)
		{
			if (flipped)
			{
				//If it is flipped, the up vector is the direction rotated -90 degrees
				var rotate = MatrixExt.Orientation(-1.57079633f);
				return MatrixExt.Multiply(rotate, direction);
			}
			else
			{
				//If it is not flipped, the up vector is the direction rotated 90 degrees
				var rotate = MatrixExt.Orientation(1.57079633f);
				return MatrixExt.Multiply(rotate, direction);
			}
		}

		/// <summary>
		/// Check for a specific keystroke, but with a rotated direction.
		/// </summary>
		/// <param name="keystroke">the keystroke to check for</param>
		/// <param name="flipped">Whether or not the check should be flipped on x axis.  If true, "left" will be "forward" and vice/versa</param>
		/// <param name="direction">The NORMALIZED direction to check against</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke keystroke, bool flipped, Vector2 direction)
		{
			//This method should only be used for the really basic keystrokes...

			//Get the 'up' vector...
			var upVect = UpVect(flipped, direction);

			switch (keystroke)
			{
				//CHECK THE DIRECTIONS

				case EKeystroke.Up:
					{
						//get the direction to check for 'up'
						return Thumbsticks.LeftThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}
				case EKeystroke.Down:
					{
						//get the direction to check for 'down'
						return Thumbsticks.LeftThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}
				case EKeystroke.Forward:
					{
						//get the direction to check for 'forward'
						return Thumbsticks.LeftThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}
				case EKeystroke.Back:
					{
						//get the direction to check for 'Back'
						return Thumbsticks.LeftThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}

				//Check the left thumnbsticks released
				case EKeystroke.Neutral:
					{
						//are any keys being held?
						if (!ControllerActionHeld[(int)ControllerAction.Up] &&
							!ControllerActionHeld[(int)ControllerAction.Down] &&
							!ControllerActionHeld[(int)ControllerAction.Right] &&
							!ControllerActionHeld[(int)ControllerAction.Left])
						{
							//did a "key up" action occur?
							return (ControllerActionRelease[(int)ControllerAction.Up] ||
								ControllerActionRelease[(int)ControllerAction.Down] ||
								ControllerActionRelease[(int)ControllerAction.Right] ||
								ControllerActionRelease[(int)ControllerAction.Left]);
						}
						else
						{
							return false;
						}
					}

				//CHECK RIGHT THUMBSTICK STUFF

				case EKeystroke.UpR:
					{
						//get the direction to check for 'up'
						return Thumbsticks.RightThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}
				case EKeystroke.DownR:
					{
						//get the direction to check for 'down'
						return Thumbsticks.RightThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}
				case EKeystroke.ForwardR:
					{
						//get the direction to check for 'forward'
						return Thumbsticks.RightThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}
				case EKeystroke.BackR:
					{
						//get the direction to check for 'Back'
						return Thumbsticks.RightThumbstick.CheckKeystroke(keystroke, direction, upVect);
					}

				//Check the right thumnbsticks released
				case EKeystroke.NeutralR:
					{
						//are any keys being held?
						if (!ControllerActionHeld[(int)ControllerAction.UpR] &&
							!ControllerActionHeld[(int)ControllerAction.DownR] &&
							!ControllerActionHeld[(int)ControllerAction.RightR] &&
							!ControllerActionHeld[(int)ControllerAction.LeftR])
						{
							//did a "key up" action occur?
							return (ControllerActionRelease[(int)ControllerAction.UpR] ||
								ControllerActionRelease[(int)ControllerAction.DownR] ||
								ControllerActionRelease[(int)ControllerAction.RightR] ||
								ControllerActionRelease[(int)ControllerAction.LeftR]);
						}
						else
						{
							return false;
						}
					}

				case EKeystroke.ForwardShoulder:
					{
						return (flipped ? ControllerActionPress[(int)ControllerAction.LShoulder] :
							ControllerActionPress[(int)ControllerAction.RShoulder]);
					}

				case EKeystroke.BackShoulder:
					{
						return (!flipped ? ControllerActionPress[(int)ControllerAction.LShoulder] :
							ControllerActionPress[(int)ControllerAction.RShoulder]);
					}

				//CHECK BUTTONS

				default:
					{
						return CheckKeystroke(keystroke);
					}
			}
		}

		#region Private Methods

		/// <summary>
		/// Check whether the player is hitting a mapped button
		/// </summary>
		/// <param name="playerIndex">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button was activated this frame</returns>
		private bool CheckControllerActionPress(IInputState inputState, int playerIndex, ControllerAction action)
		{
			if (UseKeyboard && (action < ControllerAction.UpR))
			{
				//get the key to check
				if (CheckKeyDown(inputState, MappedKey(playerIndex, action)))
				{
					return true;
				}
			}

			//First check if it is a direction
			switch (action)
			{
				case ControllerAction.Up:
					{
						return ((inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickUp) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickUp)) ||
								(inputState.ButtonDown(playerIndex, Buttons.DPadUp) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.DPadUp)));
					}
				case ControllerAction.Down:
					{
						return ((inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickDown) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickDown)) ||
								(inputState.ButtonDown(playerIndex, Buttons.DPadDown) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.DPadDown)));
					}
				case ControllerAction.Left:
					{
						return ((inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickLeft) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickLeft)) ||
								(inputState.ButtonDown(playerIndex, Buttons.DPadLeft) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.DPadLeft)));
					}
				case ControllerAction.Right:
					{
						return ((inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickRight) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickRight)) ||
								(inputState.ButtonDown(playerIndex, Buttons.DPadRight) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.DPadRight)));
					}
				case ControllerAction.UpR:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.RightThumbstickUp) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickUp));
					}
				case ControllerAction.DownR:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.RightThumbstickDown) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickDown));
					}
				case ControllerAction.LeftR:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.RightThumbstickLeft) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickLeft));
					}
				case ControllerAction.RightR:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.RightThumbstickRight) &&
								 !inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickRight));
					}
				default:
					{
						//get the attack button to check
						var mappedButton = ButtonMaps[playerIndex].ActionMap(action);
						return (inputState.ButtonDown(playerIndex, mappedButton) && !inputState.PrevButtonDown(playerIndex, mappedButton));
					}
			}
		}

		/// <summary>
		/// Check whether the player is holding a mapped button
		/// </summary>
		/// <param name="playerIndex">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button is held this frame</returns>
		private bool CheckControllerActionHeld(IInputState inputState, int playerIndex, ControllerAction action)
		{
			if (UseKeyboard && (action < ControllerAction.UpR))
			{
				//get the key to check
				if (inputState.CurrentKeyboardState.IsKeyDown(MappedKey(playerIndex, action)))
				{
					return true;
				}
			}

			//First check if it is a direction
			switch (action)
			{
				case ControllerAction.Up:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickUp) ||
								inputState.ButtonDown(playerIndex, Buttons.DPadUp));
					}
				case ControllerAction.Down:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickDown) ||
								inputState.ButtonDown(playerIndex, Buttons.DPadDown));
					}
				case ControllerAction.Left:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickLeft) ||
								inputState.ButtonDown(playerIndex, Buttons.DPadLeft));
					}
				case ControllerAction.Right:
					{
						return (inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickRight) ||
								inputState.ButtonDown(playerIndex, Buttons.DPadRight));
					}
				case ControllerAction.UpR:
					{
						return inputState.ButtonDown(playerIndex, Buttons.RightThumbstickUp);
					}
				case ControllerAction.DownR:
					{
						return inputState.ButtonDown(playerIndex, Buttons.RightThumbstickDown);
					}
				case ControllerAction.LeftR:
					{
						return inputState.ButtonDown(playerIndex, Buttons.RightThumbstickLeft);
					}
				case ControllerAction.RightR:
					{
						return inputState.ButtonDown(playerIndex, Buttons.RightThumbstickRight);
					}
				default:
					{
						//get the attack button to check
						var mappedButton = ButtonMaps[playerIndex].ActionMap(action);
						return inputState.ButtonDown(playerIndex, mappedButton);
					}
			}
		}

		/// <summary>
		/// Check whether the player released a mapped button this frame
		/// </summary>
		/// <param name="i">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button was deactivated this frame</returns>
		private bool CheckControllerActionReleased(IInputState inputState, int playerIndex, ControllerAction action)
		{
			if (UseKeyboard && (action < ControllerAction.UpR))
			{
				//first do the keyboard check
				if (CheckKeyUp(inputState, MappedKey(playerIndex, action)))
				{
					return true;
				}
			}

			//First check if it is a direction
			switch (action)
			{
				case ControllerAction.Up:
					{
						return ((!inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickUp) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickUp)) ||
								(!inputState.ButtonDown(playerIndex, Buttons.DPadUp) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.DPadUp)));
					}
				case ControllerAction.Down:
					{
						return ((!inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickDown) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickDown)) ||
								(!inputState.ButtonDown(playerIndex, Buttons.DPadDown) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.DPadDown)));
					}
				case ControllerAction.Left:
					{
						return ((!inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickLeft) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickLeft)) ||
								(!inputState.ButtonDown(playerIndex, Buttons.DPadLeft) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.DPadLeft)));
					}
				case ControllerAction.Right:
					{
						return ((!inputState.ButtonDown(playerIndex, Buttons.LeftThumbstickRight) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.LeftThumbstickRight)) ||
								(!inputState.ButtonDown(playerIndex, Buttons.DPadRight) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.DPadRight)));
					}
				case ControllerAction.UpR:
					{
						return (!inputState.ButtonDown(playerIndex, Buttons.RightThumbstickUp) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickUp));
					}
				case ControllerAction.DownR:
					{
						return (!inputState.ButtonDown(playerIndex, Buttons.RightThumbstickDown) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickDown));
					}
				case ControllerAction.LeftR:
					{
						return (!inputState.ButtonDown(playerIndex, Buttons.RightThumbstickLeft) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickLeft));
					}
				case ControllerAction.RightR:
					{
						return (!inputState.ButtonDown(playerIndex, Buttons.RightThumbstickRight) &&
								 inputState.PrevButtonDown(playerIndex, Buttons.RightThumbstickRight));
					}
				default:
					{
						//get the attack button to check
						Buttons mappedButton = ButtonMaps[playerIndex].ActionMap(action);
						return (!inputState.ButtonDown(playerIndex, mappedButton) && inputState.PrevButtonDown(playerIndex, mappedButton));
					}
			}
		}

		/// <summary>
		/// Check if a keyboard key was pressed this update
		/// </summary>
		/// <param name="inputState">current input state</param>
		/// <param name="key">key to check</param>
		/// <returns>bool: key was pressed this update</returns>
		private bool CheckKeyDown(IInputState inputState, Keys key)
		{
			if (UseKeyboard)
			{
				return (inputState.CurrentKeyboardState.IsKeyDown(key) && inputState.LastKeyboardState.IsKeyUp(key));
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// check if a key was released this update
		/// </summary>
		/// <param name="inputState">current input state</param>
		/// <param name="i">controller index</param>
		/// <param name="key">key to check</param>
		/// <returns>bool: true if the key was released this update.</returns>
		private bool CheckKeyUp(IInputState inputState, Keys key)
		{
			if (UseKeyboard)
			{
				return (inputState.CurrentKeyboardState.IsKeyUp(key) && inputState.LastKeyboardState.IsKeyDown(key));
			}
			else
			{
				return false;
			}
		}

		#endregion //Private Methods

		#endregion //Methods
	}
}