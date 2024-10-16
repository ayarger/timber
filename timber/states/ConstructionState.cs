using System;
using System.Collections;
using System.Collections.Generic;
using Godot;


public class ConstructionState : CombatState
{
	float time = 0.0f, rotateTime = 0.0f;

	public override string name
	{
		get { return "ConstructionState"; }
	}

	public override string stateType
	{
		get { return "ConstructionState"; }
	}
	
	public override void Config(StateConfig stateConfig)
	{
		CombatConfig c = stateConfig as CombatConfig;
		attackRange = c.attackRange;
		attackDamage = c.attackDamage;
		criticalHitRate = 0;
		attackWindup = c.attackWindup;
		attackRecovery = c.attackRecovery;
		attackCooldown = c.attackCooldown;
	}

	public override void Update(float delta)
	{
		Tower _tower = TargetActor as Tower;
		if (TargetActor != null && IsInstanceValid(TargetActor))//TODO check if actor is dead
		{
			
			// If construction is completed, stop this state
			if (_tower._HasStats.GetStat("construction_progress").currVal >= _tower._HasStats.GetStat("construction_progress").maxVal)
			{
				attackable = true;
				ArborCoroutine.StopCoroutinesOnNode(this);
				manager.DisableState("ConstructionState");
				manager.EnableState("IdleState");
				return;
			}

			Coord dest = Grid.ConvertToCoord(TargetActor.GlobalTranslation);
			MovementState movementState = null;
			if (manager.states.ContainsKey("MovementState"))
			{
				movementState = manager.states["MovementState"] as MovementState;
			}

			Coord actorCoord = Grid.ConvertToCoord(actor.GlobalTranslation);

			float dist = Math.Abs(dest.x - actorCoord.x) + Math.Abs(dest.z - actorCoord.z);

			if (dist > attackRange)
			{
				ArborCoroutine.StopCoroutinesOnNode(this);
				attacking = false;
				attackable = true;
				//check if there are closer target
				foreach (var actors in GetAttackableActorList())
				{

					var actorInRange = actors as Actor;
					if (actorInRange != null && actorInRange.GetNode<HasTeam>("HasTeam").team == "construction")
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

				if(movementState == null)
				{
					manager.DisableState("ConstructionState");
				}

				if (movementState.waypoints.Count == 0)
				{
					ArborCoroutine.StopCoroutinesOnNode(movementState);
					Coord coordDest = TestMovement.FindClosestTileInRange(Grid.ConvertToCoord(TargetActor.GlobalTranslation), actor, attackRange);
					Vector3 vectorDest = new Vector3(coordDest.x * Grid.tileWidth, .1f, coordDest.z * Grid.tileWidth);
					ArborCoroutine.StartCoroutine(TestMovement.PathFindAsync(actor.GlobalTranslation, vectorDest, (List<Vector3> a) =>
					{
						if (a.Count > 0)
						{
							manager.EnableState("MovementState");
							movementState.waypoints = a;
						}
					}), movementState);
					manager.DisableState("ConstructionState");
					return;


				}
				//b.TargetActor = TargetActor;
				//manager.EnableState("ChaseState");
				//manager.DisableState("CombatState");
			}
			else if (attackable)
			{
				ArborCoroutine.StartCoroutine(attackAnimation(), this);
			}
		}
		else
		{
			manager.DisableState("ConstructionState");
			return;
		}
	}

	
	//************************************//
	//*** Copied from MeleeCombatState ***//
	//************************************//
	
	
	public override void Stop()
	{
		rotateTime = 0;
		ArborCoroutine.StopCoroutinesOnNode(this);
	}

	protected IEnumerator attackAnimation()
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
		Vector3 position_delta = TargetActor.GlobalTranslation - actor.GlobalTranslation;
		if (position_delta.x > 0.01f)
			desired_scale_x = actor.initial_view_scale.x;
		if (position_delta.x < -0.01f)
			desired_scale_x = -actor.initial_view_scale.x;

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
