using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#if !NO_NETWORKING
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
		/// this is an intermediate step between buttons/directions and keystrokes
		/// </summary>
		private enum EControllerAction
		{
			Up,
			Down,
			Right,
			Left,
			A,
			B,
			X,
			Y,
			LShoulder,
			RShoulder,
			LTrigger,
			RTrigger,
			NumControllerActions
		}

		/// <summary>
		/// key mappings for all 4 controllers
		/// These can be changed to do button remapping...
		/// TODO: add a function to do button remapping:  should take an action & button, reset the actions mapped to that same button
		/// </summary>
		static public Buttons[,] g_KeyMap =
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

		#endregion //Mapped Keys

		#region Member Variables

		/// <summary>
		/// If this is a gamepad input, which gamepad is it?
		/// </summary>
		private PlayerIndex m_eGamePadIndex;

		///<summary>
		///flags for which actions are pressed
		///one flag for each controller action
		///</summary>
		private bool[] m_bControllerActionPress;

		/// <summary>
		/// list of the directions and if they are being held down
		/// only 4 flags, for each direction
		/// </summary>
		private bool[] m_bControllerActionHeld;

		/// <summary>
		/// list of flags for which actions have been released
		/// only 4 flags, for each direction
		/// </summary>
		private bool[] m_bControllerActionRelease;

		/// <summary>
		/// how do we want to clean up thumbsticks?
		/// </summary>
		public ThumbstickType ThumbstickScrubbing { get; set; }

		/// <summary>
		/// The current direction of the left thumbstick, cleaned up however we want.
		/// </summary>
		private Vector2 m_LeftThumbstickDirection;

		/// <summary>
		/// The current direction of the right thumbstick, cleaned up however we want.
		/// </summary>
		private Vector2 m_RightThumbstickDirection;

		/// <summary>
		/// constant value used to direct the thumbstick power curve
		/// between -3 -> 3
		/// </summary>
		private float m_fPower = 3.0f;

		#endregion //Member Variables

		#region Properties

		public bool[] ControllerActionHeld
		{
			get { return m_bControllerActionHeld; }
		}

		public Vector2 LeftThumbstickDirection
		{
			get { return m_LeftThumbstickDirection; }
		}

		public Vector2 RightThumbstickDirection
		{
			get { return m_RightThumbstickDirection; }
		}

		public float ThumbstickPower
		{
			get { return m_fPower; }
			set
			{
				if ((value <= 3.0f) && (value >= -3.0f))
				{
					m_fPower = value;
				}
			}
		}

		#endregion //Properties

		#region Initialization / Cleanup

		/// <summary>
		///	hello, standard constructor!
		/// </summary>
		/// <param name="iGamePadIndex">If this isn't a keyboard, which gamepad index it should use.</param>
		public ControllerWrapper(PlayerIndex? eGamePadIndex)
		{
			if (eGamePadIndex.HasValue)
			{
				m_eGamePadIndex = eGamePadIndex.Value;
			}

			m_bControllerActionPress = new bool[(int)EControllerAction.NumControllerActions];
			m_bControllerActionHeld = new bool[(int)EControllerAction.NumControllerActions];
			m_bControllerActionRelease = new bool[(int)EControllerAction.NumControllerActions];

			ThumbstickScrubbing = ThumbstickType.Scrubbed;

			//initialize input states
			ResetController();
		}

		/// <summary>
		/// Reset all the controls to null
		/// </summary>
		public void ResetController()
		{
			m_LeftThumbstickDirection = Vector2.Zero;
			m_RightThumbstickDirection = Vector2.Zero;

			for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			{
				m_bControllerActionPress[i] = false;
				m_bControllerActionHeld[i] = false;
				m_bControllerActionRelease[i] = false;
			}
		}
		
		#endregion //Initialization / Cleanup

		#region Methods

		/// <summary>
		/// update the current state of this controller interface
		/// </summary>
		/// <param name="rInputState">current state of all the input in the system</param>
		/// <param name="rParameters">current state of all the key mappings and shit</param>
		/// <param name="bFlip">true if the character this thing controls is facing left</param>
		public void Update(InputState rInputState)
		{
			int i = (int)m_eGamePadIndex;

			//update the thumbstick
			UpdateThumbsticks(rInputState, i);

			for (EControllerAction j = 0; j < EControllerAction.NumControllerActions; j++)
			{
				//update which buttons were presses this frame
				m_bControllerActionPress[(int)j] = CheckControllerActionPress(rInputState, i, j);

				//update which directions are held this frame
				m_bControllerActionHeld[(int)j] = CheckControllerActionHeld(rInputState, i, j);

				//update which dircetions are released this frame
				m_bControllerActionRelease[(int)j] = CheckControllerActionReleased(rInputState, i, j);
			}
		}

		/// <summary>
		/// called each frame to update the thumbstick vector and the cleaned "direction" vector
		/// </summary>
		/// <param name="rInputState"></param>
		/// <param name="i"></param>
		private void UpdateThumbsticks(InputState rInputState, int i)
		{
			//update the left thumbstick
			UpdateSingleThumbstick(rInputState, i, ref m_LeftThumbstickDirection, rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left, true);
			UpdateSingleThumbstick(rInputState, i, ref m_RightThumbstickDirection, rInputState.m_CurrentGamePadStates[i].ThumbSticks.Right, false);
		}

		/// <summary>
		/// Update one single thumbsticks.
		/// </summary>
		/// <param name="rInputState">The current input state.</param>
		/// <param name="i">controller index to check</param>
		/// <param name="myThumbstick">the vector to hold the scrubbed thumbstick direction</param>
		/// <param name="controllerThumbstick">the raw thumbstick input from the controller</param>
		/// <param name="bLeft">whether or not this thumbstick is the left</param>
		private void UpdateSingleThumbstick(InputState rInputState, int i, ref Vector2 myThumbstick, Vector2 controllerThumbstick, bool bLeft)
		{
			//flag used to tell if direction needs to be normalized
			bool bDirection = false;

			//if either of the axis are larger than 0.1, the length squared will be greater than 0.01
			if (controllerThumbstick.LengthSquared() >= 0.01f)
			{
				bDirection = true;
				myThumbstick = controllerThumbstick;

				//thumbstick needs to be flipped on Y to match screen coords
				myThumbstick.Y *= -1.0f;
			}

			//Check dpad if this is the left thumbstick
			if (bLeft)
			{
				if (rInputState.m_CurrentGamePadStates[i].DPad.Up == ButtonState.Pressed)
				{
					//check up... 
					bDirection = true;
					myThumbstick.Y = -1.0f;
				}
				else if (rInputState.m_CurrentGamePadStates[i].DPad.Down == ButtonState.Pressed)
				{
					//check down... 
					bDirection = true;
					myThumbstick.Y = 1.0f;
				}

				if (rInputState.m_CurrentGamePadStates[i].DPad.Left == ButtonState.Pressed)
				{
					//check left
					bDirection = true;
					myThumbstick.X = -1.0f;
				}
				else if (rInputState.m_CurrentGamePadStates[i].DPad.Right == ButtonState.Pressed)
				{
					//check right
					bDirection = true;
					myThumbstick.X = 1.0f;
				}

#if WINDOWS
				//Check keyboard so we can test this stuff on computer
				if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Up))
				{
					//check up... 
					bDirection = true;
					myThumbstick.Y = -1.0f;
				}
				else if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Down))
				{
					//check down... 
					bDirection = true;
					myThumbstick.Y = 1.0f;
				}

				if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Left))
				{
					//check left
					bDirection = true;
					myThumbstick.X = -1.0f;
				}
				else if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Right))
				{
					//check right
					bDirection = true;
					myThumbstick.X = 1.0f;
				}
