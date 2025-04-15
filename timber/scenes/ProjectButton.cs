using Godot;
using System;

public class ProjectButton : MarginContainer
{
    public string uuid = null;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnPressed()
    {
        TempSecrets.MOD_UUID = uuid;
        //TransitionSystem.RequestTransition(@"res://Main.tscn");
        string scene_path = "res://scenes/CutsceneManager.tscn";
        PackedScene new_scene = ResourceLoader.Load<PackedScene>(scene_path);
        if (new_scene == null)
        {
            GD.Print("Failed to load scene: " + scene_path);
        }
        else
        {
            GetTree().ChangeSceneTo(new_scene);
            GD.Print("Scene Loaded: " + scene_path);
        }
    }
}
