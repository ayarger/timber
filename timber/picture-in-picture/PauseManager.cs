using Godot;
using System;

public class PauseManager : TextureButton
{
    private bool isPaused = false;

    public override void _Ready()
    {
        // Connect the button's "pressed" signal
        Connect("pressed", this, nameof(OnButtonPressed));

        // Allow this node to process input even when the game is paused
        PauseMode = PauseModeEnum.Process;
    }

    private void OnButtonPressed()
    {
        // Toggle the pause state
        isPaused = !isPaused;

        // Set the game's pause state
        GetTree().Paused = isPaused;

        // Print debug message
        GD.Print($"Pause state toggled: {isPaused}. Game is now {(isPaused ? "paused" : "running")}.");
    }
}