#endif
			}

			//normalize, or set to 0
			if (bDirection)
			{
				//are we doing scurbbing, or powercurving?
				switch (ThumbstickScrubbing)
				{
					case ThumbstickType.PowerCurve:
						{
							myThumbstick.X += PowerCurve(myThumbstick.X);
							myThumbstick.Y += PowerCurve(myThumbstick.Y);
						}
						break;
					default:
						{
							myThumbstick.Normalize();
						}
						break;
				}
			}
			else
			{
				myThumbstick = Vector2.Zero;
			}
		}

		/// <summary>
		/// Check whether the player is hitting a mapped button
		/// </summary>
		/// <param name="i">controller index to check</param>
		/// <param name="iButton">the action to get the mapped button for</param>
		/// <returns>bool: whether or not that button was activated this frame</returns>
		private bool CheckControllerActionPress(InputState rInputState, int i, EControllerAction iAction)
		{
			Debug.Assert(iAction < EControllerAction.NumControllerActions);

#if WINDOWS
			//first do the keyboard check
			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
					{
						if (CheckKeyDown(rInputState, i, Keys.Up))
						{
							return true;
						}
					}
					break;
				case EControllerAction.Down:
					{
						if (CheckKeyDown(rInputState, i, Keys.Down))
						{
							return true;
						}
					}
					break;
				case EControllerAction.Left:
					{
						if (CheckKeyDown(rInputState, i, Keys.Left))
						{
							return true;
						}
					}
					break;
				case EControllerAction.Right:
					{
						if (CheckKeyDown(rInputState, i, Keys.Right))
						{
							return true;
						}
					}
					break;
				default:
					{
						//get the attack button to check
						Buttons mappedButton = g_KeyMap[i, (int)(iAction - EControllerAction.A)];
						switch (mappedButton)
						{
							case Buttons.A:
								{
									if (CheckKeyDown(rInputState, i, Keys.Z))
									{
										return true;
									}
								}
								break;
							case Buttons.B:
								{
									if (CheckKeyDown(rInputState, i, Keys.X))
									{
										return true;
									}
								}
								break;
							case Buttons.X:
								{
									if (CheckKeyDown(rInputState, i, Keys.A))
									{
										return true;
									}
								}
								break;
							case Buttons.Y:
								{
									if (CheckKeyDown(rInputState, i, Keys.S))
									{
										return true;
									}
								}
								break;
							case Buttons.LeftShoulder:
								{
									if (CheckKeyDown(rInputState, i, Keys.D))
									{
										return true;
									}
								}
								break;
							case Buttons.RightShoulder:
								{
									if (CheckKeyDown(rInputState, i, Keys.F))
									{
										return true;
									}
								}
								break;
							case Buttons.LeftTrigger:
								{
									if (CheckKeyDown(rInputState, i, Keys.C))
									{
										return true;
									}
								}
								break;
							case Buttons.RightTrigger:
								{
									if (CheckKeyDown(rInputState, i, Keys.V))
									{
										return true;
									}
								}
								break;
							default:
								{
									//wtf did u do
									Debug.Assert(false);
									return false;
								}
						}
					}
					break;
			}
