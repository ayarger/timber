using Godot;
using System;

public class Bar : Control
{

	HasStats target_data;

	String data_name;

	int index;

	TextureProgress tex_progress;

	Tween ui_tween;

	Subscription<StatChangeEvent> statChangeEvent;

	public override void _Ready()
	{
		// get tex_progress
		tex_progress = GetNode<TextureProgress>("FrontProgress");
		// get target_tween
		ui_tween = GetNode<Tween>("UITween");
		// connect stats change event here
		//target_data.Connect("stat_change", this, nameof(OnUIStatChange));
		statChangeEvent = EventBus.Subscribe<StatChangeEvent>(OnUIStatChange);
	}

	public void Configure(HasStats _target, string _data_name)
	{
		target_data = _target;
		data_name = _data_name;
	}

	public void ChangeColor(Color _color)
	{
		tex_progress.SelfModulate = _color;
	}

	public void toggleVisible(bool visible)
	{
		Visible = visible;
	}

	/// <summary>
	/// Update UI on health change
	/// </summary>
	public void OnUIStatChange(StatChangeEvent e)
	{
		//GD.Print("curr_stat_name: " + data_name, " incoming singal name: " + e.stat_name);
		if (e.stat_name == data_name)
		{
			float target_value = target_data.Stats[data_name].Ratio * 100;
			// Stop any ongoing tween operation.
			ui_tween.StopAll();
			//GD.Print(tex_progress.Value);
			ui_tween.InterpolateProperty(
				tex_progress,
				"value",
				tex_progress.Value,
				target_value,
				0.5f,
				Tween.TransitionType.Linear,
				Tween.EaseType.InOut
			);
			// start tween 
			ui_tween.Start();
		}
	}

	public void OnCreate()
	{
		RectScale = new Vector2(0, 0);

		// Setup the tween to scale the child from 0 to 1 (original size).
		ui_tween.InterpolateProperty(this,"rect_scale", RectScale,new Vector2(1, 1), 0.3f,Tween.TransitionType.Back, Tween.EaseType.Out);
		ui_tween.Start(); // Start the tween.
	}

	public void OnCreate2()
	{
	   Color currentColor = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
		FadeIn(1);
		ui_tween.Start();

	}

	public void FadeIn(float duration)
	{
		Color currentColor = Modulate;
		ui_tween.InterpolateProperty(this, "modulate:a", currentColor.a, 1, duration, Tween.TransitionType.Linear, Tween.EaseType.Out);
		ui_tween.Start();
	}

	public void FadeOut(float duration)
	{
		Color currentColor = Modulate;
		ui_tween.InterpolateProperty(this, "modulate:a", currentColor.a, 0, duration, Tween.TransitionType.Linear, Tween.EaseType.In);
		ui_tween.Start();
	}

    public override void _ExitTree()
    {
        if(statChangeEvent != null)
        {
			EventBus.Unsubscribe(statChangeEvent);
			statChangeEvent = null;
        }

		if (ui_tween != null)
		{
			ui_tween.StopAll();
		}
		base._ExitTree();
	}

	// TODO destroy the UI bar on stat remove event
}
