using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public bool has_idle_animation = true;
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

        float idle_scale_impact = 1;
        /* idle / breathing animation */
        if (has_idle_animation)
        {
            idle_scale_impact = (1.0f + Mathf.Sin(time * 4 + animation_offset) * 0.025f);

        }

        /* Paper Turning */
        float current_scale_x = actor.view.Scale.x;
        current_scale_x += (actor.GetDesiredScaleX() - current_scale_x) * 0.2f;
        actor.view.Scale = new Vector3(current_scale_x, actor.initial_view_scale.y * idle_scale_impact, actor.view.Scale.z);
    }

    public void SetAnimationOffset(float val)
    {
        animation_offset = val;
    }

    int detectionRange = 3;
    public override void Update(float delta)
    {
        if (actor.GetNode<HasTeam>("HasTeam").team == "enemy")
        {
            foreach (var actors in GetAttackableActorList())
            {
                var actorInRange = actors as Actor;
                if (actorInRange != null && actorInRange.GetNode<HasTeam>("HasTeam").team == "player")
                {
                    Coord actorPos = Grid.ConvertToCoord(actorInRange.GlobalTranslation);
                    Coord cur = Grid.ConvertToCoord(actor.GlobalTranslation);
                    float dist = Math.Abs(actorPos.x - cur.x)
                + Math.Abs(actorPos.z - cur.z);

                    if (dist <= detectionRange)
                    {
                        CombatState cs = manager.states["CombatState"] as CombatState;
                        cs.TargetActor = actorInRange;
                        manager.EnableState("CombatState");
                    }
                }
            }
        } 
        else if (actor.GetNode<HasTeam>("HasTeam").team == "player")
        {
            foreach (var actors in GetAttackableActorList())
            {
                var actorInRange = actors as Actor;
                if (actorInRange != null && actorInRange.GetNode<HasTeam>("HasTeam").team == "construction")
                {
                    Coord actorPos = Grid.ConvertToCoord(actorInRange.GlobalTranslation);
                    Coord cur = Grid.ConvertToCoord(actor.GlobalTranslation);
                    float dist = Math.Abs(actorPos.x - cur.x)
                                 + Math.Abs(actorPos.z - cur.z);

                    if (dist <= detectionRange)
                    {
                        CombatState cs = manager.states["ConstructionState"] as CombatState;
                        cs.TargetActor = actorInRange;
                        manager.EnableState("ConstructionState");
                    }
                }
            }

            if (actor.GetNode<Tower>(".") != null)
            {
                foreach (var actors in GetAttackableActorList())
                {
                    var actorInRange = actors as Actor;
                    if (actorInRange != null && actorInRange.GetNode<HasTeam>("HasTeam").team == "enemy")
                    {
                        Coord actorPos = Grid.ConvertToCoord(actorInRange.GlobalTranslation);
                        Coord cur = Grid.ConvertToCoord(actor.GlobalTranslation);
                        float dist = Math.Abs(actorPos.x - cur.x)
                                     + Math.Abs(actorPos.z - cur.z);

                        if (dist <= detectionRange)
                        {
                            CombatState cs = manager.states["CombatState"] as CombatState;
                            cs.TargetActor = actorInRange;
                            manager.EnableState("CombatState");
                        }
                    }
                }
            }
        }

    }
}