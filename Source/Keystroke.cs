
namespace HadoukInput
{
	/// <summary>
	/// These are the keystrokes (and combinations of keystrokes) that controller actions map to
	/// Used by the controller scrubber to clean up input
	/// </summary>
	public enum EKeystroke
	{
		//All the basic keystrokes
		Up,
		Down,
		Forward,
		Back,
		A,
		B,
		X,
		Y,
		LShoulder,
		RShoulder,
		LTrigger,
		RTrigger,

		//key-up keystrokes
		UpRelease,
		DownRelease,
		ForwardRelease,
		BackRelease,
		ARelease,
		BRelease,
		XRelease,
		YRelease,
		LShoulderRelease,
		RShoulderRelease,
		LTriggerRelease,
		RTriggerRelease,

		//keystrokes + directions

		AUp,
		ADown,
		AForward,
		ABack,

		BUp,
		BDown,
		BForward,
		BBack,

		XUp,
		XDown,
		XForward,
		XBack,

		YUp,
		YDown,
		YForward,
		YBack,

		LShoulderUp,
		LShoulderDown,
		LShoulderForward,
		LShoulderBack,

		RShoulderUp,
		RShoulderDown,
		RShoulderForward,
		RShoulderBack,

		LTriggerUp,
		LTriggerDown,
		LTriggerForward,
		LTriggerBack,

		RTriggerUp,
		RTriggerDown,
		RTriggerForward,
		RTriggerBack,

		NumKeystrokes
	}
}
