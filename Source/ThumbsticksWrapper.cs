using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
		public DeadZoneType ThumbstickScrubbing
		{
			set
			{
				LeftThumbstick.ThumbstickScrubbing = value;
				RightThumbstick.ThumbstickScrubbing = value;
			}
		}

		/// <summary>
		/// The current direction of the left thumbstick, cleaned up however we want.
		/// </summary>
		public ThumbstickWrapper LeftThumbstick { get; protected set; }

		/// <summary>
		/// The current direction of the right thumbstick, cleaned up however we want.
		/// </summary>
		public ThumbstickWrapper RightThumbstick { get; protected set; }

		#endregion //Members

		#region Properties

		public float ThumbstickPower
		{
			set
			{
				LeftThumbstick.ThumbstickPower = value;
				RightThumbstick.ThumbstickPower = value;
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="HadoukInput.ThumbsticksWrapper"/> class.
		/// </summary>
		public ThumbsticksWrapper()
		{
			LeftThumbstick = new ThumbstickWrapper();
			RightThumbstick = new ThumbstickWrapper();
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public void Reset()
		{
			LeftThumbstick.Reset();
			RightThumbstick.Reset();
		}

		/// <summary>
		/// called each frame to update the thumbstick vector and the cleaned "direction" vector
		/// </summary>
		/// <param name="rInputState"></param>
		/// <param name="i"></param>
		public void UpdateThumbsticks(InputState rInputState, int i)
		{
			//update the left thumbstick
			LeftThumbstick.Update(rInputState, i, rInputState.m_CurrentGamePadStates[i].ThumbSticks.Left, true);
			RightThumbstick.Update(rInputState, i, rInputState.m_CurrentGamePadStates[i].ThumbSticks.Right, false);
		}

		#endregion //Methods

		#region Networking

#if NETWORKING
		
	/// <summary>
	/// Read this object from a network packet reader.
	/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			LeftThumbstick.ReadFromNetwork(packetReader);
			RightThumbstick.ReadFromNetwork(packetReader);
		}
		
		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			LeftThumbstick.WriteToNetwork(packetWriter);
			RightThumbstick.WriteToNetwork(packetWriter);
		}
		
#endif

		#endregion //Networking
	}
}