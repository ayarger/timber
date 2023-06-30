using Godot;
using System;

public class Hookes : Node
{
    Control parent;

    public override void _Ready()
    {
        parent = GetParent<Control>();
        parent.RectPivotOffset = parent.RectSize * 0.5f;

        //parent.RectSize *= 2.0f;
    }

    public override void _Process(float delta)
    {
        PerformHookesScale();
    }

    float velocity = 0.0f;

    void PerformHookesScale()
    {
        float x = 1.0f - parent.RectScale.x;
        float k = 0.05f;

        float friction_factor = 0.9f;

        float a = x * k;
        velocity += a;
        velocity *= friction_factor;

        parent.RectScale += velocity * Vector2.One;
    }
}
