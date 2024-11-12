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
            return "IdleState";
        }
    }

    //Inclusive States should always be empty for idle state.

    float time = 0.0f;
    public bool has_idle_animation = true;

    public override void Start()
    {
        //actor.SetActorTexture("spot_idle.png");
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

        float current_scale_y = actor.view.Scale.y;
        if (actor.GetNode<Actor>(".").GetActorConfig().type == "tower")
        {
            current_scale_y += (actor.initial_view_scale.y - actor.view.Scale.y) * 0.3f;
        }
        else
        {
            current_scale_y = actor.initial_view_scale.y * idle_scale_impact;
        }
        
        /* Paper Turning */
        float current_scale_x = actor.view.Scale.x;
        current_scale_x += (actor.GetDesiredScaleX() - current_scale_x) * 0.2f;
        actor.view.Scale = new Vector3(current_scale_x, current_scale_y, actor.view.Scale.z);
    }
    
    

    public void SetAnimationOffset(float val)
    {
        animation_offset = val;
    }

    int detectionRange = 3;

    public override void Update(float delta)
    {
        if (!manager.states.ContainsKey("CombatState")) return;
        HasTeam team = actor.GetNode<HasTeam>("HasTeam");
        
        if (team != null && team.team == "enemy")//only enemy actor has aggro right now
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
                        ChaseState cs = manager.states["ChaseState"] as ChaseState;
                        cs.TargetActor = actorInRange;
                        manager.EnableState("ChaseState");
                    }
                }
            }
        }
        else if (actor.GetNode<HasTeam>("HasTeam").team == "player")
        {
            // For construction units to actively search for constructions
            if (manager.states.ContainsKey("ConstructionState"))
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
            }
            // For tower units to actively search for enemys
            if (actor.GetNode<Actor>(".").GetActorConfig().type == "tower")
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