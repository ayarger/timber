using Godot;
using System;

public class Bar : Control
{

    HasStats target_data;

    String data_name;

    int index;

    TextureProgress tex_progress;

    Tween ui_tween;

    public override void _Ready()
    {
        // get tex_progress
        tex_progress = GetNode<TextureProgress>("FrontProgress");
        // get target_tween
        ui_tween = GetNode<Tween>("UITween");
        // connect stats change event here
        target_data.Connect("stat_change", this, nameof(OnUIStatChange));
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

    /// <summary>
    /// Update UI on health change
    /// </summary>
    public void OnUIStatChange(string stat_name)
    {
        GD.Print("curr_stat_name: " + data_name, " incoming singal name: " + stat_name);
        if (stat_name == data_name)
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
                2f,
                Tween.TransitionType.Linear,
                Tween.EaseType.InOut
            );
            // start tween 
            ui_tween.Start();
        }
    }
}
