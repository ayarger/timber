using Godot;
using System;
using System.Collections.Generic;

public abstract class ActorState : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    public abstract string name
    {
        get;
    }

    public virtual string stateType
    {
        get { return name; }
    }

    public Actor actor;
    public StateManager manager;

    //Empty for strictly exclusive, full for inclusive with all states
    public HashSet<string> inclusiveStates = new HashSet<string>();

    public override void _Ready()
    {
        actor = GetParent().GetParent<Actor>();
        manager = GetParent<StateManager>();
    }

    public virtual void Start()
    {

    }

    public virtual void Update(float delta)
    {

    }

    public virtual void Stop()
    {

    }

    public virtual void Animate(float delta)
    {

    }

    public virtual void Config(StateConfig stateConfig)
    {

    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }


}
