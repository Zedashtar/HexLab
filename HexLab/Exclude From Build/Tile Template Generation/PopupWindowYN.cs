using Godot;
using System;

public partial class PopupWindowYN : Panel
{
    [Signal] public delegate void YesPressedEventHandler();
    [Signal] public delegate void NoPressedEventHandler();

    public void Close()
    {
        Visible = false;
        MouseFilter = Control.MouseFilterEnum.Pass;
    }

    public void Open()
    {
        Visible = true;
        MouseFilter = Control.MouseFilterEnum.Stop;
    }

    void _on_buttonY_down()
    {
        EmitSignal(SignalName.YesPressed);
        Close();
    }

    void _on_buttonN_down()
    {
        EmitSignal(SignalName.NoPressed);
        Close();
    }
}
