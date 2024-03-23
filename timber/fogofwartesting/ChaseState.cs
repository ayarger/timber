using Godot;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;

//NOT IN USE
public class ChaseState : ActorState
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    int attackRange = 2;//number of grids
    int aggroRange = 3;

    bool attackable = true;
    bool attacking = false;

    public Actor TargetActor;
    Coord targetCoord;

    float time = 0.0f, rotateTime = 0.0f;

    public override string name
    {
        get { return "ChaseState"; }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    public override void Start()
    {
        inclusiveStates = new HashSet<string>();
        inclusiveStates.Add("MovementState");
        ArborCoroutine.StopCoroutinesOnNode(this);
        attackable = true;

    }

    public override void Update(float delta)
    {
        if (TargetActor != null && IsInstanceValid(TargetActor))
        {
            Vector3 dest = Grid.LockToGrid(TargetActor.GlobalTranslation);

            float dist = Math.Abs(Grid.ConvertToCoord(actor.GlobalTranslation).x - Grid.ConvertToCoord(dest).x)
                + Math.Abs(Grid.ConvertToCoord(actor.GlobalTranslation).z - Grid.ConvertToCoord(dest).z);

            if (dist <= attackRange)
            {
                if(manager.IsStateActive("MovementState"))
                    manager.DisableState("MovementState");
            }
        }
        else
        {
            manager.DisableState("ChaseState");
            return;
        }
    }
}
