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

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Scancode == (int)KeyList.Key1)
            {
                //GD.Print("adding shield");
                stats.AddStat("shield", 0, 100, 50, true);
            }

            else if (eventKey.Scancode == (int)KeyList.Key2)
            {
                //GD.Print("adding xp");
                stats.AddStat("xp", 0, 100, 80, true);
            }

            else if (eventKey.Scancode == (int)KeyList.Key6)
            {
                Stat health = stats.GetStat("health");
                if (health != null)
                {
                    GD.Print(health.currVal);
                    health.DecreaseCurrentValue(random.Next(10, 100));
                }
            }

            else if (eventKey.Scancode == (int)KeyList.Key7)
            {
                Stat health = stats.GetStat("health");
                if (health != null)
                {
                    GD.Print(health.currVal);
                    health.IncreaseCurrentValue(random.Next(10, 100));
                }
            }

            // toggle visibility testing  inside container 
        }
    }
}
