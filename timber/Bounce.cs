using Godot;
using System;

public class Bounce : Sprite
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    Vector2 initial_position;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        initial_position = Position;
    }

    float t = 0.0f;
    public override void _Process(float delta)
    {
        t += delta;

        float signal = Mathf.Abs(Mathf.Sin(t * 5));
        Position = initial_position + Vector2.Up * signal * 10;
    }
}
