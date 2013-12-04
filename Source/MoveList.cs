using System.Collections.Generic;

namespace HadoukInput
{
	/// <summary>
	/// This is the object that holds all the data for a move list after it is loaded from XML by the XMLImporter.
	/// </summary>
	public class MoveListXML
	{
		public List<MoveXML> moves = new List<MoveXML>();
	}

	/// <summary>
	/// This object holds the data for a single move after the move list is loaded from XML by the XMLImporter
	/// </summary>
	public class MoveXML
	{
		public string name = "";
		public List<string> keystrokes = new List<string>();
	}
}