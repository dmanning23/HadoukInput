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
		/// <param name="fTime">game time of the input creation</param>
		/// <param name="eKeystroke">keystroke of this input item</param>
		public InputItem(float fTime, EKeystroke eKeystroke)
		{
			Set(fTime, eKeystroke);
		}

		/// <summary>
		/// Set the parameters of this input item.
		/// Separate from the constructor so we can reuse these items in the factory.
		/// </summary>
		/// <param name="fTime">game time of the input creation</param>
		/// <param name="eKeystroke">keystroke of this input item</param>
		public void Set(float fTime, EKeystroke eKeystroke)
		{
			Time = fTime;
			Keystroke = eKeystroke;
		}

		#endregion //Methods
	}
}