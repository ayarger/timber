using Godot;
using System;

public class ContinueScreenSpotlight : Sprite
{
    Vector2 initial_position;

    public override void _Ready()
    {
        initial_position = GlobalPosition;
    }

    public override void _Process(float delta)
    {
        float t = 1.5f * Time.GetTicksMsec() / 1000.0f;
        Vector2 desired_position = initial_position + new Vector2(Mathf.Cos(t) * 20.0f, Mathf.Sin(t) * 10.0f);
        GlobalPosition = desired_position;
    }
}
