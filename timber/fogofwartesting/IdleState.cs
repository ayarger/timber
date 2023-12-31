using Godot;
using System;

public class IdleState : ActorState
{
    public override string name
    {
        get
        {
            return "Idle";
        }
    }

    //Inclusive States should always be empty for idle state.

    float time = 0.0f;
    public override void Start()
    {
        animation_offset = GD.Randf() * 100.0f;
    }

    float animation_offset = 0;
    public override void Animate(float delta)
    {
        time += delta;

        /* Return unit to cell if it has been moved by other states */
        actor.view.Translation += (Vector3.Zero - actor.view.Translation) * 0.1f;

        /* idle / breathing animation */
        float idle_scale_impact = (1.0f + Mathf.Sin(time * 4 + animation_offset) * 0.025f);

        /* Paper Turning */
        float current_scale_x = actor.view.Scale.x;
        current_scale_x += (actor.GetDesiredScaleX() - current_scale_x) * 0.2f;
        actor.view.Scale = new Vector3(current_scale_x, actor.initial_view_scale.y * idle_scale_impact, actor.view.Scale.z);
    }
}
