using Godot;
using System;
using Yarn;
using YarnSpinnerGodot;

public class StartDialogueButton : Button
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    [Export] public NodePath dialogueRunner;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    private void _on_Button_pressed()
    {
        GetNode<DialogueRunner>(dialogueRunner).StartDialogue(GetNode<DialogueRunner>(dialogueRunner).startNode);
        this.Hide();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
