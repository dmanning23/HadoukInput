using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using XmlBuddy;
#if OUYA
using Ouya.Console.Api;
#endif

namespace HadoukInput
{
	/// <summary>
	/// This is the object that holds all the data for a move list after it is loaded from XML by the XMLImporter.
	/// </summary>
	public class MoveList : XmlFileBuddy
	{
		#region Properties

		/// <summary>
		/// List of all the moves in this move list
		/// </summary>
		public Dictionary<EKeystroke, MoveNode> Moves { get; private set; }

		private MessageNameToId NameResolver { get; set; }

		#endregion //Fields

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		public MoveList(MessageNameToId nameResolver, Filename xmlFilename)
			: base("MoveList", xmlFilename)
		{
			Moves = new Dictionary<EKeystroke, MoveNode>();
			NameResolver = nameResolver;
		}

		/// <summary>
		/// Get the next Move out of the queue
		/// This clears the move out of the queue, so you can only get it once!
		/// </summary>
		/// <returns>int: the id of the Move (as message index in statemachine). -1 for no Move</returns>
		public int GetNextMove(List<InputItem> input)
		{
			Debug.Assert(null != input);
			for (var i = 0; i < input.Count; i++)
			{
				//get the branch of the Move tree for the current keystroke
				if (Moves.ContainsKey(input[i].Keystroke))
				{
					var moveId = Moves[input[i].Keystroke].ParseInput(input, i);
					if (-1 != moveId)
					{
						return moveId;
					}
				}
			}

			//no Moves were found
			return -1;
		}

		public override void ParseXmlNode(XmlNode node)
		{
			var name = node.Name;
			var value = node.Value;

			switch (name)
			{
				case "Moves":
				{
					ReadChildNodes(node, ReadMove);
				}
				break;
				default:
				{
					throw new Exception(string.Format("unknown xml node passed to MoveList: {0}", name));
				}
			}
		}

		private void ReadMove(XmlNode node)
		{
			//Get teh name node
			var nameNode = node.FirstChild;
			var moveName = nameNode.InnerXml;
			var moveId = NameResolver(moveName);
			Debug.Assert(moveId >= 0);

			//get the keystrokes node
			var keystrokesNode = nameNode.NextSibling;
			if (null == keystrokesNode || !keystrokesNode.HasChildNodes)
			{
				return;
			}

			//put the input into a proper list
			var keystrokes = new List<EKeystroke>();
			try
			{
				for (XmlNode keystrokeNode = keystrokesNode.FirstChild;
					 null != keystrokeNode;
					 keystrokeNode = keystrokeNode.NextSibling)
				{
					var myKeystroke = (EKeystroke)Enum.Parse(typeof(EKeystroke), keystrokeNode.InnerXml);
					keystrokes.Add(myKeystroke);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Bad xml in the move list", ex);
			}

			//Get the correct moveNode
			var key = keystrokes[0];
			MoveNode moveNode;
			if (Moves.ContainsKey(key))
			{
				moveNode = Moves[key];
			}
			else
			{
				moveNode = new MoveNode(key);
				Moves[key] = moveNode;
			}

			//add the move to the Move tree
			moveNode.AddMove(keystrokes, 0, moveId, moveName);
		}

		public override void WriteXmlNodes(XmlTextWriter xmlFile)
		{
			throw new NotImplementedException();
		}

		#endregion //Methods
	}
}