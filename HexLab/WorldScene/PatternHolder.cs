using Godot;
using System.Collections.Generic;
using HexUtilities;

public partial class PatternHolder : TilesManager
{
	public List<Hex> currentPattern = new List<Hex>();
	public void RotatePattern(Hex.RotateDirection direction)
	{
		currentPattern = PatternUtility.Rotate(currentPattern, direction);
		DisplayCurrentPattern();
	}

	public void TranslatePattern(Hex translation)
	{
		currentPattern = PatternUtility.Translate(currentPattern, translation);
		DisplayCurrentPattern();
	}

	public void CenterPattern()
	{
		currentPattern = PatternUtility.Center(currentPattern);
		DisplayCurrentPattern();
	}

	public void DisplayCurrentPattern()
	{
		ClearTiles();
		AddTileMultiple(currentPattern);
	}
}
