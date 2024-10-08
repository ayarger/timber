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

	[Signal] public delegate void stat_change(string stat_name);
	BarContainer container;
	// legacy code -> UIorb UIbar
	public float curr_health = 100;
	public float max_health = 100;
	public float health_ratio = 1;

	public override void _Ready()
	{
		container = BarContainer.Create(this);
	}


	// Refactor AddStat to send signals to BarContainer/UI Manager.
	public void AddStat(string name, int min, int max, int initial, bool display)
	{
		// Add new stat into the dictionary
		if (display && Stats_With_Bar.Count < 3)
		{
			Stats_With_Bar.Add(name);
			int index = Stats_With_Bar.Count - 1;

			if (!Stats.ContainsKey(name))
			{
				//create stat
				Stats[name] = new Stat(name, min, max, initial, display);

				//TODO: add to actorConfig

				//create bars
				if (name == "health")
				{
					Bar bar = container.CreatePrimary(name);
				}

				else
				{
					Bar bar = container.CreateSecondary(name);
					if (index == 2)
					{
						//TODO custom color
						bar.ChangeColor(new Color(0.33f, 0.63f, 0.35f, 1));
					}
				}
			}
			// TODO Prompt player to change display settings if > 3
		}

	}

	/// <summary>
    /// Remove stat and its corresponding bar
    /// </summary>
    /// <param name="name"></param>
	public void RemoveStat(string name)
    {
        if (Stats.ContainsKey(name))
        {
            if (Stats[name].displayOn)
            {
				Bar curr_bar = container.bar_dict[name];
				curr_bar.QueueFree();
            }
			Stats.Remove(name);
        }

		//TODO: remove from actorConfig
    }

	/// <summary>
    /// Get Stats
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
	public Stat GetStat(string name)
	{
		if (Stats.ContainsKey(name))
			return Stats[name];
		return null;
	}


	public void ToggleBarVisibility(string name, bool visible)
	{
		Bar curr_bar = container.bar_dict[name];

		if (curr_bar != null) {
			if (visible)
			{
				curr_bar.FadeIn(0.2f);
			}

            else
            {
				curr_bar.FadeOut(0.2f);
            }
		}
    }

	public override void _Process(float delta)
	{

	}
}
