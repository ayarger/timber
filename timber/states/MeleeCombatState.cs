using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class MeleeCombatState : CombatState
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";


	float time = 0.0f, rotateTime = 0.0f;

    public override string name
    {
        get { return "MeleeCombatState"; }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		base._Ready();
	}

	public override void Start()
	{
		GD.Print("MeleeCombatState started by " + actor.Name);
		inclusiveStates = new HashSet<string>();
		ArborCoroutine.StopCoroutinesOnNode(this);
		animation_offset = GD.Randf() * 100.0f;
		attackable = true;
		//if (actor.GetNode<HasTeam>("HasTeam").team == "player")//Hardcode different actor stats
		//{
		//    attackDamage = 40;
		//    attackCooldown = 0.75f;
		//}
	}

	public override void Update(float delta)
	{

		if (TargetActor != null && IsInstanceValid(TargetActor))//TODO check if actor is dead
		{
			Coord dest = Grid.ConvertToCoord(TargetActor.GlobalTranslation);
			ChaseState cs = null;
			if (manager.states.ContainsKey("ChaseState"))
			{
				cs = manager.states["ChaseState"] as ChaseState;
			}

			if (!TestMovement.WithinRange(dest, actor, attackRange))
			{
				GD.Print("Not in range");
				ArborCoroutine.StopCoroutinesOnNode(this);
				attacking = false;
				attackable = true;
				//check if there are closer target
				foreach (var actors in GetAttackableActorList())
				{

					var actorInRange = actors as Actor;
					if (actorInRange != null && actorInRange.GetNode<HasTeam>("HasTeam").team != actor.GetNode<HasTeam>("HasTeam").team)
					{
						Coord actorPos = Grid.ConvertToCoord(actorInRange.GlobalTranslation);
						Coord cur = Grid.ConvertToCoord(actor.GlobalTranslation);
						if ((cur - actorPos).Mag() < attackRange)
						{
							TargetActor = actorInRange;
							return;
						}
					}
				}

				if(cs == null)
				{
					manager.DisableState("CombatState");
				}

                cs.TargetActor = TargetActor;
                manager.EnableState("ChaseState");
                manager.DisableState("CombatState");
            }
            else if (attackable)
            {
                ArborCoroutine.StartCoroutine(attackRoutine(), this);
            }
        }
        else
        {
            manager.DisableState("CombatState");
            return;
        }
    }

	public override void Stop()
	{
		rotateTime = 0;
		ArborCoroutine.StopCoroutinesOnNode(this);
	}

    protected IEnumerator attackRoutine()
    {
        attacking = true;
        attackable = false;
        yield return ArborCoroutine.WaitForSeconds(attackWindup);

		attacking = false;
		if (GD.Randf() < criticalHitRate)
		{
			TargetActor.Hurt(attackDamage, true, actor);
		}
		else TargetActor.Hurt(attackDamage, false, actor);

		yield return ArborCoroutine.WaitForSeconds(attackRecovery);
		rotateTime = 0.0f;
		ArborCoroutine.StartCoroutine(CoolDown());

	}

	IEnumerator CoolDown()
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
        float desired_scale_x = current_scale_x;
        if (TargetActor != null && IsInstanceValid(TargetActor))
        {
            Vector3 position_delta = TargetActor.GlobalTranslation - actor.GlobalTranslation;
            if (position_delta.x > 0.01f)
                desired_scale_x = actor.initial_view_scale.x;
            if (position_delta.x < -0.01f)
                desired_scale_x = -actor.initial_view_scale.x;
        }
        current_scale_x += (desired_scale_x - current_scale_x) * 0.2f;
        actor.view.Scale = new Vector3(current_scale_x, actor.initial_view_scale.y * idle_scale_impact, actor.view.Scale.z);

		if (attacking)
		{
			rotateTime += delta;
			const float rot_amplitude = 5f;
			int direction = -1;
			if (TargetActor.GlobalTranslation.x < actor.GlobalTranslation.x) direction = 1;

			actor.view.Rotation = actor.initial_rotation + new Vector3(0, 0, direction * rot_amplitude * rotateTime * (rotateTime - attackWindup));
		}

	}

}
