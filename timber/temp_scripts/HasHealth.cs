using Godot;
using System;

public class HasHealth : Node
{
    [Export]
    private int maxHealth = 100; //max health
    [Export]
    private int currentHealth; //current health
    [Signal]
    public delegate void health_change();

    public override void _Ready()
    {
        currentHealth = maxHealth; // initialize current health
        GD.Print("currentHealth:" + currentHealth);
        GD.Print("maxHealth:" + maxHealth);
    }

    public override void _Process(float delta)
    {

    }

    public void ApplyDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
            currentHealth = 0;
        GD.Print("damage: " + damageAmount + " curr_health: " + currentHealth);

        if (currentHealth == 0)
        {
            // Handle death or other related logic here
            GD.Print("death event!");
        }

        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public void ApplyHeal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        GD.Print("heal: " + healAmount +  "curr_health: " + currentHealth);

        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        GD.Print("max_health increased to: " + maxHealth);

        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public void DecreaseMaxHealth(int amount)
    {
        maxHealth -= amount;
        if (maxHealth < 0)
            maxHealth = 0;
        //adjust curr_health
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        GD.Print("max_health decreased to: " + maxHealth);
        //signal for UI/sound 
        EmitSignal("health_change");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}