using Godot;
using System;

public class HealthManager : Node
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
        if (@event is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Scancode == (int)KeyList.J)
            {
                stats.ApplyDamage(10);  // Decrease health by 10
            }
            else if (eventKey.Scancode == (int)KeyList.K)
            {
                stats.ApplyHeal(10);  // Increase health by 10
            }
            else if(eventKey.Scancode == (int)KeyList.U)
            {
                stats.DecreaseMaxHealth(10);
            }
            else if(eventKey.Scancode == (int)KeyList.I)
            {
                stats.IncreaseMaxHealth(10);
            }
        }
    }
}