#endif

			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
				{
					return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.Y > 0.1f) &&
							(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.Y <= 0.1f)) ||
							((rInputState.m_CurrentGamePadStates[i].DPad.Up == ButtonState.Pressed) &&
							(rInputState.m_LastGamePadStates[i].DPad.Up == ButtonState.Released));
				}
				case EControllerAction.Down:
				{
					return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.Y < -0.1f) &&
							(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.Y >= -0.1f)) ||
							((rInputState.m_CurrentGamePadStates[i].DPad.Down == ButtonState.Pressed) &&
							(rInputState.m_LastGamePadStates[i].DPad.Down == ButtonState.Released));
				}
				case EControllerAction.Left:
				{
					return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.X < -0.1f) &&
							(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.X >= -0.1f)) ||
							((rInputState.m_CurrentGamePadStates[i].DPad.Left == ButtonState.Pressed) &&
							(rInputState.m_LastGamePadStates[i].DPad.Left == ButtonState.Released));
				}
				case EControllerAction.Right:
				{
					return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.X > 0.1f) &&
							(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.X <= 0.1f)) ||
							((rInputState.m_CurrentGamePadStates[i].DPad.Right == ButtonState.Pressed) &&
							(rInputState.m_LastGamePadStates[i].DPad.Right == ButtonState.Released));
				}
				default:
				{
					//get the attack button to check
					Buttons mappedButton = g_KeyMap[i, (int)(iAction - EControllerAction.A)];
					return (rInputState.m_CurrentGamePadStates[i].IsButtonDown(mappedButton) && rInputState.m_LastGamePadStates[i].IsButtonUp(mappedButton));
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
#if WINDOWS
			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
				{
					if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Up))
					{
						return true;
					}
				}
				break;
				case EControllerAction.Down:
				{
					if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Down))
					{
						return true;
					}
				}
				break;
				case EControllerAction.Left:
				{
					if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Left))
					{
						return true;
					}
				}
				break;
				case EControllerAction.Right:
				{
					if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Right))
					{
						return true;
					}
				}
				break;
				default:
					{
						//get the attack button to check
						Buttons mappedButton = g_KeyMap[i, (int)(iAction - EControllerAction.A)];
						switch (mappedButton)
						{
							case Buttons.A:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Z))
									{
										return true;
									}
								}
								break;
							case Buttons.B:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.X))
									{
										return true;
									}
								}
								break;
							case Buttons.X:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.A))
									{
										return true;
									}
								}
								break;
							case Buttons.Y:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.S))
									{
										return true;
									}
								}
								break;
							case Buttons.LeftShoulder:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.D))
									{
										return true;
									}
								}
								break;
							case Buttons.RightShoulder:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.F))
									{
										return true;
									}
								}
								break;
							case Buttons.LeftTrigger:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.C))
									{
										return true;
									}
								}
								break;
							case Buttons.RightTrigger:
								{
									if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.V))
									{
										return true;
									}
								}
								break;
							default:
								{
									//wtf did u do
									Debug.Assert(false);
									return false;
								}
						}
					}
					break;
			}
