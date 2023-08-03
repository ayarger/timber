using Godot;
using System;

public class OrbitCam : Camera
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    float t = 0.0f;

    public override void _Process(float delta)
    {
        if (EditModeManager.edit_mode)
            return;

        t += delta;
        Vector3 desired_position = new Vector3(Mathf.Cos(t), 1.0f, Mathf.Sin(t)) * 5.0f;
        GlobalTranslation = desired_position;
        LookAt(Vector3.Zero, Vector3.Up);
    }
}
