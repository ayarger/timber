using Godot;
using MoonSharp.Interpreter;
using System;

public class ProjectsPopup : PopupDialog
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    VBoxContainer _vboxContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _vboxContainer = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
    public void Open(ProjectMetadata[] projects)
    {
        PopupCentered();
        foreach (Node n in _vboxContainer.GetChildren())
        {
            _vboxContainer.RemoveChild(n);
            n.QueueFree();
        }
        foreach (var project in projects)
        {
            var newButton = ResourceLoader.Load<PackedScene>("res://scenes/ProjectButton.tscn").Instance<ProjectButton>();
            newButton.Name = project.name;
            newButton.GetNode<Button>("Button").Text = project.name;
            newButton.uuid = project.uuid;
            _vboxContainer.AddChild(newButton);
        }

    }

}
