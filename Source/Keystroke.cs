namespace HadoukInput
{
	/// <summary>
	/// These are the keystrokes (and combinations of keystrokes) that controller actions map to
	/// Used by the controller scrubber to clean up input
	/// </summary>
	public enum EKeystroke
	{
		//All the basic keystrokes

		// keyboard\dpad\left-thumbstick directions
		Up,
		Down,
		Forward,
		Back,
		Neutral,

		// right-thumbstick directions
		UpR,
		DownR,
		ForwardR,
		BackR,
		NeutralR,

		//button-down keystrokes
		A,
		B,
		X,
		Y,
		LShoulder,
		RShoulder,
		LTrigger,
		RTrigger,

		//Check some special stuff for the shoulder buttons...
		ForwardShoulder, //shoulder in the direction the player is facing
		BackShoulder, //shoulder in the other direction

		//button-up keystrokes
		ARelease,
		BRelease,
		XRelease,
		YRelease,
		LShoulderRelease,
		RShoulderRelease,
		LTriggerRelease,
		RTriggerRelease,

		//diagonals on right thumbstick
		UpForwardR,
		DownForwardR,
		UpBackR,
		DownBackR,

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

		//left_right thumbstick combination directions
		UpUp,
		UpUpForward,
		UpForward,
		UpDownForward,
		UpDown,
		UpDownBack,
		UpBack,
		UpUpBack,
		UpNeutral,

		ForwardUp,
		ForwardUpForward,
		ForwardForward,
		ForwardDownForward,
		ForwardDown,
		ForwardDownBack,
		ForwardBack,
		ForwardUpBack,
		ForwardNeutral,

		DownUp,
		DownUpForward,
		DownForward,
		DownDownForward,
		DownDown,
		DownDownBack,
		DownBack,
		DownUpBack,
		DownNeutral,

		BackUp,
		BackUpForward,
		BackForward,
		BackDownForward,
		BackDown,
		BackDownBack,
		BackBack,
		BackUpBack,
		BackNeutral,

		NeutralUp,
		NeutralUpForward,
		NeutralForward,
		NeutralDownForward,
		NeutralDown,
		NeutralDownBack,
		NeutralBack,
		NeutralUpBack,
		NeutralNeutral,

		NumKeystrokes
	}
}