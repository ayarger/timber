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
        if (TargetActor != null)//TODO check if actor is dead
        {
            Vector3 dest = Grid.LockToGrid(TargetActor.GlobalTranslation);
            MovementState b = (manager.states["MovementState"] as MovementState);

            float dist = Math.Abs(Grid.ConvertToCoord(actor.GlobalTranslation).x - Grid.ConvertToCoord(dest).x)
                + Math.Abs(Grid.ConvertToCoord(actor.GlobalTranslation).z - Grid.ConvertToCoord(dest).z);

            if (dist <= attackRange || dist > aggroRange)
            {
                manager.DisableState("MovementState");
                manager.DisableState("ChaseState");
                return;
            }
            else if (!manager.IsStateActive("MovementState") || Grid.ConvertToCoord(actor.GlobalTranslation) != targetCoord)
            {
                ArborCoroutine.StopCoroutinesOnNode(b);
                ArborCoroutine.StartCoroutine(TestMovement.PathFindAsync(actor.GlobalTranslation, dest, (List<Vector3> a) =>
                {
                    if (a.Count > 0)
                    {
                        GD.Print("move ok");
                        manager.EnableState("MovementState");
                        b.waypoints = a;
                    }
                }), b);

                targetCoord = Grid.ConvertToCoord(actor.GlobalTranslation);
                return;
            }
        }
        else
        {
            manager.DisableState("ChaseState");
            return;
        }
    }
}