#endif
			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
				{
					return (rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.Y > 0.1f) ||
						(rInputState.m_CurrentGamePadStates[i].DPad.Up == ButtonState.Pressed);
				}
				case EControllerAction.Down:
				{
					return (rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.Y < -0.1f) ||
						(rInputState.m_CurrentGamePadStates[i].DPad.Down == ButtonState.Pressed);
				}
				case EControllerAction.Left:
				{
					return (rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.X < -0.1f) ||
						(rInputState.m_CurrentGamePadStates[i].DPad.Left == ButtonState.Pressed);
				}
				case EControllerAction.Right:
				{
					return (rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.X > 0.1f) ||
						(rInputState.m_CurrentGamePadStates[i].DPad.Right == ButtonState.Pressed);
				}
				default:
				{
					//get the attack button to check
					Buttons mappedButton = g_KeyMap[i, (int)(iAction - EControllerAction.A)];
					return rInputState.m_CurrentGamePadStates[i].IsButtonDown(mappedButton);
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

#if WINDOWS
			//first do the keyboard check
			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
					{
						if (CheckKeyUp(rInputState, i, Keys.Up))
						{
							return true;
						}
					}
					break;
				case EControllerAction.Down:
					{
						if (CheckKeyUp(rInputState, i, Keys.Down))
						{
							return true;
						}
					}
					break;
				case EControllerAction.Left:
					{
						if (CheckKeyDown(rInputState, i, Keys.Left))
						{
							return true;
						}
					}
					break;
				case EControllerAction.Right:
					{
						if (CheckKeyUp(rInputState, i, Keys.Right))
						{
							return true;
						}
					}
					break;
				default:
					{
						//get the attack button to check
						Buttons mappedButton = g_KeyMap[i, (int)(iAction - EControllerAction.A)];
						switch (mappedButton)
						{
							case Buttons.A:
								{
									if (CheckKeyUp(rInputState, i, Keys.Z))
									{
										return true;
									}
								}
								break;
							case Buttons.B:
								{
									if (CheckKeyUp(rInputState, i, Keys.X))
									{
										return true;
									}
								}
								break;
							case Buttons.X:
								{
									if (CheckKeyUp(rInputState, i, Keys.A))
									{
										return true;
									}
								}
								break;
							case Buttons.Y:
								{
									if (CheckKeyUp(rInputState, i, Keys.S))
									{
										return true;
									}
								}
								break;
							case Buttons.LeftShoulder:
								{
									if (CheckKeyUp(rInputState, i, Keys.D))
									{
										return true;
									}
								}
								break;
							case Buttons.RightShoulder:
								{
									if (CheckKeyUp(rInputState, i, Keys.F))
									{
										return true;
									}
								}
								break;
							case Buttons.LeftTrigger:
								{
									if (CheckKeyUp(rInputState, i, Keys.C))
									{
										return true;
									}
								}
								break;
							case Buttons.RightTrigger:
								{
									if (CheckKeyUp(rInputState, i, Keys.V))
									{
										return true;
									}
								}
								break;
							default:
								{
									//wtf did u do
									Debug.Assert(false);
									return false;
								}
						}
					}
					break;
			}
