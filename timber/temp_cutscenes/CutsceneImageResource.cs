using Godot;

[GlobalClass]
public partial class CutsceneImageResource : Resource
{
    [Export] public Texture Image { get; set; }           // Image for the cutscene
    [Export] public string TransitionStyle { get; set; } = "instant"; // Transition style
    [Export] public string DisplayStyle { get; set; } = "standard";   // Display style
}