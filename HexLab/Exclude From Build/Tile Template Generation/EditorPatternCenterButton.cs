using Godot;
using System;
using System.Collections.Generic;
using HexUtilities;
using System.Linq;

public partial class EditorPatternCenterButton : Button
{
    [Export] public PatternHolder pattern_holder;

    public override void _Pressed()
    {
        pattern_holder.CenterPattern();
    }
}
