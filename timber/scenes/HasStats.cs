using Godot;
using System;

public class HasStats : Node
{
    public float health = 100;
    public override void _Ready()
    {
        health = GD.Randf() * 100.0f;

        UIOrb.Create(this);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
