using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if !BRIDGE
using System.Xml;
#endif
using XmlBuddy;

namespace HadoukInput
{
	/// <summary>
	/// This is the object that holds all the data for a move list after it is loaded from XML by the XMLImporter.
	/// </summary>
	public class MoveList
	{
		#region Properties

		/// <summary>
		/// List of all the moves in this move list
		/// </summary>
		public Dictionary<EKeystroke, MoveNode> Moves { get; private set; }

		#endregion //Fields

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		public MoveList()
		{
			Moves = new Dictionary<EKeystroke, MoveNode>();
		}

		public MoveList(MoveListModel moveListModel) : this()
		{
			AddMoves(moveListModel);
		}

		public void AddMoves(MoveListModel moveListModel)
		{
			foreach (var move in moveListModel.Moves)
			{
				AddMove(move.Name, move.Keystrokes.ToArray());
			}
		}

		public void RemoveMoves(MoveListModel moveListModel)
		{
			foreach (var move in moveListModel.Moves)
			{
				RemoveMove(move.Name, move.Keystrokes.ToArray());
			}
		}

		public void AddMove(string moveName, params EKeystroke[] keystrokes)
		{
			if (string.IsNullOrEmpty(moveName))
			{
				throw new Exception("Tried to add a move with no name");
			}
			else if (keystrokes.Length == 0)
			{
				throw new Exception("Tried to add a move {moveName} with 0 keystrokes");
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
				moveNode = new MoveNode();
				Moves[key] = moveNode;
			}

			//add the move to the Move tree
			moveNode.AddMove(0, moveName, keystrokes);
		}

		public void RemoveMove(string moveName, params EKeystroke[] keystrokes)
		{
			if (string.IsNullOrEmpty(moveName))
			{
				throw new Exception("Tried to remove a move with no name");
			}
			else if (keystrokes.Length == 0)
			{
				throw new Exception("Tried to remove a move {moveName} with 0 keystrokes");
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
				//nothing to remove!
				return;
			}

			//check if the movenode can be removed
			if (moveNode.RemoveMove(0, moveName, keystrokes))
			{
				Moves.Remove(key);
			}
		}

		/// <summary>
		/// Get the next Move out of the queue
		/// This clears the move out of the queue, so you can only get it once!
		/// </summary>
		/// <returns>int: the id of the Move (as message index in statemachine). -1 for no Move</returns>
		public string GetNextMove(List<InputItem> input)
		{
			for (var i = 0; i < input.Count; i++)
			{
				//get the branch of the Move tree for the current keystroke
				if (Moves.ContainsKey(input[i].Keystroke))
				{
					var moveId = Moves[input[i].Keystroke].ParseInput(input, i);
					if (!string.IsNullOrEmpty(moveId))
					{
						return moveId;
					}
				}
			}

			//no Moves were found
			return string.Empty;
		}

		#endregion //Methods
	}
}