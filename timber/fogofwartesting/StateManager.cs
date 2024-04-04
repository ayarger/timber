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
    string defaultState = "Idle";
    bool enabled = true;

    public override void _Ready()
    {
        states = new Dictionary<string,ActorState>();
        foreach(var state in GetChildren())
        {
            var actorState = state as ActorState;
            if (actorState != null) states[actorState.name] = actorState;

        }
        if (!states.ContainsKey("Idle"))
        {
            states.Add("Idle", new IdleState());
            states["Idle"].actor = GetParent<Actor>();
            states["Idle"].manager = this;
            AddChild(states["Idle"]);
        }
        activeStates = new HashSet<ActorState>();
        actor = GetParent<Actor>();
    }

    public void Configure(List<StateConfig> stateConfigs)
    {

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
        if (!activeStates.Contains(states[state])){
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
        if (activeStates.Contains(states[state]))
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
        if (oldScale == actor.view.Scale)
        {
            actor.view.Scale = .8f * actor.view.Scale + .2f * actor.initial_view_scale;
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
