using Godot;
using System;

public class canDisplayStatsChange : TextureProgress
{

    public HasStats player_data;

    // can be modified; will be used to fetch the corresponding player data
    public String data_name;

    TextureProgress tex_progress;

    Tween ui_tween;

    Subscription<StatChangeEvent> statChangeEvent;

    public override void _Ready()
    {
        data_name = "health";
        // get tex_progress
        tex_progress = this;
        // get target_tween
        ui_tween = GetNode<Tween>("Tween");
        //statChangeEvent = EventBus.Subscribe<StatChangeEvent>(OnUIStatChange);
    }

    public void ConfigurePlayerData(HasStats _playerData)
    {
        player_data = _playerData;
    }

    //TODO: choose the data to be

    public void ChangeColor(Color _color)
    {
        tex_progress.SelfModulate = _color;
    }

    public void toggleVisible(bool visible)
    {
        Visible = visible;
    }

    /// <summary>
    /// Update HUD UI on stats change
    /// </summary>
    public void OnUIStatChange(StatChangeEvent e)
    {
        //check if stats change should be displayed by this specifc orb
        GD.Print("curr_stat_name: " + data_name, " incoming singal name: " + e.stat_name);
        if (e.stat_name == data_name)
        {
            float target_value = player_data.Stats[data_name].Ratio * 100;
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

        // TODO destroy the UI bar on stat remove event
}