#endif

			//First check if it is a direction
			switch (iAction)
			{
				case EControllerAction.Up:
					{
						return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.Y < 0.1f) &&
						(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.Y >= 0.1f)) ||
						((rInputState.m_CurrentGamePadStates[i].DPad.Up == ButtonState.Released) &&
						(rInputState.m_LastGamePadStates[i].DPad.Up == ButtonState.Pressed));
					}
				case EControllerAction.Down:
					{
						return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.Y > -0.1f) &&
						(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.Y <= -0.1f)) ||
						((rInputState.m_CurrentGamePadStates[i].DPad.Down == ButtonState.Released) &&
						(rInputState.m_LastGamePadStates[i].DPad.Down == ButtonState.Pressed));
					}
				case EControllerAction.Left:
					{
						return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.X > -0.1f) &&
						(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.X <= -0.1f)) ||
						((rInputState.m_CurrentGamePadStates[i].DPad.Left == ButtonState.Released) &&
						(rInputState.m_LastGamePadStates[i].DPad.Left == ButtonState.Pressed));
					}
				case EControllerAction.Right:
					{
						return ((rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left.X < 0.1f) &&
						(rInputState.m_LastGamePadStates[i].ThumbSticks.Left.X >= 0.1f)) ||
						((rInputState.m_CurrentGamePadStates[i].DPad.Right == ButtonState.Released) &&
						(rInputState.m_LastGamePadStates[i].DPad.Right == ButtonState.Pressed));
					}
				default:
					{
						//get the attack button to check
						Buttons mappedButton = g_KeyMap[i, (int)(iAction - EControllerAction.A)];
						return (rInputState.m_CurrentGamePadStates[i].IsButtonUp(mappedButton) && rInputState.m_LastGamePadStates[i].IsButtonDown(mappedButton));
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
			return (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(myKey) && rInputState.m_LastKeyboardStates[i].IsKeyUp(myKey));
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
			return (rInputState.m_CurrentKeyboardStates[i].IsKeyUp(Keys.Up) && rInputState.m_LastKeyboardStates[i].IsKeyDown(Keys.Up));
		}

		/// <summary>
		/// Run the thumbstick direction through a power curve to clean it up a bit.
		/// </summary>
		/// <param name="fValue">the value to run through the power curve, -1.0 - 1.0</param>
		/// <returns>float: the input value run through the power curve</returns>
		private float PowerCurve(float fValue)
		{
			return (float)System.Math.Pow(System.Math.Abs(fValue), m_fPower) * System.Math.Sign(fValue);
		}

		/// <summary>
		/// Check for a specific keystroke.
		/// </summary>
		/// <param name="eKeystroke">the keystroke to check for</param>
		/// <param name="bFlipped">Whether or not the check should be flipped on x axis.  If true, "left" will be "forward" and vice/versa</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke eKeystroke, bool bFlipped)
		{
			switch (eKeystroke)
			{
					//CHECK THE DIRECTIONS

				case EKeystroke.Up:
					{
						//get the direction to check for 'up'
						return m_bControllerActionHeld[(int)EControllerAction.Up];
					}
				case EKeystroke.Down:
					{
						//Don't send down if left or right are held... it pops really bad
						if (m_bControllerActionHeld[(int)EControllerAction.Left] || m_bControllerActionHeld[(int)EControllerAction.Right])
						{
							return false;
						}

						//get the direction to check for 'down'
						return m_bControllerActionHeld[(int)EControllerAction.Down];
					}
				case EKeystroke.Forward:
					{
						//Don't send left/right if up is held... it pops really bad
						if (m_bControllerActionHeld[(int)EControllerAction.Up])
						{
							return false;
						}

						//get the direction to check for 'forward'
						if (bFlipped)
						{
							return m_bControllerActionHeld[(int)EControllerAction.Left];
						}
						else
						{
							return m_bControllerActionHeld[(int)EControllerAction.Right];
						}
					}
				case EKeystroke.Back:
					{
						//Don't send left/right if up is held... it pops really bad
						if (m_bControllerActionHeld[(int)EControllerAction.Up])
						{
							return false;
						}

						//get the direction to check for 'Back'
						if (bFlipped)
						{
							return m_bControllerActionHeld[(int)EControllerAction.Right];
						}
						else
						{
							return m_bControllerActionHeld[(int)EControllerAction.Left];
						}
					}

					//CHECK DIRECTIONS RELEASED

				case EKeystroke.UpRelease:
					{
						//get the direction to check for 'up'
						return m_bControllerActionRelease[(int)EControllerAction.Up];
					}
				case EKeystroke.DownRelease:
					{
						//get the direction to check for 'down'
						return m_bControllerActionRelease[(int)EControllerAction.Down];
					}
				case EKeystroke.ForwardRelease:
					{
						//get the direction to check for 'forward'
						if (bFlipped)
						{
							return m_bControllerActionRelease[(int)EControllerAction.Left];
						}
						else
						{
							return m_bControllerActionRelease[(int)EControllerAction.Right];
						}
					}
				case EKeystroke.BackRelease:
					{
						//get the direction to check for 'back'
						if (bFlipped)
						{
							return m_bControllerActionRelease[(int)EControllerAction.Right];
						}
						else
						{
							return m_bControllerActionRelease[(int)EControllerAction.Left];
						}
					}

					//CHECK BUTTONS

				case EKeystroke.A:
					{
						return m_bControllerActionPress[(int)EControllerAction.A];
					}
				case EKeystroke.B:
					{
						return m_bControllerActionPress[(int)EControllerAction.B];
					}
				case EKeystroke.X:
					{
						return m_bControllerActionPress[(int)EControllerAction.X];
					}
				case EKeystroke.Y:
					{
						return m_bControllerActionPress[(int)EControllerAction.Y];
					}
				case EKeystroke.LShoulder:
					{
						return m_bControllerActionPress[(int)EControllerAction.LShoulder];
					}
				case EKeystroke.RShoulder:
					{
						return m_bControllerActionPress[(int)EControllerAction.RShoulder];
					}
				case EKeystroke.LTrigger:
					{
						return m_bControllerActionPress[(int)EControllerAction.LTrigger];
					}
				case EKeystroke.RTrigger:
					{
						return m_bControllerActionPress[(int)EControllerAction.RTrigger];
					}

					//CHECK BUTTONS RELEASED

				case EKeystroke.ARelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.A];
					}
				case EKeystroke.BRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.B];
					}
				case EKeystroke.XRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.X];
					}
				case EKeystroke.YRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.Y];
					}
				case EKeystroke.LShoulderRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.LShoulder];
					}
				case EKeystroke.RShoulderRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.RShoulder];
					}
				case EKeystroke.LTriggerRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.LTrigger];
					}
				case EKeystroke.RTriggerRelease:
					{
						return m_bControllerActionRelease[(int)EControllerAction.RTrigger];
					}
				
				default:
					{
						//you passed in one of the direction+button keystrokes?
						Debug.Assert(false);
						return false;
					}
			}
		}

		#endregion //Methods

		#region Networking

#if !NO_NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			m_LeftThumbstickDirection = packetReader.ReadVector2();
			m_RightThumbstickDirection = packetReader.ReadVector2();

			//read in buttons
			for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			{
				m_bControllerActionPress[i] = packetReader.ReadBoolean();
			}

			//read in directions
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				m_bControllerActionHeld[i] = packetReader.ReadBoolean();
			}

			//read in released
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				m_bControllerActionRelease[i] = packetReader.ReadBoolean();
			}
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			packetWriter.Write(m_LeftThumbstickDirection);
			packetWriter.Write(m_RightThumbstickDirection);

			//write out buttons
			for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			{
				packetWriter.Write(m_bControllerActionPress[i]);
			}

			//write out directions
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				packetWriter.Write(m_bControllerActionHeld[i]);
			}

			//write out released
			for (int i = 0; i < (int)EControllerAction.X; i++)
			{
				packetWriter.Write(m_bControllerActionRelease[i]);
			}
		}

#endif

		#endregion //Networking
	}
}