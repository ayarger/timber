using Godot;
using System;

public class HasStats : Node
{
    [Export]
    public float max_health = 100;
    [Export]
    public float curr_health = 100;
    public float health_ratio = 1;
    [Signal]
    public delegate void health_change();
    public override void _Ready()
    {
        //UIOrb.Create(this);
        UIBar.Create(this);
    }

    public void ApplyDamage(int damageAmount)
    {
        curr_health -= damageAmount;
        if (curr_health < 0)
            curr_health = 0;

        if (curr_health == 0)
        {
            // Handle death or other related logic here
            GD.Print("death event!");
        }

        //update ratio
        health_ratio = curr_health / max_health;
        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public void ApplyHeal(float healAmount)
    {
        curr_health += healAmount;
        if (curr_health > max_health)
            curr_health = max_health;

        //update ratio
        health_ratio = curr_health / max_health;
        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public void IncreaseMaxHealth(float amount)
    {
        max_health += amount;
        //update ratio
        health_ratio = curr_health / max_health;
        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public void DecreaseMaxHealth(float amount)
    {
        max_health -= amount;
        if (max_health < 0)
            max_health = 0;
        //adjust curr_health
        if (curr_health > max_health)
            curr_health = max_health;

        //update ratio
        health_ratio = curr_health / max_health;
        //signal for UI/sound 
        EmitSignal("health_change");
    }



    public override void _Process(float delta)
    {
        
    }
}
