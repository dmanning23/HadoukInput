
namespace HadoukInput
{
	/// <summary>
	/// Different ways that thumbstick directions can be cleaned up.
	/// </summary>
	public enum ThumbstickType
	{
		Scrubbed, //checks the deadzone and returns the direction as a unit vector.
		PowerCurve //returns thumbstick direction as a vector between 0.0-1.0, but cleaned up with a power curve.
	}
}
