using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Generic stat class with minimum, current, maximum value and stat specific UI info
/// </summary>
public class Stat
{
    public string Name { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public float CurrentValue { get; set; }
    public bool DisplayBar { get; set; }
    // TODO UI styling info
    // maybe bar colors?
    public float Ratio = 1;

    //signal for UI
    [Signal]
    public delegate void stat_change (string stat_name);

    public Stat(string name, float minValue, float maxValue, float initialValue, bool displayBar)
    {
        Name = name;
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = initialValue;
        DisplayBar = displayBar;
    }

    /// <summary>
    /// Example defualt stat constructor
    /// </summary>
    public Stat()
    {
        Name = "health";
        MinValue = 0;
        MaxValue = 100;
        CurrentValue = 100;
        DisplayBar = true;
    }

    /// <summary>
    /// Ratio of current value to max value.
    /// Used for UI elements like progress bars.
    /// </summary>

    public void IncreaseCurrentValue(float amount)
    {
        CurrentValue += amount;
        ClampCurrentValue();
    }

    public void DecreaseCurrentValue(float amount)
    {
        CurrentValue -= amount;
        ClampCurrentValue();
    }

    public void IncreaseMaxValue(int amount)
    {
        MaxValue += amount;
        ClampCurrentValue();
    }

    public void DecreaseMaxValue(int amount)
    {
        MaxValue -= amount;
        if (MaxValue < MinValue)
            MaxValue = MinValue;
        ClampCurrentValue();
    }

    private void ClampCurrentValue()
    {
        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
        Ratio = CurrentValue / MaxValue;
        // Can't emit signal from here since stat is not a node
        if (DisplayBar)
        {
            // TODO: maybe use event bus to publish stat_change event
            // Inside has stats class, emit health_change signal with the stat_name 
        }
    }
}

public class HasStats : Node
{
    /// <summary>
    /// Stats Dictionary
    /// </summary>
    public Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();
    public Dictionary<string, Stat> Stats_With_Bar = new Dictionary<string, Stat>();

    public float curr_health = 100;
    public float max_health = 100;
    public float health_ratio = 1;
    //TODO: max 3 overhead progress bar can be stacked together
    [Signal] public delegate void health_change();

    public override void _Ready()
    {
        // TODO:
        // UIOrb.Create(this);
        AddStat("health", 0, 100, 100, true);
        AddStat("mana", 0, 100, 100, true);
        UIBar.Create(this);
    }

    public void AddStat(string name, int min, int max, int initial, bool display)
    {
        // TODO: UI_count max = 3
        // Prompt player to change display settings if > 3
        if (display)
            Stats_With_Bar[name] = new Stat(name, min, max, initial, display);
        else
            Stats[name] = new Stat(name, min, max, initial, display);
    }

    public Stat GetStat(string name)
    {
        if (Stats.ContainsKey(name))
            return Stats[name];
        return null;
    }

    public void ApplyDamage(int damageAmount)
    {
        var health = GetStat("Health");
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
