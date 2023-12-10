using Godot;
using System;

public class hudEffect : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public Stat health;
    Tween tween;
    Subscription<StatChangeEvent> statChangeEvent;
    TextureProgress tex_progress;

    public override void _Ready()
    {
        tween = GetNode<Tween>("Tween");
        tex_progress = GetNode<TextureProgress>("Overlay");
        statChangeEvent = EventBus.Subscribe<StatChangeEvent>(OnUIStatChange);
    }

    public void OnUIStatChange(StatChangeEvent e)
    {
        float target_value = 100 - (health.Ratio * 100);
        // Stop any ongoing tween operation.
        tween.StopAll();
        //GD.Print(tex_progress.Value);
        tween.InterpolateProperty(
            tex_progress,
            "value",
            tex_progress.Value,
            target_value,
            0.5f,
            Tween.TransitionType.Linear,
            Tween.EaseType.InOut
        );
        // start tween 
        tween.Start();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
