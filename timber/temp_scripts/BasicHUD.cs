using Godot;
using System;
/// <summary>
/// Basic hud script 
/// </summary>
public class BasicHUD : Control
{
    private float container_width;
    private Panel bar;
    private HasHealth healthSystem;
    public override void _Ready()
    {
        // get healthSystem
        healthSystem = GetNode<HasHealth>("../HasHealth");
        // hook health ui
        healthSystem.Connect("health_change", this, nameof(OnHealthChange));
        // initializing HUD
        Panel container = GetNode<Panel>("container");
        bar = GetNode<Panel>("bar");
        container_width = container.RectSize.x;
        bar.SetSize(new Vector2(container_width, bar.RectSize.y));
    }

    public void OnHealthChange()
    { 
        // update hud bar on health change
        float ratio =(float) healthSystem.GetCurrentHealth() / (float) healthSystem.GetMaxHealth();
        GD.Print(ratio);
        float curr_width = ratio * container_width;
        bar.SetSize(new Vector2(curr_width, bar.RectSize.y));
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
