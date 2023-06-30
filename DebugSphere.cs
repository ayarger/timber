using Godot;
using System;

public class DebugSphere : CSGSphere
{
    static DebugSphere instance;
    public static void VisualizePoint(Vector3 p)
    {
        instance.GlobalTranslation = p;
    }

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        instance = this;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
