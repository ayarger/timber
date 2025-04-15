using Godot;
using System;

public class ExitCutsceneEditor : Button
{
    private Control cutsceneEditor; 

    public override void _Ready()
    {
        Connect("pressed", this, nameof(OnExitButtonPressed));

        // Find the parent popup (assuming this button is inside the popup)
        cutsceneEditor = GetParent() as Control;
    }

    private void OnExitButtonPressed()
    {
        if (cutsceneEditor != null)
        {
            cutsceneEditor.Hide(); // Close the popup
        }
        else
        {
            GD.PrintErr("editor reference is null!");
        }
    }
}