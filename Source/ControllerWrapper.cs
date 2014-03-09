using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MatrixExtensions;
using System.Collections.Generic;

#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif

namespace HadoukInput
{
	/// <summary>
	/// This is a class that wraps around a raw controller object and abstracts the input a little bit
	/// </summary>
	public class ControllerWrapper
	{
		#region Mapped Keys

		/// <summary>
		/// button mappings for all 4 controllers
		/// These map a controller action to a button on a controller.
		/// These can be changed to do button remapping...
		/// TODO: add a function to do button remapping:  should take an action & button, reset the actions mapped to that same button
		/// </summary>
		static public Buttons[,] g_ButtonMap =
		{
			{
				Buttons.A,
				Buttons.B,
				Buttons.X,
				Buttons.Y,
				Buttons.LeftShoulder,
				Buttons.RightShoulder,
				Buttons.LeftTrigger,
				Buttons.RightTrigger
			},
			{
				Buttons.A,
				Buttons.B,
				Buttons.X,
				Buttons.Y,
				Buttons.LeftShoulder,
				Buttons.RightShoulder,
				Buttons.LeftTrigger,
				Buttons.RightTrigger
			},
			{
				Buttons.A,
				Buttons.B,
				Buttons.X,
				Buttons.Y,
				Buttons.LeftShoulder,
				Buttons.RightShoulder,
				Buttons.LeftTrigger,
				Buttons.RightTrigger
			},
			{
				Buttons.A,
				Buttons.B,
				Buttons.X,
				Buttons.Y,
				Buttons.LeftShoulder,
				Buttons.RightShoulder,
				Buttons.LeftTrigger,
				Buttons.RightTrigger
			}
		};

		/// <summary>
		/// key mappings for all 4 controllers
		/// These map a controller action to a key on the keyboard.
		/// These can be changed to do key remapping...
		/// TODO: add a function to do button remapping:  should take an action & button, reset the actions mapped to that same button
		/// </summary>
		static public Keys[,] g_KeyMap =
		{
			{
				Keys.Up,
				Keys.Down,
				Keys.Right,
				Keys.Left,
				Keys.Z, //Buttons.A,
				Keys.X, //Buttons.B,
				Keys.A, //Buttons.X,
				Keys.S, //Buttons.Y,
				Keys.D, //Buttons.LeftShoulder,
				Keys.F, //Buttons.RightShoulder,
				Keys.C, //Buttons.LeftTrigger,
				Keys.V  //Buttons.RightTrigger
			},
			{
				Keys.Up,
				Keys.Down,
				Keys.Right,
				Keys.Left,
				Keys.Z,
				Keys.X,
				Keys.A,
				Keys.S,
				Keys.D,
				Keys.F,
				Keys.C,
				Keys.V
			},
			{
				Keys.Up,
				Keys.Down,
				Keys.Right,
				Keys.Left,
				Keys.Z,
				Keys.X,
				Keys.A,
				Keys.S,
				Keys.D,
				Keys.F,
				Keys.C,
				Keys.V
			},
			{
				Keys.Up,
				Keys.Down,
				Keys.Right,
				Keys.Left,
				Keys.Z,
				Keys.X,
				Keys.A,
				Keys.S,
				Keys.D,
				Keys.F,
				Keys.C,
				Keys.V
			}
		};

		#endregion

		#region Member Variables

		/// <summary>
		/// If this is a gamepad input, which gamepad is it?
		/// </summary>
		public PlayerIndex GamePadIndex { get; set; }

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

		#endregion

		#region Properties

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

		#endregion

		#region Methods

		#region Initialization / Cleanup

		/// <summary>
		/// Initializes the <see cref="HadoukInput.ControllerWrapper"/> class.
		/// Thereare a few variables in Monogame that screw stuff up... set them here
		/// </summary>
		static ControllerWrapper()
		{
#if !XNA
			GamePadThumbSticks.Gate = GamePadThumbSticks.GateType.None;
#endif
		}

		/// <summary>
		///	hello, standard constructor!
		/// </summary>
		/// <param name="iGamePadIndex">If this isn't a keyboard, which gamepad index it should use.</param>
		public ControllerWrapper(PlayerIndex? eGamePadIndex, bool bUseKeyboard = false)
		{
			Thumbsticks = new ThumbsticksWrapper(this);

#if OUYA
			//no keyboard at all on the Ouya
			UseKeyboard = false;
#else
			UseKeyboard = bUseKeyboard;
#endif

			if (eGamePadIndex.HasValue)
			{
				GamePadIndex = eGamePadIndex.Value;
			}

			ControllerActionPress = new bool[(int)EControllerAction.NumControllerActions];
			ControllerActionHeld = new bool[(int)EControllerAction.NumControllerActions];
			ControllerActionRelease = new bool[(int)EControllerAction.NumControllerActions];

			//initialize input states
			ResetController();
		}

