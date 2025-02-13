using System;
using Godot;

[Tool]
public partial class CutsceneResource : Resource
{
    [Export] public Texture Image { get; set; }
    [Export] public string ImagePath { get; set; } = "res://temp_cutscenes/intro_images/1.png"; // image path
    [Export] public string TransitionStyle { get; set; } = "instant"; // Transition style
    [Export] public string DisplayStyle { get; set; } = "standard";   // Display style

    [Export] public int Index { get; set; } = 0; //index;
}