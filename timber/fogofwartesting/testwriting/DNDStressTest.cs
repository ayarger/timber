using Godot;
using System;

public class DNDStressTest : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    [Export]
    public NodePath fpsCounter;
    public RichTextLabel rtl;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rtl = GetNode<RichTextLabel>(fpsCounter);
        var scene = GD.Load<PackedScene>("res://fogofwartesting/testwriting/TestCSM.tscn");
        for (int i = 0; i < 500; i++)
        {
            var obj = scene.Instance();
            AddChild(obj);
        }
    }

    public override void _Process(float delta)
    {
        rtl.Text = Engine.GetFramesPerSecond().ToString();

    }//  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
