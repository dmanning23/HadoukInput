using Microsoft.Xna.Framework;

namespace HadoukInput
{
	public interface IControllerWrapper
	{
		bool AnyDirectionHeld { get; }
		ThumbsticksWrapper Thumbsticks { get; }

		bool CheckKeystroke(EKeystroke keystroke);
		bool CheckKeystroke(EKeystroke keystroke, bool flipped, Vector2 direction);
		bool CheckKeystrokeHeld(EKeystroke keystroke);
		void ResetController();
		void Update(InputState inputState);
	}
}