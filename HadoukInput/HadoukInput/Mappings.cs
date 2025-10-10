using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace HadoukInput
{
	public static class Mappings
	{
		/// <summary>
		/// button mappings for all 4 controllers
		/// These map a controller action to a button on a controller.
		/// These can be changed to do button remapping...
		/// TODO: add a function to do button remapping:  should take an action & button, reset the actions mapped to that same button
		/// </summary>
		static public List<ButtonMap> ButtonMaps { get; private set; }

		/// <summary>
		/// key mappings for all 4 controllers
		/// These map a controller action to a key on the keyboard.
		/// These can be changed to do key remapping...
		/// </summary>
		static public List<KeyMap> KeyMaps { get; private set; }

		static public List<bool> UseKeyboard { get; private set; }

		/// <summary>
		/// Initializes the <see cref="HadoukInput.ControllerWrapper"/> class.
		/// Thereare a few variables in Monogame that screw stuff up... set them here
		/// </summary>
		static Mappings()
		{
			ButtonMaps = new List<ButtonMap>();
			KeyMaps = new List<KeyMap>();
			UseKeyboard = new List<bool>();
			for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
			{
				ButtonMaps.Add(new ButtonMap());
				KeyMaps.Add(new KeyMap());
				UseKeyboard.Add(false);
			}

			//Set the first player to use the keyboard
			UseKeyboard[0] = true;
		}

		/// <summary>
		/// Get the keyboard key that is mapped to an action
		/// </summary>
		/// <param name="gamePadIndex"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static Keys MappedKey(int gamePadIndex, ControllerAction action)
		{
			return KeyMaps[gamePadIndex].ActionMap(action);
		}

		public static void UseIpacMappings(int playerIndex)
		{
			UseKeyboard[playerIndex] = true;
			KeyMaps[playerIndex].UseIpacMappings(playerIndex);
		}
	}
}
