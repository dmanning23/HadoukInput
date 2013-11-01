using System.Collections.Generic;
using System.Diagnostics;

namespace HadoukInput
{
	/// <summary>
	/// This is a single node in the move tree, used for input pattern recognition
	/// </summary>
	internal class MoveNode
	{
		#region	Members

		///	<summary>
		///	child nodes
		///	</summary>
		private readonly List<MoveNode> m_listChildren;

		///	<summary>
		///	if this	is a leaf node,	this is	the	message	id to send for this	Move
		///	</summary>
		private int m_iMessageID;

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
		/// <param name="eKeystroke">the keystroke this move node represents</param>
		public MoveNode(EKeystroke eKeystroke)
		{
			m_listChildren = new List<MoveNode>();
			Keystroke = eKeystroke;
			m_iMessageID = -1;
		}

		///	<summary>
		///	Recursively	add	Moves to the tree
		///	Set	all	this nodes parameters 
		///	Allocate and add children nodes
		///	</summary>
		///	<param name="rgInputItems">array of	all	the	keystrokes for this	move</param>
		///	<param name="iInputIndex">the index	of the rgInputItems	that this node represents</param>
		///	<param name="iMessageID">message id	to send	when this Movee	is activated</param>
		///	<param name="strMoveName">name of the move</param>
		public void AddMove(
			List<EKeystroke> rgInputItems,
			int iInputIndex,
			int iMessageID,
			string strMoveName)
		{
			Debug.Assert(null != rgInputItems);
			Debug.Assert(iInputIndex >= 0);
			Debug.Assert(iInputIndex < rgInputItems.Count);
			Debug.Assert(iMessageID >= 0);
			Debug.Assert(EKeystroke.NumKeystrokes > rgInputItems[iInputIndex]);

			//verify that the input	item matches this dude
			Debug.Assert(Keystroke == rgInputItems[iInputIndex]);

			//If the index is the end of the list, make	this a leaf
			if (iInputIndex == (rgInputItems.Count - 1))
			{
				m_iMessageID = iMessageID;
				MoveName = strMoveName;
				return;
			}

			//Get the child	node for the next input	item, recurse
			int iNextIndex = iInputIndex + 1;
			MoveNode rNode = GetChildNode(rgInputItems[iNextIndex]);
			rNode.AddMove(rgInputItems, iNextIndex, iMessageID, strMoveName);
		}

		///	<summary>
		///	Parse a	list of	inputitems to find the next	Move.
		///	</summary>
		///	<param name="rgInputItems">a list of input to parse</param>
		///	<param name="iInputIndex">index	of the input buffer	to check. If a Move	is found, the item at this index will be removed.</param>
		///	<returns>int: The first	Move that matches the input. -1	if no Moves	are	found</returns>
		public int ParseInput(List<InputItem> rgInputItems, int iInputIndex)
		{
			Debug.Assert(0 <= iInputIndex);
			Debug.Assert(iInputIndex < rgInputItems.Count);

			//verify that the input	item matches this dude
			Debug.Assert(Keystroke == rgInputItems[iInputIndex].Keystroke);

			//Check	if this	is a leaf node
			if (0 <= m_iMessageID)
			{
				//remove item from linked list
				rgInputItems.RemoveAt(iInputIndex);
				//return my Move
				return m_iMessageID;
			}

			//check	if iter	is at end of list
			if (iInputIndex == (rgInputItems.Count - 1))
			{
				return -1;
			}

			//otherwise, find child	nodes that matches next	input item
			int iNextIndex = iInputIndex + 1;
			for (int i = 0; i < m_listChildren.Count; i++)
			{
				if (m_listChildren[i].Keystroke == rgInputItems[iNextIndex].Keystroke)
				{
					//increment	iter and recurse into child	node
					int iChildMove = m_listChildren[i].ParseInput(rgInputItems, iNextIndex);
					if (-1 != iChildMove)
					{
						//if child node	returns	Move, remove my	node from linked list
						rgInputItems.RemoveAt(iInputIndex);

						//return the found Move
						return iChildMove;
					}
				}
			}

			return -1;
		}

		///	<summary>
		///	Get	a child	node with a	specified input	item.
		///	if the child node does not exist, this function	creates	it,	sets it, and adds it to	the	list of	child nodes
		///	</summary>
		///	<param name="eKeystroke">the keystroke we want a node for</param>
		///	<returns>MoveNode:	a child	node that uses that	keystroke</returns>
		private MoveNode GetChildNode(EKeystroke eKeystroke)
		{
			//check	if a node exists already with that input item
			for (int i = 0; i < m_listChildren.Count; i++)
			{
				if (eKeystroke == m_listChildren[i].Keystroke)
				{
					//return the existing node
					return m_listChildren[i];
				}
			}

			//if not, create the node and set it
			var rNode = new MoveNode(eKeystroke);

			//add the node to the list
			m_listChildren.Add(rNode);

			return rNode;
		}

		#endregion //Methods
	}
}