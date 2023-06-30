using Godot;
using System;

public class PrimaryCanvas : CanvasLayer
{
    static PrimaryCanvas instance;

    public static void AddChildNode(Node node)
    {
        instance.AddChild(node);
    }

    public override void _Ready()
    {
        instance = this;
    }
}
