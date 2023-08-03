using Godot;
using System;

public class UISinMovement : Label
{
    [Export]
    float offset = 0.0f;
    [Export]
    float amplitude = 10.0f;
    [Export]
    float frequency_factor = 2.0f;

    Vector2 initial_position;

    public override void _Ready()
    {
        initial_position = RectPosition;
    }

    public override void _Process(float delta)
    {
        float signal = Mathf.Sin(frequency_factor * OS.GetTicksMsec() / 1000.0f + offset) * amplitude;
        Vector2 desired_position = initial_position + Vector2.Up * signal;
        RectPosition = desired_position;
    }
}
