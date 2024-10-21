using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProjectileState : ActorState//TODO collision body to chekc for collision
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
        Node rb = GetParent().GetParent().FindNode("RigidBody");
        rb.Connect("body_entered", this, "onBodyEntered");
    }

    float rotationSpeed = 5, horizontalSpeed = 10f, verticalSpeed = 1f, lifeTime = 5f, timer = 0, bounceDecay = 0.8f;
    float gravity = 60f;
    Vector3 direction = Vector3.Zero;
    public override void Animate(float delta)
    {
        actor.view.Rotation += Vector3.Back * rotationSpeed * delta;
        timer += delta;
        if(timer > lifeTime)
        {
            actor.Kill();
        }
    }


    public override void Update(float delta)
    {
        actor.GlobalTranslation += direction * horizontalSpeed * delta;
        actor.GlobalTranslation += Vector3.Up * verticalSpeed * delta;
        verticalSpeed = verticalSpeed - gravity * delta;
        if(actor.GlobalTranslation.y < 0)
        {
            verticalSpeed = -verticalSpeed * bounceDecay;
        }
    }

    public void setTarget(Vector3 targetPosition)
    {
        //projectile motion
        float dx =Mathf.Sqrt(Mathf.Pow(targetPosition.x - actor.GlobalTranslation.x, 2) + Mathf.Pow(targetPosition.z - actor.GlobalTranslation.z, 2));
        direction = (targetPosition - actor.GlobalTranslation).Normalized();
        float dt = dx / horizontalSpeed;
        verticalSpeed = (0.5f * gravity * dt * dt) / dt;
    }

    public void onBodyEntered(Node body)
    {
        Actor TargetActor = body.GetNode("..") as Actor;
        if (TargetActor != null && !(TargetActor is Projectile))
        {
            
            if(TargetActor.GetNode<HasTeam>("HasTeam").team != actor.GetNode<HasTeam>("HasTeam").team )
            {
                Projectile projectile = actor as Projectile;
                TargetActor.Hurt(projectile.damage, false, projectile.owner);
                projectile.Kill();
            }
            
        }
    }


}