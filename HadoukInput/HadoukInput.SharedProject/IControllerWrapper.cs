using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HadoukInput
{
	public interface IControllerWrapper
	{
		bool AnyDirectionHeld { get; }
		ThumbsticksWrapper Thumbsticks { get; }
		bool UseKeyboard { get; set; }

		bool CheckKeystroke(EKeystroke keystroke);
		bool CheckKeystroke(EKeystroke keystroke, bool flipped, Vector2 direction);
		bool CheckKeystrokeHeld(EKeystroke keystroke);
		void ResetController();

		Keys MappedKey(int gamePadIndex, ControllerAction action);

		void Update(IInputState inputState);
	}
}