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
		hud = GetParent().GetParent().GetParent().GetNode<CanvasLayer>("CanvasLayer").GetNode<Control>("HUD");
		hud.GetNode<hudEffect>("PlayerProfile").health = stats.GetStat("health");
		//hud.GetNode<canDisplayStatsChange>("HBoxContainer/orb1/orbProgress").player_data = stats;
		hud.GetNode<canDisplayStatsChange>("HBoxContainer/orb2/orbProgress").player_data = stats;
	}

	public void Config(StatConfig config)
	{
		foreach(var i in config.stats)
		{
			stats.AddStat(i.Key, 0, (int)i.Value, (int)i.Value, true);
		}
	}
}
