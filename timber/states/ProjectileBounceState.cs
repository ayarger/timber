using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProjectileBounceState : ActorState//TODO collision body to chekc for collision
{
    public override string name
    {
        get
        {
            return "ProjectileBounceState";
        }
    }

    //Inclusive States should always be empty for idle state.
    RigidBody collision;
    public override void Start()
    {
    }

    float rotationSpeed = 5, horizontalSpeed = 10f, verticalSpeed = 1f, lifeTime = 2f, timer = 0, bounceDecay = 0.8f, horizontalAcceleration = -3f;
    float gravity = 60f;
    Vector3 direction = Vector3.Zero;
    bool isActive = true, bounceOn = true;

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
        if(bounceOn)
            verticalSpeed = verticalSpeed - gravity * delta;

        if(actor.GlobalTranslation.y < 0)
        {
            
            verticalSpeed = -verticalSpeed * bounceDecay;
            if(verticalSpeed < 0.2f)
            {
                verticalSpeed = 0;
                bounceOn = false;
            }
        }
        if(horizontalSpeed > 0)
            horizontalSpeed = horizontalSpeed + horizontalAcceleration * delta;

    }

    public void setDirection(Vector3 d, float directionalSpeed, float Vi)
    {
        //projectile motion
        direction = new Vector3(-d.x, 0, d.z).Normalized();
        horizontalSpeed = 0.6f*directionalSpeed;
        verticalSpeed = 1.5f * Math.Abs(Vi);
    }



}