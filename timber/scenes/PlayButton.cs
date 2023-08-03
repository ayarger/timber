using Godot;
using System;

public class PlayButton : Button
{
    public void OnPressed()
    {
        TransitionSystem.RequestTransition(@"res://Main.tscn");
    }
}
