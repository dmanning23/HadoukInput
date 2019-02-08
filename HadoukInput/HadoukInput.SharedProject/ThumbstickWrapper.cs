using MatrixExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace HadoukInput
{
	/// <summary>
	/// An object for wrapping a single thumbstick
	/// </summary>
	public class ThumbstickWrapper
	{
		#region Members

		/// <summary>
		/// how do we want to clean up thumbsticks?
		/// </summary>
		public DeadZoneType ThumbstickScrubbing { get; set; }

		/// <summary>
		/// The previous direction of the thumbstick, cleaned up however we want.
		/// </summary>
		private Vector2 _prevDirection;

		/// <summary>
		/// The current direction of the thumbstick, cleaned up however we want.
		/// </summary>
		private Vector2 _direction;

		/// <summary>
		/// constant value used to direct the thumbstick power curve
		/// between -3 -> 3
		/// </summary>
		private float _thumbstickPower = 3.0f;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="HadoukInput.ControllerWrapper"/> also uses keyboard.
		/// </summary>
		/// <value><c>true</c> if use keyboard; otherwise, <c>false</c>.</value>
		public ControllerWrapper Controller { get; set; }

		/// <summary>
		/// the dot product of the forward vector and a direction has to be greater than this number to count as "forward"
		/// </summary>
		private readonly float ForwardThreshold;

		#endregion //Members

		#region Properties

		/// <summary>
		/// The current direction of the thumbstick, dpad, and keyboard
		/// </summary>
		public Vector2 Direction
		{
			get { return _direction; }
		}

		public Vector2 PrevDirection
		{
			get { return _prevDirection; }
		}

		public float ThumbstickPower
		{
			get { return _thumbstickPower; }
			set
			{
				if ((value <= 3.0f) && (value >= -3.0f))
				{
					_thumbstickPower = value;
				}
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="HadoukInput.ThumbsticksWrapper"/> class.
		/// </summary>
		public ThumbstickWrapper(ControllerWrapper controls, bool squareGate)
		{
			//grba the controller 
			Controller = controls;

			//set up the forward threshold

			//get the forward vector
			var forwardVect = new Vector2(1.0f, 0.0f);

			//rotate the vector and get the min threshhold that counts as "forward"
			var rot = (squareGate ?
				MatrixExt.Orientation(MathHelper.ToRadians(45.0f)) :
				MatrixExt.Orientation(MathHelper.ToRadians(67.0f)));
			var threshholdVect = MatrixExt.Multiply(rot, forwardVect);
			ForwardThreshold = Vector2.Dot(forwardVect, threshholdVect);

			ThumbstickScrubbing = DeadZoneType.Radial;
			Reset();
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public void Reset()
		{
			_direction = Vector2.Zero;
			_prevDirection = Vector2.Zero;
		}

		/// <summary>
		/// Update one single thumbsticks.
		/// </summary>
		/// <param name="inputState">The current input state.</param>
		/// <param name="i">controller index to check</param>
		/// <param name="controllerThumbstick">the raw thumbstick input from the controller</param>
		/// <param name="left">whether or not this thumbstick is the left</param>
		public void Update(InputState inputState, int i, Vector2 controllerThumbstick, bool left)
		{
			//First set the prev direction
			_prevDirection = _direction;

			//first set the thumbstick to 0.  it will be set to the real value below
			_direction = Vector2.Zero;

			//flag used to tell if direction is from thumbstick or not.  if this flag is true, apply the deadzone logic
			var thumbstickDirection = true;

			//Check dpad if this is the left thumbstick
			if (left)
			{
				//Check keyboard so we can test this stuff on computer
				if (ButtonState.Pressed == inputState._currentGamePadStates[i].DPad.Up)
				{
					//check up... 
					thumbstickDirection = false;

					_direction.Y = 1.0f;
				}
				else if (ButtonState.Pressed == inputState._currentGamePadStates[i].DPad.Down)
				{
					//check down... 
					thumbstickDirection = false;
					_direction.Y = -1.0f;
				}

				if (ButtonState.Pressed == inputState._currentGamePadStates[i].DPad.Left)
				{
					//check left
					thumbstickDirection = false;
					_direction.X -= 1.0f;
				}
				else if (ButtonState.Pressed == inputState._currentGamePadStates[i].DPad.Right)
				{
					//check right
					thumbstickDirection = false;
					_direction.X += 1.0f;
				}

				//if we didnt find a direction and using the keyboard, check it now
				if (Controller.UseKeyboard && thumbstickDirection)
				{
					//Check keyboard so we can test this stuff on computer
					if (inputState.CurrentKeyboardState.IsKeyDown(Controller.MappedKey(i, EControllerAction.Up)))
					{
						//check up... 
						thumbstickDirection = false;
						_direction.Y += 1.0f;
					}
					else if (inputState.CurrentKeyboardState.IsKeyDown(Controller.MappedKey(i, EControllerAction.Down)))
					{
						//check down... 
						thumbstickDirection = false;
						_direction.Y -= 1.0f;
					}

					if (inputState.CurrentKeyboardState.IsKeyDown(Controller.MappedKey(i, EControllerAction.Left)))
					{
						//check left
						thumbstickDirection = false;
						_direction.X -= 1.0f;
					}
					else if (inputState.CurrentKeyboardState.IsKeyDown(Controller.MappedKey(i, EControllerAction.Right)))
					{
						//check right
						thumbstickDirection = false;
						_direction.X += 1.0f;
					}
				}
			}

			//do we need to apply dead zone scrubbing?
			if (thumbstickDirection)
			{
				//set the thumbstick to the real value
				_direction = controllerThumbstick;

				//are we doing scurbbing, or powercurving?
				switch (ThumbstickScrubbing)
				{
					case DeadZoneType.Axial:
						{
							//This will give us a really sticky controller that square gates all the time

							//First set the sticks to ignore the dead zone
							if (Math.Abs(_direction.X) < inputState.DeadZone)
							{
								_direction.X = 0.0f;
							}
							if (Math.Abs(_direction.Y) < inputState.DeadZone)
							{
								_direction.Y = 0.0f;
							}

							//Normalize the thumbsticks direction
							if (_direction.LengthSquared() != 0.0f)
							{
								_direction.Normalize();
							}
						}
						break;

					case DeadZoneType.Radial:
						{
							if (controllerThumbstick.LengthSquared() >= inputState.DeadZoneSquared)
							{
								//Radial just cares about the direction, so just normalize the stick
								_direction.Normalize();
							}
							else
							{
								//stick is not outside the deadzone
								_direction = Vector2.Zero;
							}
						}
						break;

					case DeadZoneType.ScaledRadial:
						{
							if (controllerThumbstick.LengthSquared() >= inputState.DeadZoneSquared)
							{
								//this gives a nice linear thumbstick, starting at the deadzone
								var normalizedThumbstick = _direction;
								normalizedThumbstick.Normalize();
								_direction = normalizedThumbstick * ((_direction.Length() - inputState.DeadZone) / (1 - inputState.DeadZone));
							}
							else
							{
								//stick is not outside the deadzone
								_direction = Vector2.Zero;
							}
						}
						break;

					case DeadZoneType.PowerCurve:
						{
							if (controllerThumbstick.LengthSquared() >= inputState.DeadZoneSquared)
							{
								//this gives a nice linear thumbstick, starting at the deadzone, but small values are smaller allowing for better precision
								var normalizedThumbstick = _direction;
								normalizedThumbstick.Normalize();
								_direction.X = PowerCurve(_direction.X);
								_direction.Y = PowerCurve(_direction.Y);
							}
							else
							{
								//stick is not outside the deadzone
								_direction = Vector2.Zero;
							}
						}
						break;
				}
			}

			//constrain the thumbstick to length of one
			if (_direction.LengthSquared() > 1.0f)
			{
				_direction.Normalize();
			}
		}

		/// <summary>
		/// Run the thumbstick direction through a power curve to clean it up a bit.
		/// </summary>
		/// <param name="initialValue">the value to run through the power curve, -1.0 - 1.0</param>
		/// <returns>float: the input value run through the power curve</returns>
		private float PowerCurve(float initialValue)
		{
			return (float)System.Math.Pow(System.Math.Abs(initialValue), ThumbstickPower) * System.Math.Sign(initialValue);
		}

		/// <summary>
		/// Check for a specific keystroke, but with a rotated direction.
		/// </summary>
		/// <param name="keystroke">the keystroke to check for</param>
		/// <param name="direction">The NORMALIZED direction to check against</param>
		/// <param name="upVect">The NORMALIZED up vector from the direciont</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke keystroke, Vector2 direction, Vector2 upVect)
		{
			switch (keystroke)
			{
				//CHECK THE DIRECTIONS

				case EKeystroke.Up:
					{
						//get the direction to check for 'up'
						return CheckDirectionHeld(upVect, Direction, true);
					}
				case EKeystroke.Down:
					{
						//Don't send down if left or right are held... it pops really bad
						if (CheckDirectionHeld(direction, Direction, true) ||
							CheckDirectionHeld(direction, Direction, false))
						{
							return false;
						}

						//get the direction to check for 'down'
						return CheckDirectionHeld(upVect, Direction, false);
					}
				case EKeystroke.Forward:
					{
						//Don't send left/right if up is held... it pops really bad
						if (CheckDirectionHeld(upVect, Direction, true))
						{
							return false;
						}

						//get the direction to check for 'forward'
						return CheckDirectionHeld(direction, Direction, true);
					}
				case EKeystroke.Back:
					{
						//Don't send left/right if up is held... it pops really bad
						if (CheckDirectionHeld(upVect, Direction, true))
						{
							return false;
						}

						//get the direction to check for 'Back'
						return CheckDirectionHeld(direction, Direction, false);
					}

				case EKeystroke.UpR:
					{
						//get the direction to check for 'up'
						return CheckDirectionHeld(upVect, Direction, true);
					}
				case EKeystroke.DownR:
					{
						//get the direction to check for 'down'
						return CheckDirectionHeld(upVect, Direction, false);
					}
				case EKeystroke.ForwardR:
					{
						//get the direction to check for 'forward'
						return CheckDirectionHeld(direction, Direction, true);
					}
				case EKeystroke.BackR:
					{
						//get the direction to check for 'Back'
						return CheckDirectionHeld(direction, Direction, false);
					}

				//For everything else, send to the other method
				default:
					{
						return false;
					}
			}
		}

		/// <summary>
		/// Check if two vectors are pointint in the same direction
		/// </summary>
		/// <param name="direction">the direction to check</param>
		/// <param name="controllerDirection">the direction the controller is pointed</param>
		/// <param name="sameDirection">true to check if they are poining in same direction, false to check for oppsite diurection</param>
		/// <returns></returns>
		private bool CheckDirectionHeld(Vector2 direction, Vector2 controllerDirection, bool sameDirection)
		{
			//get the dot product of the directions
			float dot = Vector2.Dot(direction, controllerDirection);

			//check the correct direction
			if (sameDirection)
			{
				//this magic number is squareroot2 / 2
				return (ForwardThreshold <= dot);
			}
			else
			{
				return (-ForwardThreshold >= dot);
			}
		}

		#endregion //Methods
	}
}