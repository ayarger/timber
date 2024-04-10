using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProjectileState : ActorState
{
    public override string name
    {
        get
        {
            return "ProjectileState";
        }
    }

    //Inclusive States should always be empty for idle state.

    public override void Start()
    {
    }

    float rotationSpeed = 5, speed = 5f;
    Vector3 direction = Vector3.Zero;
    public override void Animate(float delta)
    {
        actor.view.Rotation += Vector3.Back * rotationSpeed * delta;
    }


    public override void Update(float delta)
    {
        actor.GlobalTranslation += direction * speed * delta;
        TileData cur = Grid.Get(actor.GlobalTranslation);
        if(cur.actor != null && cur.actor != actor)
        {
            GD.Print("found!");
        }
    }

    public void setDirection(Vector3 dir)
    {
        direction = dir;
    }

    
}