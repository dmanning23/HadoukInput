using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#if !NO_NETWORKING
using Microsoft.Xna.Framework.Net;
#endif

namespace HadoukInput
{
	/// <summary>
	/// An object for wrapping the thumbsticks
	/// </summary>
	public class ThumbsticksWrapper
	{
		#region Members

		/// <summary>
		/// how do we want to clean up thumbsticks?
		/// </summary>
		public DeadZoneType ThumbstickScrubbing { get; set; }
		
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
		
		/// <summary>
		/// the radius of the controller dead zone
		/// </summary>
		private float _deadZone;
		
		/// <summary>
		/// the square of the dead zone
		/// </summary>
		private float _deadZoneSquared;

		#endregion //Members

		#region Properties

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
		
		/// <summary>
		/// Gets or sets the size of the thumbstick dead zone.
		/// </summary>
		/// <value>The size of the dead zone.</value>
		public float DeadZone
		{
			get
			{
				return _deadZone;
			}
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
		private float DeadZoneSquared
		{
			get 
			{
				return _deadZoneSquared;
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="HadoukInput.ThumbsticksWrapper"/> class.
		/// </summary>
		public ThumbsticksWrapper()
		{
			DeadZone = 0.2f;
			ThumbstickScrubbing = DeadZoneType.Radial;
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public void Reset()
		{
			m_LeftThumbstickDirection = Vector2.Zero;
			m_RightThumbstickDirection = Vector2.Zero;
		}

		/// <summary>
		/// called each frame to update the thumbstick vector and the cleaned "direction" vector
		/// </summary>
		/// <param name="rInputState"></param>
		/// <param name="i"></param>
		public void UpdateThumbsticks(InputState rInputState, int i)
		{
			//update the left thumbstick
			UpdateSingleThumbstick(rInputState, i, ref m_LeftThumbstickDirection, rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left, true);
			//UpdateSingleThumbstick(rInputState, i, ref m_RightThumbstickDirection, rInputState.m_CurrentGamePadStates[i].ThumbSticks.Right, false);
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
			//TODO: this is to work around a monogame bug where the thumbsticks go from -2 - 2 instead of -1 - 1
			controllerThumbstick *= 0.5f;

			//first set the thumbstick to 0.  it will be set to the real value below
			myThumbstick = Vector2.Zero;

			//flag used to tell if direction is from thumbstick or not.  if this flag is true, apply the deadzone logic
			bool bThumbstickDirection = false;
			
			//if either of the axis are larger than 0.1, the length squared will be greater than 0.01
			if (controllerThumbstick.LengthSquared() >= DeadZoneSquared)
			{
				bThumbstickDirection = true;
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
					bThumbstickDirection = false;
					myThumbstick.Y = -1.0f;
				}
				else if (rInputState.m_CurrentGamePadStates[i].DPad.Down == ButtonState.Pressed)
				{
					//check down... 
					bThumbstickDirection = false;
					myThumbstick.Y = 1.0f;
				}
				
				if (rInputState.m_CurrentGamePadStates[i].DPad.Left == ButtonState.Pressed)
				{
					//check left
					bThumbstickDirection = false;
					myThumbstick.X = -1.0f;
				}
				else if (rInputState.m_CurrentGamePadStates[i].DPad.Right == ButtonState.Pressed)
				{
					//check right
					bThumbstickDirection = false;
					myThumbstick.X = 1.0f;
				}
				
				#if KEYBOARD
				//Check keyboard so we can test this stuff on computer
				if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Up))
				{
					//check up... 
					bThumbstickDirection = false;
					myThumbstick.Y = -1.0f;
				}
				else if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Down))
				{
					//check down... 
					bThumbstickDirection = false;
					myThumbstick.Y = 1.0f;
				}
				
				if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Left))
				{
					//check left
					bThumbstickDirection = false;
					myThumbstick.X = -1.0f;
				}
				else if (rInputState.m_CurrentKeyboardStates[i].IsKeyDown(Keys.Right))
				{
					//check right
					bThumbstickDirection = false;
					myThumbstick.X = 1.0f;
				}
				#endif
			}
			
			//do we need to apply dead zone scrubbing?
			if (bThumbstickDirection)
			{
				//are we doing scurbbing, or powercurving?
				switch (ThumbstickScrubbing)
				{
					case DeadZoneType.Axial:
					{
						//This will give us a really sticky controller that square gates all the time

						//First set the sticks to ignore the dead zone
						if(Math.Abs(myThumbstick.X) < DeadZone)
						{
							myThumbstick.X = 0.0f;
						}
						if(Math.Abs(myThumbstick.Y) < DeadZone)
						{
							myThumbstick.Y = 0.0f;
						}

						//Normalize the thumbsticks direction
						myThumbstick.Normalize();
					}
					break;

					case DeadZoneType.Radial:
					{
						//Radial just cares about the direction, so just normalize the stick
						myThumbstick.Normalize();
					}
					break;

					case DeadZoneType.ScaledRadial:
					{
						//this gives a nice linear thumbstick, starting at the deadzone
						Vector2 normalizedThumbstick = myThumbstick;
						normalizedThumbstick.Normalize();
						myThumbstick = normalizedThumbstick * ((myThumbstick.Length() - DeadZone) / (1 - DeadZone));
					}
					break;

					case DeadZoneType.PowerCurve:
					{
						//this gives a nice linear thumbstick, starting at the deadzone, but small values are smaller allowing for better precision
						Vector2 normalizedThumbstick = myThumbstick;
						normalizedThumbstick.Normalize();
						myThumbstick.X = PowerCurve(myThumbstick.X);
						myThumbstick.Y = PowerCurve(myThumbstick.Y);
					}
					break;
				}
			}

			//constrain the thumbstick to length of one
			if (myThumbstick.LengthSquared() > 1.0f)
			{
				myThumbstick.Normalize();
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

		#region Networking
		
		#if !NO_NETWORKING
		
		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			m_LeftThumbstickDirection = packetReader.ReadVector2();
			m_RightThumbstickDirection = packetReader.ReadVector2();
		}
		
		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			packetWriter.Write(m_LeftThumbstickDirection);
			packetWriter.Write(m_RightThumbstickDirection);
		}
		
		#endif
		
		#endregion //Networking
	}
}
