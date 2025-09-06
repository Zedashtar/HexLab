using Godot;
using System;
using System.Collections.Generic;
using HexUtilities;
using System.Linq;

public partial class EditorPatternRotateButton : Button
{
    [Export] public PatternHolder pattern_holder;
    [Export] public Hex.RotateDirection direction;

    public override void _Pressed()
    {
        pattern_holder.RotatePattern(direction);
    }
}
