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
		private Dictionary<EKeystroke, MoveNode> Children { get; set; }

		///	<summary>
		///	if this	is a leaf node,	this is	the	message	id to send for this	Move
		///	</summary>
		private int MessageId { get; set; }

		///	<summary>
		///	the	controller keystroke this node represents
		///	</summary>
		public EKeystroke Keystroke { get; set; }

		///	<summary>
		///	if this	is a leaf node,	the	name of	the	Move
		///	</summary>
		public string MoveName { get; private set; }

		#endregion //Members

		#region	Methods

		/// <summary>
		/// contrsuctor
		/// </summary>
		/// <param name="keystroke">the keystroke this move node represents</param>
		public MoveNode(EKeystroke keystroke)
		{
			Children = new Dictionary<EKeystroke, MoveNode>();
			Keystroke = keystroke;
			MessageId = -1;
		}

		///	<summary>
		///	Recursively	add	Moves to the tree
		///	Set	all	this nodes parameters 
		///	Allocate and add children nodes
		///	</summary>
		///	<param name="keystrokes">array of all	the	keystrokes for this	move</param>
		///	<param name="inputIndex">the index of the keystrokes that this node represents</param>
		///	<param name="messageId">message id	to send	when this Movee	is activated</param>
		///	<param name="moveName">name of the move</param>
		public void AddMove(
			List<EKeystroke> keystrokes,
			int inputIndex,
			int messageId,
			string moveName)
		{
			Debug.Assert(null != keystrokes);
			Debug.Assert(inputIndex >= 0);
			Debug.Assert(inputIndex < keystrokes.Count);
			Debug.Assert(messageId >= 0);

			//verify that the input	item matches this dude
			Debug.Assert(Keystroke == keystrokes[inputIndex]);

			//If the index is the end of the list, make	this a leaf
			if (inputIndex == (keystrokes.Count - 1))
			{
				MessageId = messageId;
				MoveName = moveName;
				return;
			}

			//Get the child	node for the next input	item, recurse
			var nextIndex = inputIndex + 1;
			var node = GetChildNode(keystrokes[nextIndex]);
			node.AddMove(keystrokes, nextIndex, messageId, moveName);
		}

		///	<summary>
		///	Parse a	list of	inputitems to find the next	Move.
		///	</summary>
		///	<param name="inputItems">a list of input to parse</param>
		///	<param name="inputIndex">index	of the input buffer	to check. If a Move	is found, the item at this index will be removed.</param>
		///	<returns>int: The first	Move that matches the input. -1	if no Moves	are	found</returns>
		public int ParseInput(List<InputItem> inputItems, int inputIndex)
		{
			Debug.Assert(0 <= inputIndex);
			Debug.Assert(inputIndex < inputItems.Count);

			//verify that the input	item matches this dude
			Debug.Assert(Keystroke == inputItems[inputIndex].Keystroke);

			//Check	if this	is a leaf node
			if (0 <= MessageId)
			{
				//remove item from linked list
				inputItems.RemoveAt(inputIndex);

				//return my Move
				return MessageId;
			}

			//check	if iter	is at end of list
			if (inputIndex == (inputItems.Count - 1))
			{
				return -1;
			}

			//otherwise, find child nodes that matches next input item
			var nextIndex = inputIndex + 1;
			if (Children.ContainsKey(inputItems[nextIndex].Keystroke))
			{
				//increment iter and recurse into child	node
				var childNode = Children[inputItems[nextIndex].Keystroke];
				var childMove = childNode.ParseInput(inputItems, nextIndex);
				if (-1 != childMove)
				{
					//if child node returns Move, remove my	node from linked list
					inputItems.RemoveAt(inputIndex);

					//return the found Move
					return childMove;
				}
			}

			return -1;
		}

		///	<summary>
		///	Get a child node with a specified input	item.
		///	if the child node does not exist, this function creates it, sets it, and adds it to the list of child nodes
		///	</summary>
		///	<param name="keystroke">the keystroke we want a node for</param>
		///	<returns>MoveNode:	a child	node that uses that	keystroke</returns>
		private MoveNode GetChildNode(EKeystroke keystroke)
		{
			if (Children.ContainsKey(keystroke))
			{
				return Children[keystroke];
			}
			else
			{
				var moveNode = new MoveNode(keystroke);
				Children[keystroke] = moveNode;
				return moveNode;
			}
		}

		#endregion //Methods
	}
}