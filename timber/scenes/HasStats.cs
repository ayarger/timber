using Godot;
using System;
using System.Collections.Generic;


/// <summary>
/// StatOnChange Event
/// </summary>
public class StatChangeEvent
{
    public string stat_name;
    public StatChangeEvent(string _stat_name)
    {
        stat_name = _stat_name;
    }
}
/// <summary>
/// Generic stat class with minimum, current, maximum value and stat specific UI info
/// </summary>
/// 
public class Stat
{
    public string name { get; set; }
    public float minVal { get; set; }
    public float maxVal { get; set; }
    public float currVal { get; set; }
    public bool displayOn { get; set; }
    public Color barColo { get; set; }
    // TODO UI styling info
    // maybe bar colors?
    public float Ratio = 1;


    //signal for UI
    [Signal]
    public delegate void stat_change(string stat_name);

    public Stat(string _name, float _minVal, float _maxVal, float _initialVal, bool _displayOn)
    {
        name = _name;
        minVal = _minVal;
        maxVal = _maxVal;
        currVal = _initialVal;
        displayOn = _displayOn;
    }

    /// <summary>
    /// Example defualt stat constructor
    /// </summary>
    public Stat()
    {
        name = "health";
        minVal = 0;
        maxVal = 100;
        currVal = 100;
        displayOn = true;
    }

    /// <summary>
    /// Ratio of current value to max value.
    /// Used for UI elements like progress bars.
    /// </summary>

    public void IncreaseCurrentValue(float amount)
    {
        currVal += amount;
        ClampCurrentValue();
    }

    public void DecreaseCurrentValue(float amount)
    {
        currVal -= amount;
        ClampCurrentValue();
    }

    public void IncreaseMaxValue(int amount)
    {
        maxVal += amount;
        ClampCurrentValue();
    }

    public void DecreaseMaxValue(int amount)
    {
        maxVal -= amount;
        if (maxVal < minVal)
            maxVal = minVal;
        ClampCurrentValue();
    }

    private void ClampCurrentValue()
    {
        currVal = Mathf.Clamp(currVal, minVal, maxVal);
        Ratio = currVal / maxVal;
        // publish statChange event when clamp function is called
        EventBus.Publish<StatChangeEvent>(new StatChangeEvent(name));
        GD.Print(name + "stat change event published");

        // Can't emit signal from here since stat is not a node
        if (displayOn)
        {
            // TODO: maybe use event bus to publish stat_change event
            // Inside has stats class, emit health_change signal with the stat_name 
        }
    }
}

public class HasStats : Node
{
    // subsription for statChangeEvent
    Subscription<StatChangeEvent> statChangeEvent;
    /// <summary>
    /// Stats Dictionary
    /// </summary>
    public Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();
    public List<string> Stats_With_Bar = new List<string>();

    public float curr_health = 100;
    public float max_health = 100;
    public float health_ratio = 1;
    //TODO: max 3 overhead progress bar can be stacked together
    [Signal] public delegate void stat_change(string stat_name);

    public override void _Ready()
    {
        statChangeEvent = EventBus.Subscribe<StatChangeEvent>(OnStatChange);
        // TODO:
        // UIOrb.Create(this);
    }

    public void OnStatChange(StatChangeEvent e)
    {
        EmitSignal("stat_change", e.stat_name);
    }

    public void AddStat(string name, int min, int max, int initial, bool display)
    {
        // TODO: UI_count max = 3
        // Prompt player to change display settings if > 3
        if (display && Stats_With_Bar.Count < 3)
        {
            Stats_With_Bar.Add(name);
            int index = Stats_With_Bar.Count - 1;
            UIBar.Create(this, name, index);
        }


        else
            Stats[name] = new Stat(name, min, max, initial, display);
        GD.Print("new stat created: " + name);
    }

    // TODO: remove stat

    public Stat GetStat(string name)
    {
        if (Stats.ContainsKey(name))
            return Stats[name];
        return null;
    }

    public void ApplyDamage(int damageAmount)
    {
        var health = GetStat("health");
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
        EmitSignal("stat_change", "health");
    }

    public void ApplyHeal(float healAmount)
    {
        curr_health += healAmount;
        if (curr_health > max_health)
            curr_health = max_health;

        //update ratio
        health_ratio = curr_health / max_health;
        //signal for UI/sound 
        EmitSignal("stat_change", "health");
    }

    public void IncreaseMaxHealth(float amount)
    {
        max_health += amount;
        //update ratio
        health_ratio = curr_health / max_health;
        //signal for UI/sound 
        EmitSignal("stat_change", "health");
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
        EmitSignal("stat_change", "health");
    }



    public override void _Process(float delta)
    {

    }

}
