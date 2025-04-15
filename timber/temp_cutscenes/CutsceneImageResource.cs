using System;
using Godot;

[Tool]
public partial class CutsceneImageResource : Resource
{
    [Export] public Texture Image { get; set; }
    [Export] public string ImagePath { get; set; } = ""; // image path
    [Export] public string TransitionStyle { get; set; } = "instant"; // Transition style
    [Export] public string DisplayStyle { get; set; } = "standard";   // Display style

    [Export] public int Index { get; set; } = 0; //index;
}