using System.Collections.Generic;
using System.Diagnostics;

namespace HadoukInput
{
	/// <summary>
	/// This is a single node in the move tree, used for input pattern recognition
	/// </summary>
	public class MoveNode
	{
		#region	Members

		///	<summary>
		///	child nodes
		///	</summary>
		public Dictionary<EKeystroke, MoveNode> Moves { get; private set; }

		///	<summary>
		///	if this is a leaf node, the name of the Move
		///	</summary>
		public string MoveName { get; private set; }

		#endregion //Members

		#region	Methods

		/// <summary>
		/// contrsuctor
		/// </summary>
		/// <param name="keystroke">the keystroke this move node represents</param>
		public MoveNode()
		{
			Moves = new Dictionary<EKeystroke, MoveNode>();
		}

		///	<summary>
		///	Recursively add Moves to the tree
		///	Set all this nodes parameters 
		///	Allocate and add children nodes
		///	</summary>
		///	<param name="inputIndex">the index of the keystrokes that this node represents</param>
		///	<param name="moveName">name of the move</param>
		///	<param name="keystrokes">array of all the keystrokes for this move</param>
		public void AddMove(int inputIndex, string moveName, params EKeystroke [] keystrokes)
		{
			//If the index is the end of the list, make	this a leaf
			if (inputIndex == (keystrokes.Length - 1))
			{
				MoveName = moveName;
				return;
			}

			//Get the child	node for the next input	item, recurse
			var nextIndex = inputIndex + 1;
			var node = GetChildNode(keystrokes[nextIndex]);
			node.AddMove(nextIndex, moveName, keystrokes);
		}

		public bool RemoveMove(int inputIndex, string moveName, params EKeystroke[] keystrokes)
		{
			//If the index is the end of the list, make	this a leaf
			if (MoveName == moveName)
			{
				return true;
			}
			else if (!string.IsNullOrEmpty(MoveName))
			{
				return false;
			}

			//Get the child	node for the next input	item, recurse
			var nextIndex = inputIndex + 1;
			if (Moves.ContainsKey(keystrokes[nextIndex]))
			{
				var node = Moves[keystrokes[nextIndex]];
				var removeChild = node.RemoveMove(nextIndex, moveName, keystrokes);
				if (removeChild)
				{
					Moves.Remove(keystrokes[nextIndex]);
					return Moves.Count == 0;
				}
			}

			return false;
		}

		///	<summary>
		///	Parse a	list of	inputitems to find the next	Move.
		///	</summary>
		///	<param name="inputItems">a list of input to parse</param>
		///	<param name="inputIndex">index	of the input buffer	to check. If a Move	is found, the item at this index will be removed.</param>
		///	<returns>int: The first	Move that matches the input. -1	if no Moves	are	found</returns>
		public string ParseInput(List<InputItem> inputItems, int inputIndex)
		{
			//Check	if this	is a leaf node
			if (!string.IsNullOrEmpty(MoveName))
			{
				//remove item from linked list
				inputItems.RemoveAt(inputIndex);

				//return my Move
				return MoveName;
			}

			//check	if iter	is at end of list
			if (inputIndex == (inputItems.Count - 1))
			{
				return string.Empty;
			}

			//otherwise, find child nodes that matches next input item
			var nextIndex = inputIndex + 1;
			if (Moves.ContainsKey(inputItems[nextIndex].Keystroke))
			{
				//increment iter and recurse into child	node
				var childNode = Moves[inputItems[nextIndex].Keystroke];
				var childMove = childNode.ParseInput(inputItems, nextIndex);
				if (!string.IsNullOrEmpty(childMove))
				{
					//if child node returns Move, remove my	node from linked list
					inputItems.RemoveAt(inputIndex);

					//return the found Move
					return childMove;
				}
			}

			return string.Empty;
		}

		///	<summary>
		///	Get a child node with a specified input	item.
		///	if the child node does not exist, this function creates it, sets it, and adds it to the list of child nodes
		///	</summary>
		///	<param name="keystroke">the keystroke we want a node for</param>
		///	<returns>MoveNode:	a child	node that uses that	keystroke</returns>
		private MoveNode GetChildNode(EKeystroke keystroke)
		{
			if (Moves.ContainsKey(keystroke))
			{
				return Moves[keystroke];
			}
			else
			{
				var moveNode = new MoveNode();
				Moves[keystroke] = moveNode;
				return moveNode;
			}
		}

		#endregion //Methods
	}
}