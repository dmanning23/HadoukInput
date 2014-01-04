using FilenameBuddy;
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
		/// Length of time input items are held in the buffer before being put in the queue
		/// </summary>
		private const float m_fBufferedInputExpire = 0.1f;

		/// <summary>
		/// Length of time input items are held in the queue before they are discarded.
		/// </summary>
		private const float m_fQueuedInputExpire = 0.55f;

		/// <summary>
		/// state machine for combining the keystrokes of input items
		/// This is an 2d array that takes 2 keystrokes and returns the keystroke that results from combining them
		/// </summary>
		private static readonly EKeystroke[,] g_InputTransitions;

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
		private readonly MoveNode[] m_MoveTree;

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

		#endregion //Members

		#region Properties

		public ControllerWrapper Controller
		{
			get { return m_Controller; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Setup the state machine for combining input
		/// </summary>
		static InputWrapper()
		{
			//setup the state machine for doing input transitions
			g_InputTransitions = new EKeystroke[(int)EKeystroke.NumKeystrokes,(int)EKeystroke.NumKeystrokes];

			//set all the keystrokes to default to the row item
			var NumKeystrokes = (int)EKeystroke.NumKeystrokes;
			for (int i = 0; i < NumKeystrokes; i++)
			{
				for (int j = 0; j < NumKeystrokes; j++)
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

			//set the button + direction transitions
			g_InputTransitions[(int)EKeystroke.A, (int)EKeystroke.Up] = EKeystroke.AUp;
			g_InputTransitions[(int)EKeystroke.A, (int)EKeystroke.Down] = EKeystroke.ADown;
			g_InputTransitions[(int)EKeystroke.A, (int)EKeystroke.Forward] = EKeystroke.AForward;
			g_InputTransitions[(int)EKeystroke.A, (int)EKeystroke.Back] = EKeystroke.ABack;

			g_InputTransitions[(int)EKeystroke.B, (int)EKeystroke.Up] = EKeystroke.BUp;
			g_InputTransitions[(int)EKeystroke.B, (int)EKeystroke.Down] = EKeystroke.BDown;
			g_InputTransitions[(int)EKeystroke.B, (int)EKeystroke.Forward] = EKeystroke.BForward;
			g_InputTransitions[(int)EKeystroke.B, (int)EKeystroke.Back] = EKeystroke.BBack;

			g_InputTransitions[(int)EKeystroke.X, (int)EKeystroke.Up] = EKeystroke.XUp;
			g_InputTransitions[(int)EKeystroke.X, (int)EKeystroke.Down] = EKeystroke.XDown;
			g_InputTransitions[(int)EKeystroke.X, (int)EKeystroke.Forward] = EKeystroke.XForward;
			g_InputTransitions[(int)EKeystroke.X, (int)EKeystroke.Back] = EKeystroke.XBack;

			g_InputTransitions[(int)EKeystroke.Y, (int)EKeystroke.Up] = EKeystroke.YUp;
			g_InputTransitions[(int)EKeystroke.Y, (int)EKeystroke.Down] = EKeystroke.YDown;
			g_InputTransitions[(int)EKeystroke.Y, (int)EKeystroke.Forward] = EKeystroke.YForward;
			g_InputTransitions[(int)EKeystroke.Y, (int)EKeystroke.Back] = EKeystroke.YBack;

			g_InputTransitions[(int)EKeystroke.LShoulder, (int)EKeystroke.Up] = EKeystroke.LShoulderUp;
			g_InputTransitions[(int)EKeystroke.LShoulder, (int)EKeystroke.Down] = EKeystroke.LShoulderDown;
			g_InputTransitions[(int)EKeystroke.LShoulder, (int)EKeystroke.Forward] = EKeystroke.LShoulderForward;
			g_InputTransitions[(int)EKeystroke.LShoulder, (int)EKeystroke.Back] = EKeystroke.LShoulderBack;

			g_InputTransitions[(int)EKeystroke.RShoulder, (int)EKeystroke.Up] = EKeystroke.RShoulderUp;
			g_InputTransitions[(int)EKeystroke.RShoulder, (int)EKeystroke.Down] = EKeystroke.RShoulderDown;
			g_InputTransitions[(int)EKeystroke.RShoulder, (int)EKeystroke.Forward] = EKeystroke.RShoulderForward;
			g_InputTransitions[(int)EKeystroke.RShoulder, (int)EKeystroke.Back] = EKeystroke.RShoulderBack;

			g_InputTransitions[(int)EKeystroke.LTrigger, (int)EKeystroke.Up] = EKeystroke.LTriggerUp;
			g_InputTransitions[(int)EKeystroke.LTrigger, (int)EKeystroke.Down] = EKeystroke.LTriggerDown;
			g_InputTransitions[(int)EKeystroke.LTrigger, (int)EKeystroke.Forward] = EKeystroke.LTriggerForward;
			g_InputTransitions[(int)EKeystroke.LTrigger, (int)EKeystroke.Back] = EKeystroke.LTriggerBack;

			g_InputTransitions[(int)EKeystroke.RTrigger, (int)EKeystroke.Up] = EKeystroke.RTriggerUp;
			g_InputTransitions[(int)EKeystroke.RTrigger, (int)EKeystroke.Down] = EKeystroke.RTriggerDown;
			g_InputTransitions[(int)EKeystroke.RTrigger, (int)EKeystroke.Forward] = EKeystroke.RTriggerForward;
			g_InputTransitions[(int)EKeystroke.RTrigger, (int)EKeystroke.Back] = EKeystroke.RTriggerBack;
		}

		/// <summary>
		/// contructor
		/// </summary>
		/// <param name="iPlayerIndex">index of the controller this player will use</param>
		/// <param name="rClock">The external clock that will be used to time this dude.  This guy doesn't update his own timer!</param>
		public InputWrapper(ControllerWrapper controller, CurrentTime rClock)
		{
			Debug.Assert(null != rClock);

			m_listBufferedInput = new List<InputItem>();
			m_listQueuedInput = new List<InputItem>();
			m_Controller = controller;
			GetCurrentTime = rClock;

			//set up the Movee tree
			m_MoveTree = new MoveNode[(int)EKeystroke.NumKeystrokes];
			for (EKeystroke i = 0; i < EKeystroke.NumKeystrokes; i++)
			{
				m_MoveTree[(int)i] = new MoveNode(i);
			}
		}

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
		/// <param name="rPlayer">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
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
		/// <param name="rPlayer">whether or not the character that this input wrapper controls is facing right(false) or left(true).</param>
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
		/// <param name="rPlayer">the object that uses this input queue</param>
		protected void UpdateMoveQueue(bool bFlipped, Vector2 direction)
		{
			//first, remove any old input from the system
			float fCurrentTime = GetCurrentTime();
			float fMinInputItemTime = fCurrentTime - m_fQueuedInputExpire;
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
				for (EKeystroke i = 0; i <= EKeystroke.RTriggerRelease; i++)
				{
					//get the result of checking that input button
					if (m_Controller.CheckKeystroke(i, bFlipped, direction))
					{
						//add to the buffered input for checking later
						var rItem = new InputItem(fCurrentTime, i);
						m_listBufferedInput.Add(rItem);
					}
				}
			}

			//okay, check all the buffered input for simultaneous keys
			int iCur = 0;
			while (iCur < (m_listBufferedInput.Count - 1))
			{
				//check this item with the next one in the list
				int iNext = iCur + 1;
				while (iNext < m_listBufferedInput.Count)
				{
					//get the two keystrokes
					EKeystroke eCurKey = m_listBufferedInput[iCur].Keystroke;
					EKeystroke eNextKey = m_listBufferedInput[iNext].Keystroke;
					Debug.Assert(eCurKey < EKeystroke.NumKeystrokes);
					Debug.Assert(eNextKey < EKeystroke.NumKeystrokes);

					//see if the two keystrokes can be combined
					EKeystroke eCombined = g_InputTransitions[(int)eCurKey, (int)eNextKey];
					if ((eCurKey != eCombined) || (eCurKey == eNextKey))
					{
						//if found one, change this to the new keystroke
						m_listBufferedInput[iCur].Keystroke = eCombined;

						//remove the next item from the buffered input
						m_listBufferedInput.RemoveAt(iNext);
					}
					else
					{
						iNext++;
					}
				}

				iCur++;
			}

			//check if any buffered input keys are expired
			fMinInputItemTime = fCurrentTime - m_fBufferedInputExpire;
			while (m_listBufferedInput.Count > 0)
			{
				if (m_listBufferedInput[0].Time <= fMinInputItemTime)
				{
					//if so, add the input message to the input list and remove from this one
					m_listQueuedInput.Add(m_listBufferedInput[0]);
					m_listBufferedInput.RemoveAt(0);
				}
				else
				{
					//got out all the old buffered input
					break;
				}
			}
		}

		/// <summary>
		/// Get the next Move out of the queue
		/// This clears the move out of the queue, so you can only get it once!
		/// </summary>
		/// <returns>int: the id of the Move (as message index in statemachine). -1 for no Move</returns>
		public int GetNextMove()
		{
			Debug.Assert(null != m_MoveTree);
			for (int i = 0; i < m_listQueuedInput.Count; i++)
			{
				//get the branch of the Move tree for the current keystroke
				Debug.Assert(EKeystroke.NumKeystrokes != m_listQueuedInput[i].Keystroke);
				var iKeystrokeIndex = (int)m_listQueuedInput[i].Keystroke;
				Debug.Assert(iKeystrokeIndex < m_MoveTree.Length);
				Debug.Assert(null != m_MoveTree[iKeystrokeIndex]);
				int iMove = m_MoveTree[iKeystrokeIndex].ParseInput(m_listQueuedInput, i);
				if (-1 != iMove)
				{
					return iMove;
				}
			}

			//no Moves were found
			return -1;
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
		/// <param name="rContent">xna content manager</param>
		/// <param name="strResource">name of the resource to load</param>
		/// <returns>bool: whether or not it was able to load the input list</returns>
		public bool ReadXmlFile(Filename strResource, MessageNameToID rStates)
		{
			Debug.Assert(null != m_MoveTree);

			//Open the file.
			FileStream stream = File.Open(strResource.File, FileMode.Open, FileAccess.Read);
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			XmlNode rootNode = xmlDoc.DocumentElement;

			//make sure it is actually an xml node
			if (rootNode.NodeType != XmlNodeType.Element)
			{
				//should be an xml node!!!
				return false;
			}

			//eat up the name of that xml node
			string strElementName = rootNode.Name;
			if (("XnaContent" != strElementName) || !rootNode.HasChildNodes)
			{
				return false;
			}

			//next node is "<Asset Type="SPFSettings.MoveListXML">"
			XmlNode AssetNode = rootNode.FirstChild;
			if (null == AssetNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!AssetNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}
			if ("Asset" != AssetNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//Read in all the moves
			XmlNode movesNode = AssetNode.FirstChild;
			for (XmlNode moveNode = movesNode.FirstChild;
			     null != moveNode;
			     moveNode = moveNode.NextSibling)
			{
				//if it isnt an element node, continue
				if (moveNode.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				//Get teh name node
				XmlNode childNode = moveNode.FirstChild;
				string strMessageName = childNode.InnerXml;
				int iMessage = rStates(strMessageName);
				Debug.Assert(iMessage >= 0);

				//get the keystrokes node
				XmlNode keystrokesNode = childNode.NextSibling;

				//put the input into a proper list
				var listKeystrokes = new List<EKeystroke>();
				try
				{
					for (XmlNode keystrokeNode = keystrokesNode.FirstChild;
					     null != keystrokeNode;
					     keystrokeNode = keystrokeNode.NextSibling)
					{
						var myKeystroke = (EKeystroke)Enum.Parse(typeof (EKeystroke), keystrokeNode.InnerXml);
						listKeystrokes.Add(myKeystroke);
					}
				}
				catch (Exception)
				{
					Debug.Assert(false, "Bad xml in the move list");
					return false;
				}

				//add the move to the Move tree
				Debug.Assert(EKeystroke.NumKeystrokes != listKeystrokes[0]);
				var iKeystrokeIndex = (int)listKeystrokes[0];
				Debug.Assert(iKeystrokeIndex < m_MoveTree.Length);
				Debug.Assert(null != m_MoveTree[iKeystrokeIndex]);
				m_MoveTree[iKeystrokeIndex].AddMove(listKeystrokes, 0, iMessage, strMessageName);
			}

			// Close the file.
			stream.Close();
			return true;
		}

		/// <summary>
		/// read input from a xna resource
		/// </summary>
		/// <param name="rContent">xna content manager</param>
		/// <param name="strResource">name of the resource to load</param>
		/// <returns>bool: whether or not it was able to load the input list</returns>
		public bool ReadSerializedFile(ContentManager rXmlContent, Filename strResource, MessageNameToID rStates)
		{
			Debug.Assert(null != m_MoveTree);

			//read in serialized xna input list
			var myXML = rXmlContent.Load<MoveListXML>(strResource.GetRelPathFileNoExt());

			//read in the state names
			for (int i = 0; i < myXML.moves.Count; i++)
			{
				//get the state machine message
				string strMessageName = myXML.moves[i].name;
				int iMessage = rStates(strMessageName);
				Debug.Assert(iMessage >= 0);

				//put the input into a proper list
				var listKeystrokes = new List<EKeystroke>();
				try
				{
					for (int j = 0; j < myXML.moves[i].keystrokes.Count; j++)
					{
						var myKeystroke = (EKeystroke)Enum.Parse(typeof (EKeystroke), myXML.moves[i].keystrokes[j]);
						listKeystrokes.Add(myKeystroke);
					}
				}
				catch (Exception)
				{
					Debug.Assert(false, "Bad xml in the move list");
					return false;
				}

				//add the move to the Move tree
				Debug.Assert(EKeystroke.NumKeystrokes != listKeystrokes[0]);
				var iKeystrokeIndex = (int)listKeystrokes[0];
				Debug.Assert(iKeystrokeIndex < m_MoveTree.Length);
				Debug.Assert(null != m_MoveTree[iKeystrokeIndex]);
				m_MoveTree[iKeystrokeIndex].AddMove(listKeystrokes, 0, iMessage, strMessageName);
			}

			return true;
		}

		#endregion //File IO
	}
}