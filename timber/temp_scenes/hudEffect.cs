using Godot;
using System;

public class hudEffect : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public Stat health;
    Tween tween;
    Subscription<StatChangeEvent> statChangeEvent;
    TextureProgress tex_progress;

    public override void _Ready()
    {
        tween = GetNode<Tween>("Tween");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
