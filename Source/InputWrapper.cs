using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace HadoukInput
{
	/// <summary>
	/// A delegate to a method that takes a message name and spits out an integer ID
	/// </summary>
	/// <param name="strMessageName">name of the message to get ID for</param>
	/// <returns>ID of that message</returns>
	public delegate int MessageNameToID(string strMessageName);

	/// <summary>
	/// A delegate to get the current time.
	/// </summary>
	/// <returns>float: the current time in seconds.</returns>
	public delegate float CurrentTime();

	/// <summary>
	/// This is the guts of the input lib... 
	/// This class queues up input from the player, condenses input into the correct keystrokes, and parses queued input to look for patterns.
	/// </summary>
	public class InputWrapper
	{
		#region Members

		/// <summary>
		/// state machine for combining the keystrokes of input items
		/// This is an 2d array that takes 2 keystrokes and returns the keystroke that results from combining them
		/// </summary>
		private static readonly EKeystroke[,] g_InputTransitions;

		/// <summary>
		/// Number of rows in the input transitions table
		/// </summary>
		private const int TransitionsRowSize = (int)EKeystroke.DownR + 1;

		/// <summary>
		/// number of columns in the intpu transistions table
		/// </summary>
		private const int TransitionsColumnSize = (int)EKeystroke.DownBackR + 1;

		/// <summary>
		/// Callback to a method to get the current time, used to time the input
		/// </summary>
		private readonly CurrentTime GetCurrentTime;

		/// <summary>
		/// The controller this dude will use
		/// </summary>
		private readonly ControllerWrapper m_Controller;

		/// <summary>
		/// The move tree, which acutally holds the wholle move list
		/// </summary>
		private readonly MoveList Moves;

		/// <summary>
		/// This is the buffer for input before it is put in the listInput
		/// this allows for simultaneous button presses
		/// Input is held in here for a split second, condensed into keystrokes as they come in, then put in the queue.
		/// </summary>
		private readonly List<InputItem> m_listBufferedInput;

		/// <summary>
		/// list of queued input
		/// This is used to look for patterns as loaded from the move list.
		/// Input is held in here for a little bit while it parses for moves.
		/// </summary>
		private readonly List<InputItem> m_listQueuedInput;

		/// <summary>
		/// A dictionary of all the keystroke combinations
		/// One row for every keystroks
		/// The columns are all the keys that are part of that keystroke
		/// </summary>
		private static readonly EKeystroke[][] g_KeystrokeCombinations;

		#endregion //Members

		#region Properties

		public ControllerWrapper Controller
		{
			get { return m_Controller; }
		}

		/// <summary>
		/// Length of time input items are held in the buffer before being put in the queue
		/// </summary>
		public float BufferedInputExpire { get; set; }

		/// <summary>
		/// Length of time input items are held in the queue before they are discarded.
		/// </summary>
		public float QueuedInputExpire { get; set; }

		#endregion //Properties

		#region Methods

		#region Initialization 

		/// <summary>
		/// Setup the state machine for combining input
		/// </summary>
		static InputWrapper()
		{
			//Only do transitions for the directions
			g_InputTransitions = new EKeystroke[TransitionsRowSize, TransitionsColumnSize];

			//create the table
			g_KeystrokeCombinations = new EKeystroke[(int)EKeystroke.NumKeystrokes][];

			SetupTransitionTable();
			SetupKeystrokeCombinationTable();
		}

		/// <summary>
		/// setup the state machine for doing input transitions
		/// </summary>
		private static void SetupTransitionTable()
		{
			//set all the keystrokes to default to the row item
			for (int i = 0; i < TransitionsRowSize; i++)
			{
				for (int j = 0; j < TransitionsColumnSize; j++)
				{
					g_InputTransitions[i, j] = (EKeystroke)i;
				}
			}

			//set the direction + button transition
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.A] = EKeystroke.AUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.B] = EKeystroke.BUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.X] = EKeystroke.XUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.Y] = EKeystroke.YUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.LShoulder] = EKeystroke.LShoulderUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.RShoulder] = EKeystroke.RShoulderUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.LTrigger] = EKeystroke.LTriggerUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.RTrigger] = EKeystroke.RTriggerUp;

			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.A] = EKeystroke.ADown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.B] = EKeystroke.BDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.X] = EKeystroke.XDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.Y] = EKeystroke.YDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.LShoulder] = EKeystroke.LShoulderDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.RShoulder] = EKeystroke.RShoulderDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.LTrigger] = EKeystroke.LTriggerDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.RTrigger] = EKeystroke.RTriggerDown;

			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.A] = EKeystroke.AForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.B] = EKeystroke.BForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.X] = EKeystroke.XForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.Y] = EKeystroke.YForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.LShoulder] = EKeystroke.LShoulderForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.RShoulder] = EKeystroke.RShoulderForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.LTrigger] = EKeystroke.LTriggerForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.RTrigger] = EKeystroke.RTriggerForward;

			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.A] = EKeystroke.ABack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.B] = EKeystroke.BBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.X] = EKeystroke.XBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.Y] = EKeystroke.YBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.LShoulder] = EKeystroke.LShoulderBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.RShoulder] = EKeystroke.RShoulderBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.LTrigger] = EKeystroke.LTriggerBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.RTrigger] = EKeystroke.RTriggerBack;

			g_InputTransitions[(int)EKeystroke.UpR, (int)EKeystroke.ForwardR] = EKeystroke.UpForwardR;
			g_InputTransitions[(int)EKeystroke.UpR, (int)EKeystroke.BackR] = EKeystroke.UpBackR;
			g_InputTransitions[(int)EKeystroke.DownR, (int)EKeystroke.ForwardR] = EKeystroke.DownForwardR;
			g_InputTransitions[(int)EKeystroke.DownR, (int)EKeystroke.BackR] = EKeystroke.DownBackR;

			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.UpR] = EKeystroke.UpUp;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.UpForwardR] = EKeystroke.UpUpForward;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.ForwardR] = EKeystroke.UpForward;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.DownForwardR] = EKeystroke.UpDownForward;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.DownR] = EKeystroke.UpDown;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.DownBackR] = EKeystroke.UpDownBack;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.BackR] = EKeystroke.UpBack;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.UpBackR] = EKeystroke.UpUpBack;
			g_InputTransitions[(int)EKeystroke.Up, (int)EKeystroke.NeutralR] = EKeystroke.UpNeutral;

			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.UpR] = EKeystroke.DownUp;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.UpForwardR] = EKeystroke.DownUpForward;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.ForwardR] = EKeystroke.DownForward;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.DownForwardR] = EKeystroke.DownDownForward;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.DownR] = EKeystroke.DownDown;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.DownBackR] = EKeystroke.DownDownBack;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.BackR] = EKeystroke.DownBack;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.UpBackR] = EKeystroke.DownUpBack;
			g_InputTransitions[(int)EKeystroke.Down, (int)EKeystroke.NeutralR] = EKeystroke.DownNeutral;

			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.UpR] = EKeystroke.ForwardUp;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.UpForwardR] = EKeystroke.ForwardUpForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.ForwardR] = EKeystroke.ForwardForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.DownForwardR] = EKeystroke.ForwardDownForward;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.DownR] = EKeystroke.ForwardDown;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.DownBackR] = EKeystroke.ForwardDownBack;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.BackR] = EKeystroke.ForwardBack;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.UpBackR] = EKeystroke.ForwardUpBack;
			g_InputTransitions[(int)EKeystroke.Forward, (int)EKeystroke.NeutralR] = EKeystroke.ForwardNeutral;

			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.UpR] = EKeystroke.BackUp;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.UpForwardR] = EKeystroke.BackUpForward;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.ForwardR] = EKeystroke.BackForward;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.DownForwardR] = EKeystroke.BackDownForward;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.DownR] = EKeystroke.BackDown;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.DownBackR] = EKeystroke.BackDownBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.BackR] = EKeystroke.BackBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.UpBackR] = EKeystroke.BackUpBack;
			g_InputTransitions[(int)EKeystroke.Back, (int)EKeystroke.NeutralR] = EKeystroke.BackNeutral;

			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.UpR] = EKeystroke.NeutralUp;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.UpForwardR] = EKeystroke.NeutralUpForward;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.ForwardR] = EKeystroke.NeutralForward;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.DownForwardR] = EKeystroke.NeutralDownForward;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.DownR] = EKeystroke.NeutralDown;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.DownBackR] = EKeystroke.NeutralDownBack;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.BackR] = EKeystroke.NeutralBack;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.UpBackR] = EKeystroke.NeutralUpBack;
			g_InputTransitions[(int)EKeystroke.Neutral, (int)EKeystroke.NeutralR] = EKeystroke.NeutralNeutral;
		}

		/// <summary>
		/// Setup the table with all the keystroke combinations in it
		/// </summary>
		private static void SetupKeystrokeCombinationTable()
		{
			g_KeystrokeCombinations[(int)EKeystroke.Up] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.Down] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.Forward] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.Back] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.Neutral] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.UpR] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.DownR] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardR] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.BackR] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralR] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.A] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.B] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.X] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.Y] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.LShoulder] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.RShoulder] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.LTrigger] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.RTrigger] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.ARelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.BRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.XRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.YRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.LShoulderRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.RShoulderRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.LTriggerRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.RTriggerRelease] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardShoulder] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.BackShoulder] = new EKeystroke[] { };
			g_KeystrokeCombinations[(int)EKeystroke.UpForwardR] = new EKeystroke[] { EKeystroke.UpR, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.DownForwardR] = new EKeystroke[] { EKeystroke.DownR, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.UpBackR] = new EKeystroke[] { EKeystroke.UpR, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.DownBackR] = new EKeystroke[] { EKeystroke.DownR, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.AUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.A };
			g_KeystrokeCombinations[(int)EKeystroke.ADown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.A };
			g_KeystrokeCombinations[(int)EKeystroke.AForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.A };
			g_KeystrokeCombinations[(int)EKeystroke.ABack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.A };
			g_KeystrokeCombinations[(int)EKeystroke.BUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.B };
			g_KeystrokeCombinations[(int)EKeystroke.BDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.B };
			g_KeystrokeCombinations[(int)EKeystroke.BForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.B };
			g_KeystrokeCombinations[(int)EKeystroke.BBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.B };
			g_KeystrokeCombinations[(int)EKeystroke.XUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.X };
			g_KeystrokeCombinations[(int)EKeystroke.XDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.X };
			g_KeystrokeCombinations[(int)EKeystroke.XForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.X };
			g_KeystrokeCombinations[(int)EKeystroke.XBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.X };
			g_KeystrokeCombinations[(int)EKeystroke.YUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.Y };
			g_KeystrokeCombinations[(int)EKeystroke.YDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.Y };
			g_KeystrokeCombinations[(int)EKeystroke.YForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.Y };
			g_KeystrokeCombinations[(int)EKeystroke.YBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.Y };
			g_KeystrokeCombinations[(int)EKeystroke.LShoulderUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.LShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.LShoulderDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.LShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.LShoulderForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.LShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.LShoulderBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.LShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.RShoulderUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.RShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.RShoulderDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.RShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.RShoulderForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.RShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.RShoulderBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.RShoulder };
			g_KeystrokeCombinations[(int)EKeystroke.LTriggerUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.LTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.LTriggerDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.LTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.LTriggerForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.LTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.LTriggerBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.LTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.RTriggerUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.RTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.RTriggerDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.RTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.RTriggerForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.RTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.RTriggerBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.RTrigger };
			g_KeystrokeCombinations[(int)EKeystroke.UpUp] = new EKeystroke[] { EKeystroke.Up, EKeystroke.UpR };
			g_KeystrokeCombinations[(int)EKeystroke.UpUpForward] = new EKeystroke[] { EKeystroke.Up, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.UpForward] = new EKeystroke[] { EKeystroke.Up, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.UpDownForward] = new EKeystroke[] { EKeystroke.Up, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.UpDown] = new EKeystroke[] { EKeystroke.Up, EKeystroke.DownR };
			g_KeystrokeCombinations[(int)EKeystroke.UpDownBack] = new EKeystroke[] { EKeystroke.Up, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR };
			g_KeystrokeCombinations[(int)EKeystroke.UpBack] = new EKeystroke[] { EKeystroke.Up, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.UpUpBack] = new EKeystroke[] { EKeystroke.Up, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR };
			g_KeystrokeCombinations[(int)EKeystroke.UpNeutral] = new EKeystroke[] { EKeystroke.Up, EKeystroke.NeutralR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardUp] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.UpR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardUpForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardDownForward] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardDown] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.DownR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardDownBack] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardBack] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardUpBack] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR };
			g_KeystrokeCombinations[(int)EKeystroke.ForwardNeutral] = new EKeystroke[] { EKeystroke.Forward, EKeystroke.NeutralR };
			g_KeystrokeCombinations[(int)EKeystroke.DownUp] = new EKeystroke[] { EKeystroke.Down, EKeystroke.UpR };
			g_KeystrokeCombinations[(int)EKeystroke.DownUpForward] = new EKeystroke[] { EKeystroke.Down, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.DownForward] = new EKeystroke[] { EKeystroke.Down, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.DownDownForward] = new EKeystroke[] { EKeystroke.Down, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.DownDown] = new EKeystroke[] { EKeystroke.Down, EKeystroke.DownR };
			g_KeystrokeCombinations[(int)EKeystroke.DownDownBack] = new EKeystroke[] { EKeystroke.Down, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR };
			g_KeystrokeCombinations[(int)EKeystroke.DownBack] = new EKeystroke[] { EKeystroke.Down, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.DownUpBack] = new EKeystroke[] { EKeystroke.Down, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR };
			g_KeystrokeCombinations[(int)EKeystroke.DownNeutral] = new EKeystroke[] { EKeystroke.Down, EKeystroke.NeutralR };
			g_KeystrokeCombinations[(int)EKeystroke.BackUp] = new EKeystroke[] { EKeystroke.Back, EKeystroke.UpR };
			g_KeystrokeCombinations[(int)EKeystroke.BackUpForward] = new EKeystroke[] { EKeystroke.Back, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.BackForward] = new EKeystroke[] { EKeystroke.Back, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.BackDownForward] = new EKeystroke[] { EKeystroke.Back, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.BackDown] = new EKeystroke[] { EKeystroke.Back, EKeystroke.DownR };
			g_KeystrokeCombinations[(int)EKeystroke.BackDownBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR };
			g_KeystrokeCombinations[(int)EKeystroke.BackBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.BackUpBack] = new EKeystroke[] { EKeystroke.Back, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR };
			g_KeystrokeCombinations[(int)EKeystroke.BackNeutral] = new EKeystroke[] { EKeystroke.Back, EKeystroke.NeutralR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralUp] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.UpR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralUpForward] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.UpR, EKeystroke.ForwardR, EKeystroke.UpForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralForward] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.ForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralDownForward] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.DownR, EKeystroke.ForwardR, EKeystroke.DownForwardR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralDown] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.DownR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralDownBack] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.DownR, EKeystroke.BackR, EKeystroke.DownBackR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralBack] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.BackR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralUpBack] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.UpR, EKeystroke.BackR, EKeystroke.UpBackR };
			g_KeystrokeCombinations[(int)EKeystroke.NeutralNeutral] = new EKeystroke[] { EKeystroke.Neutral, EKeystroke.NeutralR };
		}

		/// <summary>
		/// contructor
		/// </summary>
		/// <param name="controller">index of the controller this player will use</param>
		/// <param name="rClock">The external clock that will be used to time this dude.  This guy doesn't update his own timer!</param>
		public InputWrapper(ControllerWrapper controller, CurrentTime rClock)
		{
			Debug.Assert(null != rClock);

			BufferedInputExpire = 0.05f;
			QueuedInputExpire = 0.5f;
			m_listBufferedInput = new List<InputItem>();
			m_listQueuedInput = new List<InputItem>();
			m_Controller = controller;
			GetCurrentTime = rClock;

			Moves = new MoveList();
		}

		#endregion //Initialization 

		/// <summary>
		/// clear out all the stored input
		/// </summary>
		public void Clear()
		{
			m_listBufferedInput.Clear();
			m_listQueuedInput.Clear();
		}

		/// <summary>
		/// Parse the move lists and queues.  This method is called every frame, unless the game is paused.
		/// This update function will read input from the controller and then parse the move lists and queues.
		/// Use this update if the character is your game only faces left or right.
		/// </summary>
		/// <param name="rInputState">the current input state of the game.
		/// If an input state is passed in, the controller wrapper will be updated with the data in there.
		/// If the controller wrapper has gotten it's input from somewhere else (ie the network), pass in null</param>
		/// <param name="bFlipped">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
		public void Update(InputState rInputState, bool bFlipped)
		{
			UpdateController(rInputState);
			
			//create a fake direction for left or right
			UpdateMoveQueue(bFlipped, (bFlipped ? new Vector2(-1.0f, 0.0f) : new Vector2(1.0f, 0.0f)));
		}

		/// <summary>
		/// Parse the move lists and queues.  This method is called every frame, unless the game is paused.
		/// This update function will read input from the controller and then parse the move lists and queues.
		/// </summary>
		/// <param name="rInputState">the current input state of the game.
		/// If an input state is passed in, the controller wrapper will be updated with the data in there.
		/// If the controller wrapper has gotten it's input from somewhere else (ie the network), pass in null</param>
		/// <param name="bFlipped">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
		/// <param name="direction">the direction the character is facing</param>
		public void Update(InputState rInputState, bool bFlipped, Vector2 direction)
		{
			UpdateController(rInputState);
			UpdateMoveQueue(bFlipped, direction);
		}

		/// <summary>
		/// update the controller
		/// </summary>
		/// <param name="rInputState"></param>
		private void UpdateController(InputState rInputState)
		{
			//first update the controller if an input state was passed in.
			if ((null != rInputState) && (null != m_Controller))
			{
				m_Controller.Update(rInputState);
			}
		}

		/// <summary>
		/// update all the queues that hold the move data
		/// </summary>
		/// <param name="bFlipped">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
		/// <param name="direction">the direction the character is facing</param>
		protected void UpdateMoveQueue(bool bFlipped, Vector2 direction)
		{
			//first, remove any old input from the system
			float fCurrentTime = GetCurrentTime();
			float fMinInputItemTime = fCurrentTime - QueuedInputExpire;
			while (m_listQueuedInput.Count > 0)
			{
				if (m_listQueuedInput[0].Time <= fMinInputItemTime)
				{
					m_listQueuedInput.RemoveAt(0);
				}
				else
				{
					//got out all the old input
					break;
				}
			}

			//loop through and check directions and single actions
			if (null != m_Controller)
			{
				//start at the top so they get combined in the correct order
				for (EKeystroke i = EKeystroke.RTriggerRelease; i >= 0; i--)
				{
					//get the result of checking that input button
					if (m_Controller.CheckKeystroke(i, bFlipped, direction))
					{
						BufferKeyStroke(i, fCurrentTime);
					}
				}
			}

			//check if any buffered input keys are expired
			fMinInputItemTime = fCurrentTime - BufferedInputExpire;
			while (m_listBufferedInput.Count > 0)
			{
				if (m_listBufferedInput[0].Time <= fMinInputItemTime)
				{
					//Check if this message is already queued
					bool bFound = false;
					for (int i = 0; i < m_listQueuedInput.Count; i++)
					{
						if (m_listQueuedInput[i] == m_listBufferedInput[0])
						{
							bFound = true;
							break;
						}
					}

					//if message not already queued, add the input message to the input list
					if (!bFound)
					{
						m_listQueuedInput.Add(m_listBufferedInput[0]);
					}

					//remove the message from the buffered input
					m_listBufferedInput.RemoveAt(0);
				}
				else
				{
					//got out all the old buffered input
					break;
				}
			}
		}

		private void BufferKeyStroke(EKeystroke foundKey, float fCurrentTime)
		{
			//ok, found a keystroke... check if we even need it
			bool bNeedIt = true;
			for (int i = 0; i < m_listBufferedInput.Count; i++)
			{
				//is it a dupe keystroke?
				if (m_listBufferedInput[i].Keystroke == foundKey)
				{
					//yeah we got plenty of that shit already
					bNeedIt = false;
					break;
				}

				//Is this new keystroke part of a combination that is already buffered?
				if (RedundantKeystroke(m_listBufferedInput[i].Keystroke, foundKey))
				{
					bNeedIt = false;
					break;
				}

				//Can we combine these keystrokes?
				EKeystroke combined = m_listBufferedInput[i].Keystroke;
				if (CombineKeystrokes(m_listBufferedInput[i].Keystroke, foundKey, ref combined))
				{
					//Ok, these two keystrokes can be combined... 
					
					//remove the old keystroke
					m_listBufferedInput.RemoveAt(i);

					//buffer the new keystroke
					BufferKeyStroke(combined, fCurrentTime);
					bNeedIt = false;
					break;
				}
			}

			if (bNeedIt)
			{
				//add to the buffered input for checking later
				m_listBufferedInput.Add(new InputItem(fCurrentTime, foundKey));
			}
		}

		/// <summary>
		/// given two keys, check if the second key is part of the first one
		/// </summary>
		/// <param name="eCurKey"></param>
		/// <param name="eNextKey"></param>
		/// <returns></returns>
		private bool RedundantKeystroke(EKeystroke eCurKey, EKeystroke eNextKey)
		{
			//iterate through the components of that higher keystroke and see if the lower is part of it
			for (int i = 0; i < g_KeystrokeCombinations[(int)eCurKey].Length; i++)
			{
				if (eNextKey == g_KeystrokeCombinations[(int)eCurKey][i])
				{
					//this new keystroke is part of an existing one
					return true;
				}
			}

			//we can use this keystroke
			return false;
		}

		/// <summary>
		/// Given two keystrokes, check if they can be combined into a third
		/// </summary>
		/// <param name="eCurKey">the current key to check</param>
		/// <param name="eNextKey">the new key to check</param>
		/// <param name="eCombined">If the keystrokes can be combined, this will hold the result.</param>
		/// <returns>true if the keystrokes were successfully combined.</returns>
		private bool CombineKeystrokes(EKeystroke eCurKey, EKeystroke eNextKey, ref EKeystroke eCombined)
		{
			//see if the two keystrokes can be combined
			EKeystroke firstKey = ((eCurKey < eNextKey) ? eCurKey : eNextKey);
			EKeystroke secondKey = ((eCurKey <= eNextKey) ? eNextKey : eCurKey);
			Debug.Assert(firstKey <= secondKey); //always check the smalelr number as the first index

			//If this is a keystroke that can be combined...
			if (((int)firstKey < TransitionsRowSize) && ((int)secondKey < TransitionsColumnSize))
			{
				eCombined = g_InputTransitions[(int)firstKey, (int)secondKey];
				if (firstKey != eCombined)
				{
					//They can be combined
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get the next Move out of the queue
		/// This clears the move out of the queue, so you can only get it once!
		/// </summary>
		/// <returns>int: the id of the Move (as message index in statemachine). -1 for no Move</returns>
		public int GetNextMove()
		{
			return Moves.GetNextMove(m_listQueuedInput);
		}

		public override string ToString()
		{
			var myText = new StringBuilder();
			for (int i = 0; i < m_listQueuedInput.Count; i++)
			{
				myText.AppendFormat("{0}, ", m_listQueuedInput[i].Keystroke.ToString());
			}

			return myText.ToString();
		}

		/// <summary>
		/// A method used for test harness to output to the screen
		/// </summary>
		/// <returns></returns>
		public string GetBufferedInput()
		{
			var myText = new StringBuilder();
			for (int i = 0; i < m_listBufferedInput.Count; i++)
			{
				myText.AppendFormat("{0}, ", m_listBufferedInput[i].Keystroke.ToString());
			}

			return myText.ToString();
		}

		/// <summary>
		/// get the number of connected gamepads
		/// </summary>
		/// <returns>The num of gamepads.</returns>
		public static int NumGamepads()
		{
			int iTotal = 0;
			for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
			{
				if (GamePad.GetState(i).IsConnected)
				{
					iTotal++;
				}
			}

			return iTotal;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// read input from a xna resource
		/// </summary>
		/// <param name="strResource">name of the resource to load</param>
		/// <para name="rStates">delegate method for resolving message names</para>
		/// <returns>bool: whether or not it was able to load the input list</returns>
		public bool ReadXmlFile(FilenameBuddy.Filename strResource, MessageNameToID rStates)
		{
			Debug.Assert(null != Moves);
			return Moves.ReadXmlFile(strResource, rStates);
		}

		#endregion //File IO
	}
}