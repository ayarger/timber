using Godot;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombatState : ActorState
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    int attackRange = 3;
    float attackCooldown = 1f;
    bool attackable = true;
    Actor TargetActor;

    float time = 0.0f;

    public override string name
    {
        get { return "CombatState"; }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    public override void Start()
    {
        inclusiveStates = new HashSet<string>();
        ArborCoroutine.StopCoroutinesOnNode(this);
        animation_offset = GD.Randf() * 100.0f;

    }

    public override void Update(float delta)
    {
        TargetActor = null;
        foreach (var actors in GetNode<LuaLoader>("/root/Main/LuaLoader").GetChildren())
        {
            var actorInRange = actors as Actor;
            if (actorInRange != null && actorInRange != actor)
            {
                Coord actorPos = Grid.ConvertToCoord(actorInRange.GlobalTranslation);
                Coord cur = Grid.ConvertToCoord(actor.GlobalTranslation);
                if ((cur - actorPos).Mag() <= attackRange)
                {
                    TargetActor = actorInRange;
                    break;
                }
            }
        }

        if(TargetActor == null)
        {
            manager.DisableState("CombatState");
            return;
        }

        if (attackable)
        {
            DamageTextManager.DrawText(9, TargetActor);
            attackable = false;
            ArborCoroutine.StartCoroutine(refreshCooldown(), this);
        }
    }

    IEnumerator refreshCooldown()
    {
        yield return ArborCoroutine.WaitForSeconds(attackCooldown);
        attackable = true;
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
