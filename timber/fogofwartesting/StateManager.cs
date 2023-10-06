using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateManager : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    public Dictionary<string,ActorState> states;
    HashSet<ActorState> activeStates;

    public override void _Ready()
    {
        states = new Dictionary<string,ActorState>();
        foreach(var state in GetChildren())
        {
            var actorState = state as ActorState;
            if (actorState != null) states[actorState.name] = actorState;
        }
        activeStates = new HashSet<ActorState>();
    }

    public override void _Process(float delta)
    {
        foreach( var state in activeStates)
        {
            state.Update(delta);
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


}
