using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class TestAttack : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

	float attackCooldown = 1;
	bool attackable = true;
	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (Input.IsActionPressed("space") && attackable)
		{
			//From SelectionSystem.cs & testmovement
			var from = GameplayCamera.GetGameplayCamera().ProjectRayOrigin(SelectionSystem.GetCursorWindowPosition());
			var dir = GameplayCamera.GetGameplayCamera().ProjectRayNormal(SelectionSystem.GetCursorWindowPosition());

			Vector3 intersection_point = GetRayPlaneIntersection(new Ray(from, dir), Vector3.Up);
			//DebugSphere.VisualizePoint(intersection_point);

			/* Round */
			Vector3 rounded_point = Grid.LockToGrid(intersection_point);
			Coord cur = Grid.ConvertToCoord(rounded_point);

			foreach (var entity in SelectionSystem.GetCurrentActiveSelectables())
			{
				//Maybe better way to do this w/o reflection?
				if (typeof(Actor).IsAssignableFrom(entity.GetParent().GetType()))
				{
					var actor = (Actor)entity.GetParent();
					StateManager sm = actor.FindNode("StateManager") as StateManager;
					string team = actor.GetNode<HasTeam>("HasTeam").team;
					if (!sm.states.ContainsKey("CombatState")) continue;
					if (team != "player") continue;

					if (Grid.Get(cur).actor != null && Grid.Get(cur).actor.GetNode<HasTeam>("HasTeam").team != team)
                    {
						//Grid.Get(cur).actor.Hurt();
						//ArborCoroutine.StartCoroutine(refreshCooldown(), this);
						CombatState cs = sm.states["CombatState"] as CombatState;
						cs.TargetActor = Grid.Get(cur).actor;
						sm.EnableState("CombatState");
					}
				}
			}

			
		}
	}

	Vector3 GetRayPlaneIntersection(Ray ray, Vector3 plane_normal)
	{
		float t = -(plane_normal.Dot(ray.start)) / (plane_normal.Dot(ray.direction));
		return ray.start + t * ray.direction;
	}
}
