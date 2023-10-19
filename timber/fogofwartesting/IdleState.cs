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
        /* idle animation */
        float idle_scale_impact = (1.0f + Mathf.Sin(time * 4 + animation_offset) * 0.025f);

        /* apply */
        actor.view.Scale = new Vector3(actor.initial_scale.x, actor.initial_scale.y * idle_scale_impact, actor.initial_scale.z);

    }
}
