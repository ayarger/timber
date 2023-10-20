using Godot;
using System;

// Temp StatManager for manageing stats
public class StatManager : Node
{
    [Export]
    public string node_name = "HasStats";
    private HasStats stats;

    public override void _Ready()
    {
        stats = GetNode<HasStats>("../" + node_name);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Scancode == (int)KeyList.Q)
            {
                GD.Print("adding health");
                stats.AddStat("health", 0, 100, 100, true);
            }

            else if (eventKey.Scancode == (int)KeyList.E)
            {
                GD.Print("adding shield");
                stats.AddStat("shield", 0, 100, 50, true);
            }

            else if (eventKey.Scancode == (int)KeyList.R)
            {
                GD.Print("adding xp");
                stats.AddStat("xp", 0, 100, 80, true);
            }

            // toggle visibility
        }
    }
}
