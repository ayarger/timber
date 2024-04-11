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
    RigidBody collision;
    public override void Start()
    {
        collision = GetNode<RigidBody>("view/RigidBody");

        collision.Connect("body_entered", this, "on_body_entered");
    }

    float rotationSpeed = 5, speed = 7f;
    Vector3 direction = Vector3.Zero;
    public override void Animate(float delta)
    {
        actor.view.Rotation += Vector3.Back * rotationSpeed * delta;
    }


    public override void Update(float delta)
    {
        actor.GlobalTranslation += direction * speed * delta;
        
    }

    public void setDirection(Vector3 dir)
    {
        direction = dir;
    }

    private void on_body_entered(Node body)
    {
        GD.Print("hello");
    }
}