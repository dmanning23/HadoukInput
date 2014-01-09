using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

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
		private Vector2 m_PrevDirection;

		/// <summary>
		/// The current direction of the thumbstick, cleaned up however we want.
		/// </summary>
		private Vector2 m_Direction;

		/// <summary>
		/// constant value used to direct the thumbstick power curve
		/// between -3 -> 3
		/// </summary>
		private float m_fPower = 3.0f;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="HadoukInput.ControllerWrapper"/> also uses keyboard.
		/// </summary>
		/// <value><c>true</c> if use keyboard; otherwise, <c>false</c>.</value>
		public bool UseKeyboard { get; set; }

		private const float SquareRoot2DividedBy2 = 0.70710678118654752440084436210485f;

		#endregion //Members

		#region Properties

		public Vector2 Direction
		{
			get { return m_Direction; }
		}

		public Vector2 PrevDirection
		{
			get { return m_PrevDirection; }
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

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="HadoukInput.ThumbsticksWrapper"/> class.
		/// </summary>
		public ThumbstickWrapper()
		{
			ThumbstickScrubbing = DeadZoneType.Radial;
			Reset();
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public void Reset()
		{
			m_Direction = Vector2.Zero;
			m_PrevDirection = Vector2.Zero;
		}

		/// <summary>
		/// Update one single thumbsticks.
		/// </summary>
		/// <param name="rInputState">The current input state.</param>
		/// <param name="i">controller index to check</param>
		/// <param name="controllerThumbstick">the raw thumbstick input from the controller</param>
		/// <param name="bLeft">whether or not this thumbstick is the left</param>
		public void Update(InputState rInputState, int i, Vector2 controllerThumbstick, bool bLeft)
		{
			//First set the prev direction
			m_PrevDirection = m_Direction;

			//first set the thumbstick to 0.  it will be set to the real value below
			m_Direction = Vector2.Zero;

			//flag used to tell if direction is from thumbstick or not.  if this flag is true, apply the deadzone logic
			bool bThumbstickDirection = true;

			//Check dpad if this is the left thumbstick
			if (bLeft)
			{
				//Check keyboard so we can test this stuff on computer
				if (ButtonState.Pressed == rInputState.m_CurrentGamePadStates[i].DPad.Up)
				{
					//check up... 
					bThumbstickDirection = false;
					#if OUYA
					//godammit, y axis is backwards on the dpad but not the thumbstick
					m_Direction.Y = -1.0f;
					#else
					m_Direction.Y += 1.0f;
					#endif
				}
				else if (ButtonState.Pressed == rInputState.m_CurrentGamePadStates[i].DPad.Down)
				{
					//check down... 
					bThumbstickDirection = false;
					#if OUYA
					//godammit, y axis is backwards on the dpad but not the thumbstick
					m_Direction.Y = 1.0f;
					#else
					m_Direction.Y -= 1.0f;
					#endif
				}

				if (ButtonState.Pressed == rInputState.m_CurrentGamePadStates[i].DPad.Left)
				{
					//check left
					bThumbstickDirection = false;
					m_Direction.X -= 1.0f;
				}
				else if (ButtonState.Pressed == rInputState.m_CurrentGamePadStates[i].DPad.Right)
				{
					//check right
					bThumbstickDirection = false;
					m_Direction.X += 1.0f;
				}

				//if we didnt find a direction and using the keyboard, check it now
				if (UseKeyboard && bThumbstickDirection)
				{
					//Check keyboard so we can test this stuff on computer
					if (rInputState.CurrentKeyboardState.IsKeyDown(Keys.Up))
					{
						//check up... 
						bThumbstickDirection = false;
						m_Direction.Y += 1.0f;
					}
					else if (rInputState.CurrentKeyboardState.IsKeyDown(Keys.Down))
					{
						//check down... 
						bThumbstickDirection = false;
						m_Direction.Y -= 1.0f;
					}
				
					if (rInputState.CurrentKeyboardState.IsKeyDown(Keys.Left))
					{
						//check left
						bThumbstickDirection = false;
						m_Direction.X -= 1.0f;
					}
					else if (rInputState.CurrentKeyboardState.IsKeyDown(Keys.Right))
					{
						//check right
						bThumbstickDirection = false;
						m_Direction.X += 1.0f;
					}
				}
			}

			//do we need to apply dead zone scrubbing?
			if (bThumbstickDirection)
			{
				//set the thumbstick to the real value
				m_Direction = controllerThumbstick;

				//are we doing scurbbing, or powercurving?
				switch (ThumbstickScrubbing)
				{
					case DeadZoneType.Axial:
					{
						//This will give us a really sticky controller that square gates all the time

						//First set the sticks to ignore the dead zone
						if (Math.Abs(m_Direction.X) < rInputState.DeadZone)
						{
							m_Direction.X = 0.0f;
						}
						if (Math.Abs(m_Direction.Y) < rInputState.DeadZone)
						{
							m_Direction.Y = 0.0f;
						}

						//Normalize the thumbsticks direction
						if (m_Direction.LengthSquared() != 0.0f)
						{
							m_Direction.Normalize();
						}
					}
						break;

					case DeadZoneType.Radial:
					{
						if (controllerThumbstick.LengthSquared() >= rInputState.DeadZoneSquared)
						{
							//Radial just cares about the direction, so just normalize the stick
							m_Direction.Normalize();
						}
						else
						{
							//stick is not outside the deadzone
							m_Direction = Vector2.Zero;
						}
					}
						break;

					case DeadZoneType.ScaledRadial:
					{
						if (controllerThumbstick.LengthSquared() >= rInputState.DeadZoneSquared)
						{
							//this gives a nice linear thumbstick, starting at the deadzone
							Vector2 normalizedThumbstick = m_Direction;
							normalizedThumbstick.Normalize();
							m_Direction = normalizedThumbstick * ((m_Direction.Length() - rInputState.DeadZone) / (1 - rInputState.DeadZone));
						}
						else
						{
							//stick is not outside the deadzone
							m_Direction = Vector2.Zero;
						}
					}
						break;

					case DeadZoneType.PowerCurve:
					{
						if (controllerThumbstick.LengthSquared() >= rInputState.DeadZoneSquared)
						{
							//this gives a nice linear thumbstick, starting at the deadzone, but small values are smaller allowing for better precision
							Vector2 normalizedThumbstick = m_Direction;
							normalizedThumbstick.Normalize();
							m_Direction.X = PowerCurve(m_Direction.X);
							m_Direction.Y = PowerCurve(m_Direction.Y);
						}
						else
						{
							//stick is not outside the deadzone
							m_Direction = Vector2.Zero;
						}
					}
						break;
				}
			}

			#if WINDOWS
			//thumbstick needs to be flipped on Y to match screen coords
			m_Direction.Y *= -1.0f;
			#endif

			//constrain the thumbstick to length of one
			if (m_Direction.LengthSquared() > 1.0f)
			{
				m_Direction.Normalize();
			}
		}

		/// <summary>
		/// Run the thumbstick direction through a power curve to clean it up a bit.
		/// </summary>
		/// <param name="fValue">the value to run through the power curve, -1.0 - 1.0</param>
		/// <returns>float: the input value run through the power curve</returns>
		private float PowerCurve(float fValue)
		{
			return (float)System.Math.Pow(System.Math.Abs(fValue), ThumbstickPower) * System.Math.Sign(fValue);
		}

		#endregion //Methods

		/// <summary>
		/// Check for a specific keystroke, but with a rotated direction.
		/// </summary>
		/// <param name="eKeystroke">the keystroke to check for</param>
		/// <param name="bFlipped">Whether or not the check should be flipped on x axis.  If true, "left" will be "forward" and vice/versa</param>
		/// <param name="direction">The NORMALIZED direction to check against</param>
		/// <returns>bool: the keystroke is being held</returns>
		public bool CheckKeystroke(EKeystroke eKeystroke, Vector2 direction, Vector2 upVect)
		{
			switch (eKeystroke)
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

				//For everything else, send to the other method
				default:
				{
					Debug.Assert(false);
					return false;
				}
			}
		}

		/// <summary>
		/// Check if two vectors are pointint in the same direction
		/// </summary>
		/// <param name="direction">the direction to check</param>
		/// <param name="controllerDirection">the direction the controller is pointed</param>
		/// <param name="SameDirection">true to check if they are poining in same direction, false to check for oppsite diurection</param>
		/// <returns></returns>
		private bool CheckDirectionHeld(Vector2 direction, Vector2 controllerDirection, bool bSameDirection)
		{
			//get the dot product of the directions
			float dot = Vector2.Dot(direction, controllerDirection);

			//check the correct direction
			if (bSameDirection)
			{
				//this magic number is squareroot2 / 2
				return (SquareRoot2DividedBy2 < dot);
			}
			else
			{
				return (-SquareRoot2DividedBy2 > dot);
			}
		}

		/// <summary>
		/// Check if a direction was just released
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="thumbstick"></param>
		/// <param name="bSameDirection"></param>
		/// <returns></returns>
		private bool CheckDirectionRelease(Vector2 direction, bool bSameDirection)
		{
			//was the direction held last time we checked?
			if (!CheckDirectionHeld(direction, PrevDirection, bSameDirection))
			{
				return false;
			}

			//It was held last time, if it isn't held now then it was a button release 
			return !CheckDirectionHeld(direction, Direction, bSameDirection);
		}

		#region Networking

#if NETWORKING
		
	/// <summary>
	/// Read this object from a network packet reader.
	/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			m_PrevDirection = packetReader.ReadVector2();
			m_Direction = packetReader.ReadVector2();
		}
		
		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			packetWriter.Write(m_PrevDirection);
			packetWriter.Write(m_Direction);
		}
		
#endif

		#endregion //Networking
	}
}