		/// <summary>
		/// Reset all the controls to null
		/// </summary>
		public void ResetController()
		{
			Thumbsticks.Reset();
			for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			{
				ControllerActionPress[i] = false;
				ControllerActionHeld[i] = false;
				ControllerActionRelease[i] = false;
			}
		}

		/// <summary>
		/// Map a players controller actions to a set of keys
		/// </summary>
		/// <param name="eIndex">player index to remap</param>
		/// <param name="mappedKeys">keys to use for that player</param>
		static public void MapKeys(PlayerIndex eIndex, Keys[] mappedKeys)
		{
			//make sure there are the correct num of keys
			Debug.Assert(mappedKeys.Length == 12);

			//replace them all!
			for (int i = 0; i < mappedKeys.Length; i++)
			{
				g_KeyMap[(int)eIndex, i] = mappedKeys[i];
			}
		}

		#endregion //Initialization / Cleanup

		/// <summary>
		/// update the current state of this controller interface
		/// </summary>
		/// <param name="rInputState">current state of all the input in the system</param>
		/// <param name="rParameters">current state of all the key mappings and shit</param>
		/// <param name="bFlip">true if the character this thing controls is facing left</param>
		public void Update(InputState rInputState)
		{
			var i = (int)GamePadIndex;

			//check if the controller is plugged in
			ControllerPluggedIn = rInputState.m_CurrentGamePadStates[i].IsConnected;

			//update the thumbstick
			Thumbsticks.UpdateThumbsticks(rInputState, i);

			for (EControllerAction j = 0; j < EControllerAction.NumControllerActions; j++)
			{
				//update which buttons were presses this frame
				ControllerActionPress[(int)j] = CheckControllerActionPress(rInputState, i, j);

				//update which directions are held this frame
				ControllerActionHeld[(int)j] = CheckControllerActionHeld(rInputState, i, j);

				//update which dircetions are released this frame
				ControllerActionRelease[(int)j] = CheckControllerActionReleased(rInputState, i, j);
			}
		}

		/// <summary>
		/// Get the keyboard key that is mapped to an action
		/// </summary>
		/// <param name="iGamePadIndex"></param>
		/// <param name="iAction"></param>
		/// <returns></returns>
		internal Keys MappedKey(int iGamePadIndex, EControllerAction iAction)
		{
			return g_KeyMap[iGamePadIndex, (int)iAction];
		}

