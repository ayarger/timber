using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public class StateManager : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.

	public Dictionary<string,ActorState> states;
	Actor actor;
	HashSet<ActorState> activeStates;
	public string defaultState = "IdleState";
	bool enabled = true;

	public override void _Ready()
	{
		actor = GetParent<Actor>();
		states = new Dictionary<string,ActorState>();
		// foreach(var state in GetChildren())
		// {
		//     var actorState = state as ActorState;
		//     if (actorState != null)
		//     {
		//         if (actor.GetNode<HasTeam>("HasTeam") != null 
		//             && actor.GetNode<HasTeam>("HasTeam").team == "enemy"
		//             && actorState.name == "construction")
		//         {
		//             continue;
		//         }
		//         states[actorState.name] = actorState;
		//     }
		// }
		// if (!states.ContainsKey("Idle"))
		// {
		//     states.Add("Idle", new IdleState());
		//     states["Idle"].actor = GetParent<Actor>();
		//     states["Idle"].manager = this;
		//     AddChild(states["Idle"]);
		// }
		activeStates = new HashSet<ActorState>();
	}

	public void Configure(List<StateConfig> stateConfigs)
	{
		foreach (var config in stateConfigs)
		{
			var type = StateProcessor.GetState(config.name);
			ActorState actorState = Activator.CreateInstance(type) as ActorState;
			
			actorState.Config(config);
			string stateType = actorState.stateType;
			//GD.Print(stateType);
			states[stateType] = actorState;
			actorState.actor = GetParent<Actor>();
			actorState.manager = this;
			AddChild(actorState);
		}
	}

	public override void _Process(float delta)
	{
		if (enabled)
		{
			if (activeStates.Count == 0)
			{
				EnableState(defaultState);
			}
			ResetAnimation();
			foreach (var state in new HashSet<ActorState>(activeStates))
			{
				state.Update(delta);
				state.Animate(delta);
			}
		}

	}

	public void EnableState(string state)
	{
		if (!states.ContainsKey(state))
		{
			GD.PrintErr("Attempted to enable non-existent state [" + state + "]");
			return;
		}

		ActorState desired_state = states[state];

		if (!activeStates.Contains(desired_state)){
			bool clearStates = false;
			foreach(var aState in activeStates)
			{
				if (!states[state].inclusiveStates.Contains(aState.name))
				{
					foreach (var s in activeStates) s.Stop();
					clearStates = true;
					break;
				}
			}
			if (clearStates)
			{
				activeStates.Clear();
			}
			activeStates.Add(states[state]);
			states[state].Start();
		}
		
	}

	public void DisableState(string state)
	{
		if (states.ContainsKey(state) && activeStates.Contains(states[state]))
		{
			activeStates.Remove(states[state]);
			states[state].Stop();
		}

	}

	//If no animations are using these values, reset them. (Subject to change)
	Vector3 oldRotation = Vector3.Zero;
	Vector3 oldScale = Vector3.One;
	void ResetAnimation()
	{
		if (!actor.initial_load) return;
		//May be a better way to do this. Ref variables don't work.
		if (oldRotation == actor.view.Rotation)
		{
			actor.view.Rotation = .8f * actor.view.Rotation + .2f * actor.initial_rotation;
		}
		if (oldScale.Abs() == actor.view.Scale.Abs())
		{
			//actor.view.Scale = .8f * actor.view.Scale + .2f * actor.initial_view_scale;
			Vector3 newScale;
			newScale.x = .8f * actor.view.Scale.x + (.2f * actor.initial_view_scale.Abs().x) * Mathf.Sign(actor.view.Scale.x);
			newScale.y = actor.view.Scale.y;
			newScale.z = .8f * actor.view.Scale.z + (.2f * actor.initial_view_scale.Abs().z) * Mathf.Sign(actor.view.Scale.z);
			actor.view.Scale = newScale;
		}

		oldRotation = actor.view.Rotation;
		oldScale = actor.view.Scale;
	}

	public bool IsStateActive(string state)
	{
		return activeStates.Contains(states[state]);
	}

	public void DisableAllState()
	{
		enabled = false;
		foreach (ActorState state in activeStates)
		{
			DisableState(state.name);
		}
	   
		
	}
}
