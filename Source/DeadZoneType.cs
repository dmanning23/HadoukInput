namespace HadoukInput
{
	/// <summary>
	/// Different ways that thumbstick directions can be cleaned up.
	/// </summary>
	public enum DeadZoneType
	{
		None, //lol leave that shit alone
		Axial, //Normalized Tile-based (4-way) movement: The Axial Dead Zone actually works well here since it snaps analog input to the only four input vectors that are actually relevant.
		Radial, //normalized direciont-centric movement: radial works well here since it is a very small area in the center of the stick within which input is ignored.
		ScaledRadial, //as you push the stick away from the center, the gradient value changes smoothly while the dead zone is still preserved.
		PowerCurve //like scaled radial, but the small is smaller.  works good for sneaking games etc. where you want a definite analog feel
	}
}