		/// <summary>
		/// Check for a specific keystroke
		/// only used for button press keystrokes
		/// </summary>
		/// <param name="eKeystroke">the keystroke to check for</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke eKeystroke)
		{
			switch (eKeystroke)
			{
				case EKeystroke.Up:
				{
					return ControllerActionPress[(int)EControllerAction.Up];
				}
				case EKeystroke.Down:
				{
					return ControllerActionPress[(int)EControllerAction.Down];
				}
				case EKeystroke.Back:
				{
					return ControllerActionPress[(int)EControllerAction.Left];
				}
				case EKeystroke.Forward:
				{
					return ControllerActionPress[(int)EControllerAction.Right];
				}

				case EKeystroke.A:
				{
					return ControllerActionPress[(int)EControllerAction.A];
				}
				case EKeystroke.B:
				{
					return ControllerActionPress[(int)EControllerAction.B];
				}
				case EKeystroke.X:
				{
					return ControllerActionPress[(int)EControllerAction.X];
				}
				case EKeystroke.Y:
				{
					return ControllerActionPress[(int)EControllerAction.Y];
				}
				case EKeystroke.LShoulder:
				{
					return ControllerActionPress[(int)EControllerAction.LShoulder];
				}
				case EKeystroke.RShoulder:
				{
					return ControllerActionPress[(int)EControllerAction.RShoulder];
				}
				case EKeystroke.LTrigger:
				{
					return ControllerActionPress[(int)EControllerAction.LTrigger];
				}
				case EKeystroke.RTrigger:
				{
					return ControllerActionPress[(int)EControllerAction.RTrigger];
				}

			//CHECK BUTTONS RELEASED

				case EKeystroke.ARelease:
				{
					return ControllerActionRelease[(int)EControllerAction.A];
				}
				case EKeystroke.BRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.B];
				}
				case EKeystroke.XRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.X];
				}
				case EKeystroke.YRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.Y];
				}
				case EKeystroke.LShoulderRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.LShoulder];
				}
				case EKeystroke.RShoulderRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.RShoulder];
				}
				case EKeystroke.LTriggerRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.LTrigger];
				}
				case EKeystroke.RTriggerRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.RTrigger];
				}

				default:
				{
					//you used the wrong CheckKeystroke method?
					//you passed in one of the direction+button keystrokes?
					Debug.Assert(false);
					return false;
				}
			}
		}

		/// <summary>
		/// Check for a specific keystroke
		/// only used for button press keystrokes
		/// </summary>
		/// <param name="eKeystroke">the keystroke to check for</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystrokeHeld(EKeystroke eKeystroke)
		{
			switch (eKeystroke)
			{
				case EKeystroke.Up:
				{
					return ControllerActionHeld[(int)EControllerAction.Up];
				}
				case EKeystroke.Down:
				{
					return ControllerActionHeld[(int)EControllerAction.Down];
				}
				case EKeystroke.Back:
				{
					return ControllerActionHeld[(int)EControllerAction.Left];
				}
				case EKeystroke.Forward:
				{
					return ControllerActionHeld[(int)EControllerAction.Right];
				}

				case EKeystroke.A:
				{
					return ControllerActionHeld[(int)EControllerAction.A];
				}
				case EKeystroke.B:
				{
					return ControllerActionHeld[(int)EControllerAction.B];
				}
				case EKeystroke.X:
				{
					return ControllerActionHeld[(int)EControllerAction.X];
				}
				case EKeystroke.Y:
				{
					return ControllerActionHeld[(int)EControllerAction.Y];
				}
				case EKeystroke.LShoulder:
				{
					return ControllerActionHeld[(int)EControllerAction.LShoulder];
				}
				case EKeystroke.RShoulder:
				{
					return ControllerActionHeld[(int)EControllerAction.RShoulder];
				}
				case EKeystroke.LTrigger:
				{
					return ControllerActionHeld[(int)EControllerAction.LTrigger];
				}
				case EKeystroke.RTrigger:
				{
					return ControllerActionHeld[(int)EControllerAction.RTrigger];
				}

				//CHECK BUTTONS RELEASED

				case EKeystroke.ARelease:
				{
					return ControllerActionRelease[(int)EControllerAction.A];
				}
				case EKeystroke.BRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.B];
				}
				case EKeystroke.XRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.X];
				}
				case EKeystroke.YRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.Y];
				}
				case EKeystroke.LShoulderRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.LShoulder];
				}
				case EKeystroke.RShoulderRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.RShoulder];
				}
				case EKeystroke.LTriggerRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.LTrigger];
				}
				case EKeystroke.RTriggerRelease:
				{
					return ControllerActionRelease[(int)EControllerAction.RTrigger];
				}

				default:
				{
					//you used the wrong CheckKeystroke method?
					//you passed in one of the direction+button keystrokes?
					Debug.Assert(false);
					return false;
				}
			}
		}

		public static Vector2 UpVect(bool bFlipped, Vector2 direction)
		{
			if (bFlipped)
			{
				//If it is flipped, the up vector is the direction rotated 90 degrees
				Matrix rotate = MatrixExt.Orientation(1.57079633f);
				return MatrixExt.Multiply(rotate, direction);
			}
			else
			{
				//If it is not flipped, the up vector is the direction rotated -90 degrees
				Matrix rotate = MatrixExt.Orientation(-1.57079633f);
				return MatrixExt.Multiply(rotate, direction);
			}
		}

		/// <summary>
		/// Check for a specific keystroke, but with a rotated direction.
		/// </summary>
		/// <param name="eKeystroke">the keystroke to check for</param>
		/// <param name="bFlipped">Whether or not the check should be flipped on x axis.  If true, "left" will be "forward" and vice/versa</param>
		/// <param name="direction">The NORMALIZED direction to check against</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke eKeystroke, bool bFlipped, Vector2 direction)
		{
			//This method should only be used for the really basic keystrokes...
			Debug.Assert(EKeystroke.RTriggerRelease >= eKeystroke);

			//Get the 'up' vector...
			Vector2 upVect = UpVect(bFlipped, direction);

			switch (eKeystroke)
			{
				//CHECK THE DIRECTIONS

				case EKeystroke.Up:
				{
					//get the direction to check for 'up'
					return Thumbsticks.LeftThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}
				case EKeystroke.Down:
				{
					//get the direction to check for 'down'
					return Thumbsticks.LeftThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}
				case EKeystroke.Forward:
				{
					//get the direction to check for 'forward'
					return Thumbsticks.LeftThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}
				case EKeystroke.Back:
				{
					//get the direction to check for 'Back'
					return Thumbsticks.LeftThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}

				//Check the left thumnbsticks released
				case EKeystroke.Neutral:
				{
					//are any keys being held?
					if (!ControllerActionHeld[(int)EControllerAction.Up] &&
						!ControllerActionHeld[(int)EControllerAction.Down] &&
						!ControllerActionHeld[(int)EControllerAction.Right] &&
						!ControllerActionHeld[(int)EControllerAction.Left])
					{
						//did a "key up" action occur?
						return (ControllerActionRelease[(int)EControllerAction.Up] ||
							ControllerActionRelease[(int)EControllerAction.Down] ||
							ControllerActionRelease[(int)EControllerAction.Right] ||
							ControllerActionRelease[(int)EControllerAction.Left]);
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
					return Thumbsticks.RightThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}
				case EKeystroke.DownR:
				{
					//get the direction to check for 'down'
					return Thumbsticks.RightThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}
				case EKeystroke.ForwardR:
				{
					//get the direction to check for 'forward'
					return Thumbsticks.RightThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}
				case EKeystroke.BackR:
				{
					//get the direction to check for 'Back'
					return Thumbsticks.RightThumbstick.CheckKeystroke(eKeystroke, direction, upVect);
				}

				//Check the right thumnbsticks released
				case EKeystroke.NeutralR:
				{
					//are any keys being held?
					if (!ControllerActionHeld[(int)EControllerAction.UpR] &&
						!ControllerActionHeld[(int)EControllerAction.DownR] &&
						!ControllerActionHeld[(int)EControllerAction.RightR] &&
						!ControllerActionHeld[(int)EControllerAction.LeftR])
					{
						//did a "key up" action occur?
						return (ControllerActionRelease[(int)EControllerAction.UpR] ||
							ControllerActionRelease[(int)EControllerAction.DownR] ||
							ControllerActionRelease[(int)EControllerAction.RightR] ||
							ControllerActionRelease[(int)EControllerAction.LeftR]);
					}
					else
					{
						return false;
					}
				}

				//CHECK BUTTONS

				default:
				{
					return CheckKeystroke(eKeystroke);
				}
			}
		}

		#region Private Methods

		/// <summary>
		/// Check whether the player is hitting a mapped button
		/// </summary>
		/// <param name="i">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button was activated this frame</returns>
		private bool CheckControllerActionPress(InputState rInputState, int i, EControllerAction iAction)
		{
			Debug.Assert(iAction < EControllerAction.NumControllerActions);

			if (UseKeyboard && (iAction < EControllerAction.UpR))
			{
				//get the key to check
				if (CheckKeyDown(rInputState, i, MappedKey(i, iAction)))
				{
					return true;
				}
			}

			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
				{
					return ((rInputState.ButtonDown(i, Buttons.LeftThumbstickUp) &&
					         !rInputState.PrevButtonDown(i, Buttons.LeftThumbstickUp)) ||
					        (rInputState.ButtonDown(i, Buttons.DPadUp) &&
					         !rInputState.PrevButtonDown(i, Buttons.DPadUp)));
				}
				case EControllerAction.Down:
				{
					return ((rInputState.ButtonDown(i, Buttons.LeftThumbstickDown) &&
					         !rInputState.PrevButtonDown(i, Buttons.LeftThumbstickDown)) ||
					        (rInputState.ButtonDown(i, Buttons.DPadDown) &&
					         !rInputState.PrevButtonDown(i, Buttons.DPadDown)));
				}
				case EControllerAction.Left:
				{
					return ((rInputState.ButtonDown(i, Buttons.LeftThumbstickLeft) &&
					         !rInputState.PrevButtonDown(i, Buttons.LeftThumbstickLeft)) ||
					        (rInputState.ButtonDown(i, Buttons.DPadLeft) &&
					         !rInputState.PrevButtonDown(i, Buttons.DPadLeft)));
				}
				case EControllerAction.Right:
				{
					return ((rInputState.ButtonDown(i, Buttons.LeftThumbstickRight) &&
					         !rInputState.PrevButtonDown(i, Buttons.LeftThumbstickRight)) ||
					        (rInputState.ButtonDown(i, Buttons.DPadRight) &&
					         !rInputState.PrevButtonDown(i, Buttons.DPadRight)));
				}
				case EControllerAction.UpR:
				{
					return (rInputState.ButtonDown(i, Buttons.RightThumbstickUp) &&
							 !rInputState.PrevButtonDown(i, Buttons.RightThumbstickUp));
				}
				case EControllerAction.DownR:
				{
					return (rInputState.ButtonDown(i, Buttons.RightThumbstickDown) &&
							 !rInputState.PrevButtonDown(i, Buttons.RightThumbstickDown));
				}
				case EControllerAction.LeftR:
				{
					return (rInputState.ButtonDown(i, Buttons.RightThumbstickLeft) &&
							 !rInputState.PrevButtonDown(i, Buttons.RightThumbstickLeft));
				}
				case EControllerAction.RightR:
				{
					return (rInputState.ButtonDown(i, Buttons.RightThumbstickRight) &&
							 !rInputState.PrevButtonDown(i, Buttons.RightThumbstickRight));
				}
				default:
				{
					//get the attack button to check
					Buttons mappedButton = g_ButtonMap[i, (iAction - EControllerAction.A)];
					return (rInputState.ButtonDown(i, mappedButton) && !rInputState.PrevButtonDown(i, mappedButton));
				}
			}
		}

		/// <summary>
		/// Check whether the player is holding a mapped button
		/// </summary>
		/// <param name="i">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button is held this frame</returns>
		private bool CheckControllerActionHeld(InputState rInputState, int i, EControllerAction iAction)
		{
			if (UseKeyboard && (iAction < EControllerAction.UpR))
			{
				//get the key to check
				if (rInputState.CurrentKeyboardState.IsKeyDown(MappedKey(i, iAction)))
				{
					return true;
				}
			}

			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
				{
					return (rInputState.ButtonDown(i, Buttons.LeftThumbstickUp) ||
					        rInputState.ButtonDown(i, Buttons.DPadUp));
				}
				case EControllerAction.Down:
				{
					return (rInputState.ButtonDown(i, Buttons.LeftThumbstickDown) ||
					        rInputState.ButtonDown(i, Buttons.DPadDown));
				}
				case EControllerAction.Left:
				{
					return (rInputState.ButtonDown(i, Buttons.LeftThumbstickLeft) ||
					        rInputState.ButtonDown(i, Buttons.DPadLeft));
				}
				case EControllerAction.Right:
				{
					return (rInputState.ButtonDown(i, Buttons.LeftThumbstickRight) ||
					        rInputState.ButtonDown(i, Buttons.DPadRight));
				}
				case EControllerAction.UpR:
				{
					return rInputState.ButtonDown(i, Buttons.RightThumbstickUp);
				}
				case EControllerAction.DownR:
				{
					return rInputState.ButtonDown(i, Buttons.RightThumbstickDown);
				}
				case EControllerAction.LeftR:
				{
					return rInputState.ButtonDown(i, Buttons.RightThumbstickLeft);
				}
				case EControllerAction.RightR:
				{
					return rInputState.ButtonDown(i, Buttons.RightThumbstickRight);
				}
				default:
				{
					//get the attack button to check
					Buttons mappedButton = g_ButtonMap[i, (iAction - EControllerAction.A)];
					return rInputState.ButtonDown(i, mappedButton);
				}
			}
		}

		/// <summary>
		/// Check whether the player released a mapped button this frame
		/// </summary>
		/// <param name="i">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button was deactivated this frame</returns>
		private bool CheckControllerActionReleased(InputState rInputState, int i, EControllerAction iAction)
		{
			Debug.Assert(iAction < EControllerAction.NumControllerActions);

			if (UseKeyboard && (iAction < EControllerAction.UpR))
			{
				//first do the keyboard check
				if (CheckKeyUp(rInputState, i, MappedKey(i, iAction)))
				{
					return true;
				}
			}

			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
				{
					return ((!rInputState.ButtonDown(i, Buttons.LeftThumbstickUp) &&
					         rInputState.PrevButtonDown(i, Buttons.LeftThumbstickUp)) ||
					        (!rInputState.ButtonDown(i, Buttons.DPadUp) &&
					         rInputState.PrevButtonDown(i, Buttons.DPadUp)));
				}
				case EControllerAction.Down:
				{
					return ((!rInputState.ButtonDown(i, Buttons.LeftThumbstickDown) &&
					         rInputState.PrevButtonDown(i, Buttons.LeftThumbstickDown)) ||
					        (!rInputState.ButtonDown(i, Buttons.DPadDown) &&
					         rInputState.PrevButtonDown(i, Buttons.DPadDown)));
				}
				case EControllerAction.Left:
				{
					return ((!rInputState.ButtonDown(i, Buttons.LeftThumbstickLeft) &&
					         rInputState.PrevButtonDown(i, Buttons.LeftThumbstickLeft)) ||
					        (!rInputState.ButtonDown(i, Buttons.DPadLeft) &&
					         rInputState.PrevButtonDown(i, Buttons.DPadLeft)));
				}
				case EControllerAction.Right:
				{
					return ((!rInputState.ButtonDown(i, Buttons.LeftThumbstickRight) &&
					         rInputState.PrevButtonDown(i, Buttons.LeftThumbstickRight)) ||
					        (!rInputState.ButtonDown(i, Buttons.DPadRight) &&
					         rInputState.PrevButtonDown(i, Buttons.DPadRight)));
				}
				case EControllerAction.UpR:
				{
					return (!rInputState.ButtonDown(i, Buttons.RightThumbstickUp) &&
							 rInputState.PrevButtonDown(i, Buttons.RightThumbstickUp));
				}
				case EControllerAction.DownR:
				{
					return (!rInputState.ButtonDown(i, Buttons.RightThumbstickDown) &&
							 rInputState.PrevButtonDown(i, Buttons.RightThumbstickDown));
				}
				case EControllerAction.LeftR:
				{
					return (!rInputState.ButtonDown(i, Buttons.RightThumbstickLeft) &&
							 rInputState.PrevButtonDown(i, Buttons.RightThumbstickLeft));
				}
				case EControllerAction.RightR:
				{
					return (!rInputState.ButtonDown(i, Buttons.RightThumbstickRight) &&
							 rInputState.PrevButtonDown(i, Buttons.RightThumbstickRight));
				}
				default:
				{
					//get the attack button to check
					Buttons mappedButton = g_ButtonMap[i, (iAction - EControllerAction.A)];
					return (!rInputState.ButtonDown(i, mappedButton) && rInputState.PrevButtonDown(i, mappedButton));
				}
			}
		}

		/// <summary>
		/// Check if a keyboard key was pressed this update
		/// </summary>
		/// <param name="rInputState">current input state</param>
		/// <param name="i">controller index</param>
		/// <param name="myKey">key to check</param>
		/// <returns>bool: key was pressed this update</returns>
		private bool CheckKeyDown(InputState rInputState, int i, Keys myKey)
		{
			if (UseKeyboard)
			{
				return (rInputState.CurrentKeyboardState.IsKeyDown(myKey) && rInputState.LastKeyboardState.IsKeyUp(myKey));
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// check if a key was released this update
		/// </summary>
		/// <param name="rInputState">current input state</param>
		/// <param name="i">controller index</param>
		/// <param name="myKey">key to check</param>
		/// <returns>bool: true if the key was released this update.</returns>
		private bool CheckKeyUp(InputState rInputState, int i, Keys myKey)
		{
			if (UseKeyboard)
			{
				return (rInputState.CurrentKeyboardState.IsKeyUp(myKey) && rInputState.LastKeyboardState.IsKeyDown(myKey));
			}
			else
			{
				return false;
			}
		}

		#endregion //Private Methods

		#endregion //Methods

		#region Networking

#if NETWORKING
		
		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			Thumbsticks.ReadFromNetwork(packetReader);

			//read in buttons
			for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			{
				ControllerActionPress[i] = packetReader.ReadBoolean();
			}

			//read in directions
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				ControllerActionHeld[i] = packetReader.ReadBoolean();
			}

			//read in released
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				ControllerActionRelease[i] = packetReader.ReadBoolean();
			}
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			Thumbsticks.WriteToNetwork(packetWriter);

			//write out buttons
			for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			{
				packetWriter.Write(ControllerActionPress[i]);
			}

			//write out directions
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				packetWriter.Write(ControllerActionHeld[i]);
			}

			//write out released
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				packetWriter.Write(ControllerActionRelease[i]);
			}
		}


#endif

		#endregion
	}
}