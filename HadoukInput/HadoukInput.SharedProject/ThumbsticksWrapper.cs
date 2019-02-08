
namespace HadoukInput
{
	/// <summary>
	/// An object for wrapping the thumbsticks
	/// </summary>
	public class ThumbsticksWrapper
	{
		#region Members

		/// <summary>
		/// the type of dead zone to use for thumbsticks
		/// </summary>
		private DeadZoneType _thumbstickScrubbing = DeadZoneType.Radial;

		/// <summary>
		/// how do we want to clean up thumbsticks?
		/// </summary>
		public DeadZoneType ThumbstickScrubbing
		{
			get
			{
				return _thumbstickScrubbing;
			}
			set
			{
				_thumbstickScrubbing = value;
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
		public ThumbsticksWrapper(ControllerWrapper controls)
		{
			LeftThumbstick = new ThumbstickWrapper(controls, true);
			RightThumbstick = new ThumbstickWrapper(controls, false);
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
		/// <param name="inputState"></param>
		/// <param name="i"></param>
		public void UpdateThumbsticks(InputState inputState, int i)
		{
			//update the left thumbstick
			LeftThumbstick.Update(inputState, i, inputState._currentGamePadStates[i].ThumbSticks.Left, true);
			RightThumbstick.Update(inputState, i, inputState._currentGamePadStates[i].ThumbSticks.Right, false);
		}

		#endregion //Methods
	}
}