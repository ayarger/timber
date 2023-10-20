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
                stats.AddStat("health", 0, 100, 100, true);
            }

            else if (eventKey.Scancode == (int)KeyList.E)
            {
                stats.AddStat("shield", 0, 100, 50, true);
            }

            else if (eventKey.Scancode == (int)KeyList.R)
            {
                stats.AddStat("mana", 0, 100, 80, true);
            }

            // toggle visibility
        }
    }
}
