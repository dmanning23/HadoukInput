using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HadoukInput
{
	/// <summary>
	/// This is an input state that also checks the mouse.
	/// </summary>
	public class InputMouseState : InputState
	{
		#region Properties

		public MouseState CurrentMouseState { get; private set; }

		public MouseState LastMouseState { get; private set; }

		/// <summary>
		/// Get the mouse position... only used in certain games
		/// </summary>
		public override Vector2 MousePos
		{
			get
			{
				return new Vector2((float)CurrentMouseState.X, (float)CurrentMouseState.Y);
			}
		}

		/// <summary>
		/// Check for left mouse click... only used in certain games
		/// </summary>
		public override bool LMouseClick
		{
			get
			{
				return ((CurrentMouseState.LeftButton == ButtonState.Pressed) &&
					(LastMouseState.LeftButton == ButtonState.Released));
			}
		}

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Constructs a new input state.
		/// </summary>
		public InputMouseState()
		{
			CurrentMouseState = new MouseState();
			LastMouseState = new MouseState();
		}

		#endregion //Initialization

		#region Public Methods

		/// <summary>
		/// Reads the latest state of the keyboard and gamepad.
		/// </summary>
		public override void Update()
		{
			base.Update();

			LastMouseState = CurrentMouseState;
			CurrentMouseState = Mouse.GetState();
		}

		#endregion //Public Methods
	}
}