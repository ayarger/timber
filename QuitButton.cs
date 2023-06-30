using Godot;
using System;
using System.Collections;

public class QuitButton : Button
{
    public void OnPressed()
    {
        TransitionSystem.RequestTransition("quit");
    }
}
