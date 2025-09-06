using Godot;
using System;
using System.Collections.Generic;
using HexUtilities;
using System.Linq;

public partial class EditorPatternTranslateButton : Button
{
    [Export] public PatternHolder pattern_holder;
    [Export] public Vector3I translation;

    public override void _Pressed()
    {
        pattern_holder.TranslatePattern(new Hex(translation));
    }
}
