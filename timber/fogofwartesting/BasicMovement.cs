using Godot;
using System;

public class BasicMovement : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    // called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.G))
        {
            GlobalTranslation += Vector3.Back * delta * 5f;
        }
        if (Input.IsKeyPressed((int)KeyList.T))
        {
            GlobalTranslation += Vector3.Forward * delta * 5f;
        }
        if (Input.IsKeyPressed((int)KeyList.F))
        {
            GlobalTranslation += Vector3.Left * delta * 5f;
        }
        if (Input.IsKeyPressed((int)KeyList.H))
        {
            GlobalTranslation += Vector3.Right * delta * 5f;
        }
    }
}
