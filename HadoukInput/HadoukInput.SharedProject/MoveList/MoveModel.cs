using System.Collections.Generic;

namespace HadoukInput
{
	public class MoveModel
	{
		public string Name { get; set; }
		public List<EKeystroke> Keystrokes { get; private set; }

		public MoveModel(string name)
		{
			Name = name;
			Keystrokes = new List<EKeystroke>();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
