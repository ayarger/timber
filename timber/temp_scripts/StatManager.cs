using Godot;
using System;

// Temp StatManager for manageing stats
public class StatManager : Node
{
    [Export]
    public string node_name = "HasStats";
    private HasStats stats;
    private Random random = new Random();
    Control hud;

    public override void _Ready()
    {
        stats = GetNode<HasStats>("../" + node_name);
        stats.AddStat("health", 0, 100, 100, true);
        hud = GetParent().GetParent().GetParent().GetNode<CanvasLayer>("CanvasLayer").GetNode<Control>("HUD");
        hud.GetNode<hudEffect>("PlayerProfile").health = stats.GetStat("health");
        //hud.GetNode<canDisplayStatsChange>("HBoxContainer/orb1/orbProgress").player_data = stats;
        hud.GetNode<canDisplayStatsChange>("HBoxContainer/orb2/orbProgress").player_data = stats;
    }
}
