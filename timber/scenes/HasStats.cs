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
	{        stat_name = _stat_name;
	}
}

/// <summary>
/// Options to display the stat & how it is displayed
/// </summary>

public enum DisplayOptions
{
	None,
	Overhead,
	HUD,
	OverheadAndHUD
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
	public Color barColor { get; set; }

	// TODO UI styling info
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

	public void SetVal(int amount)
	{
		currVal = amount;
		ClampCurrentValue();
	}

	public void SetMaxVal(int amount)
	{
		maxVal = amount;
		ClampCurrentValue();
	}

	public void toggleUI()
	{
		if (displayOn)
		{
			displayOn = false;
		}

		else
		{
			displayOn = true;
		}
	}

	private void ClampCurrentValue()
	{
		currVal = Mathf.Clamp(currVal, minVal, maxVal);
		Ratio = currVal / maxVal;
		// publish statChange event when clamp function is called
		EventBus.Publish<StatChangeEvent>(new StatChangeEvent(name));
		GD.Print(name + "stat change event published");
	}
}

/// <summary>
/// Character stats system
/// </summary>
public class HasStats : Node
{
	// subsription for statChangeEvent

	/// <summary>
	/// Stats Dictionary
	/// </summary>
	/// 
	public Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();
	public List<string> Stats_With_Bar = new List<string>();

	public float curr_health = 100;
	public float max_health = 100;
	public float health_ratio = 1;
	//TODO: max 3 overhead progress bar can be stacked together
	[Signal] public delegate void stat_change(string stat_name);

	BarContainer container;

    public override void _Ready()
    {
        container = BarContainer.Create(this);
        EventBus.Subscribe<StatChangeEvent>(updateOnStatChanged);
    }

    public void updateOnStatChanged(StatChangeEvent e)
    {
		if (Stats.ContainsKey(e.stat_name)){
			if(Stats[e.stat_name].Ratio != 1)
			{
				container.ShowOnStatChanged();
			}
		}
        
    }


	// Refactor AddStat to send signals to BarContainer/UI Manager.
	public void AddStat(string name, int min, int max, int initial, bool display)
	{
		GD.Print("Adding stat: " + name + " min: " + min + " max: " + max + " initial: " + initial + " display: " + display);
		// Add new stat into the dictionary
		if (display && Stats_With_Bar.Count < 3)
		{
			Stats_With_Bar.Add(name);
			int index = Stats_With_Bar.Count - 1;
			
			// TODO create bars

			if (name == "health" && !Stats.ContainsKey(name))
			{
				Bar bar = container.CreatePrimary(name);
			}

			else
			{
				Bar bar =  container.CreateSecondary(name);
				if (index == 2)
				{
					//GD.Print("green");
					bar.ChangeColor(new Color(0.33f, 0.63f, 0.35f, 1));
				}
			}
		}
		// TODO Prompt player to change display settings if > 3
		Stats[name] = new Stat(name, min, max, initial, display);
	}

	// TODO: remove stat

	public Stat GetStat(string name)
	{
		if (Stats.ContainsKey(name))
			return Stats[name];
		return null;
	}



	public override void _Process(float delta)
	{

	}

}
