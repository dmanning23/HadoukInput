using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
#if OUYA
using Ouya.Console.Api;
#endif

namespace HadoukInput
{
	/// <summary>
	/// This is the object that holds all the data for a move list after it is loaded from XML by the XMLImporter.
	/// </summary>
	public class MoveList
	{
		#region Fields

		/// <summary>
		/// List of all the moves in this move list
		/// </summary>
		public List<MoveNode> Moves { get; set; }

		#endregion //Fields

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		public MoveList()
		{
			Moves = new List<MoveNode>();

			//set up the Movee tree
			for (EKeystroke i = 0; i < EKeystroke.NumKeystrokes; i++)
			{
				Moves.Add(new MoveNode(i));
			}
		}

		/// <summary>
		/// Get the next Move out of the queue
		/// This clears the move out of the queue, so you can only get it once!
		/// </summary>
		/// <returns>int: the id of the Move (as message index in statemachine). -1 for no Move</returns>
		public int GetNextMove(List<InputItem> input)
		{
			Debug.Assert(null != Moves);
			for (int i = 0; i < input.Count; i++)
			{
				//get the branch of the Move tree for the current keystroke
				Debug.Assert(EKeystroke.NumKeystrokes != input[i].Keystroke);
				var iKeystrokeIndex = (int)input[i].Keystroke;
				Debug.Assert(iKeystrokeIndex < Moves.Count);
				Debug.Assert(null != Moves[iKeystrokeIndex]);
				int iMove = Moves[iKeystrokeIndex].ParseInput(input, i);
				if (-1 != iMove)
				{
					return iMove;
				}
			}

			//no Moves were found
			return -1;
		}

		/// <summary>
		/// read input from a xna resource
		/// </summary>
		/// <param name="strResource">name of the resource to load</param>
		/// <para name="rStates">delegate method for resolving message names</para>
		/// <returns>bool: whether or not it was able to load the input list</returns>
		public bool ReadXmlFile(FilenameBuddy.Filename strResource, MessageNameToID rStates)
		{
			//Open the file.
			#if OUYA
			Stream stream = Game.Activity.Assets.Open(strResource.File);
			#else
			FileStream stream = File.Open(strResource.File, FileMode.Open, FileAccess.Read);
			#endif
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
						var myKeystroke = (EKeystroke)Enum.Parse(typeof(EKeystroke), keystrokeNode.InnerXml);
						listKeystrokes.Add(myKeystroke);
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Bad xml in the move list", ex);
				}

				//add the move to the Move tree
				Debug.Assert(EKeystroke.NumKeystrokes != listKeystrokes[0]);
				var iKeystrokeIndex = (int)listKeystrokes[0];
				Debug.Assert(iKeystrokeIndex < Moves.Count);
				Debug.Assert(null != Moves[iKeystrokeIndex]);
				Moves[iKeystrokeIndex].AddMove(listKeystrokes, 0, iMessage, strMessageName);
			}

			// Close the file.
			stream.Close();
			return true;
		}

		#endregion //Methods
	}
}