using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using XmlBuddy;

namespace HadoukInput
{
	/// <summary>
	/// This is the object that holds all the data for a move list after it is loaded from XML by the XMLImporter.
	/// </summary>
	public class MoveListModel : XmlFileBuddy
	{
		#region Properties

		/// <summary>
		/// List of all the moves in this move list
		/// </summary>
		public List<MoveModel> Moves { get; private set; }

		#endregion //Fields

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		public MoveListModel(Filename xmlFilename)
			: base("MoveList", xmlFilename)
		{
			Moves = new List<MoveModel> ();
		}

		public override void ParseXmlNode(XmlNode node)
		{
			var name = node.Name;
			var value = node.Value;

			switch (name.ToLower())
			{
				case "asset":
					{
						//skip these old ass nodes
						XmlFileBuddy.ReadChildNodes(node, ParseXmlNode);
					}
					break;
				case "type":
					{
						//Really skip these old ass nodes
					}
					break;
				case "moves":
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
			var move = new MoveModel(nameNode.InnerXml);

			//get the keystrokes node
			var keystrokesNode = nameNode.NextSibling;
			if (null == keystrokesNode || !keystrokesNode.HasChildNodes)
			{
				return;
			}

			//put the input into a proper list
			try
			{
				for (XmlNode keystrokeNode = keystrokesNode.FirstChild;
					 null != keystrokeNode;
					 keystrokeNode = keystrokeNode.NextSibling)
				{
					var myKeystroke = (EKeystroke)Enum.Parse(typeof(EKeystroke), keystrokeNode.InnerXml);
					move.Keystrokes.Add(myKeystroke);
				}

				Moves.Add(move);
			}
			catch (Exception ex)
			{
				throw new Exception("Bad xml in the move list", ex);
			}
		}

#if !WINDOWS_UWP
		public override void WriteXmlNodes(XmlTextWriter xmlFile)
		{
			throw new NotImplementedException();
		}
#endif

		#endregion //Methods
	}
}