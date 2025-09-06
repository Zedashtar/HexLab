using Godot;
using System.Collections.Generic;
using HexUtilities;

public partial class PatternHolder : TilesManager
{
	public List<Hex> currentPattern = new List<Hex>();
	public void RotatePattern(Hex.RotateDirection direction)
	{
		SetDisplayedPattern(PatternUtility.Rotate(currentPattern, direction));
	}

	public void TranslatePattern(Hex translation)
	{
		SetDisplayedPattern(PatternUtility.Translate(currentPattern, translation));
	}

	public void CenterPattern()
	{
		SetDisplayedPattern(PatternUtility.Center(currentPattern));
	}

	public void SetDisplayedPattern(List<Hex> pattern)
	{
		currentPattern = pattern;
		ClearTiles();
		AddTileMultiple(currentPattern);
	}
}
