namespace HadoukInput
{
	/// <summary>
	/// This is a single directional/button press input from the player.
	/// </summary>
	public class InputItem
	{
		#region Properties

		/// <summary>
		/// The time that this input item was added to the input engine
		/// </summary>
		public float Time { get; set; }

		/// <summary>
		/// The keystroke that this input item represents
		/// </summary>
		public EKeystroke Keystroke { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Constructor!
		/// </summary>
		/// <param name="time">game time of the input creation</param>
		/// <param name="keystroke">keystroke of this input item</param>
		public InputItem(float time, EKeystroke keystroke)
		{
			Set(time, keystroke);
		}

		/// <summary>
		/// Set the parameters of this input item.
		/// Separate from the constructor so we can reuse these items in the factory.
		/// </summary>
		/// <param name="time">game time of the input creation</param>
		/// <param name="keystroke">keystroke of this input item</param>
		public void Set(float time, EKeystroke keystroke)
		{
			Time = time;
			Keystroke = keystroke;
		}

		#endregion //Methods
	}
}