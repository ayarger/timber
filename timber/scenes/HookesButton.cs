using Godot;
using System;

public class HookesButton : Node
{
    Button parent_button;

    float desired_scale = 1.0f;

    public override void _Ready()
    {
        parent_button = GetParent<Button>();
    }

    public override void _Process(float delta)
    {
        PerformHookesScale();

        desired_scale = 1.0f;

        

        if (parent_button.IsHovered())
            desired_scale = 0.9f;

        else if (parent_button.IsPressed())
            desired_scale = 0.7f;            
    }

    float velocity = 0.0f;

    void PerformHookesScale()
    {
        float x = desired_scale - parent_button.RectScale.x;
        float k = 0.15f;

        float friction_factor = 0.9f;

        float a = x * k;
        velocity += a;
        velocity *= friction_factor;

        parent_button.RectScale += velocity * Vector2.One;
    }
}
