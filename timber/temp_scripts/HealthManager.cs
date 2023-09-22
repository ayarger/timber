using Godot;
using System;

public class HealthManager : Node
{
    [Export]
    public string health_name = "HasHealth"; 
    private HasHealth healthSystem;

    public override void _Ready()
    {
        healthSystem = GetNode<HasHealth>("../" + health_name);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Scancode == (int)KeyList.J)
            {
                healthSystem.ApplyDamage(10);  // Decrease health by 10
            }
            else if (eventKey.Scancode == (int)KeyList.K)
            {
                healthSystem.ApplyHeal(10);  // Increase health by 10
            }
            else if(eventKey.Scancode == (int)KeyList.U)
            {
                healthSystem.DecreaseMaxHealth(10);
            }
            else if(eventKey.Scancode == (int)KeyList.I)
            {
                healthSystem.IncreaseMaxHealth(10);
            }
        }
    }
}
