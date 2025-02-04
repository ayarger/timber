using Godot;
using System;
using System.Threading.Tasks;

public class PlayButton : Button
{
	public async void OnPressed()
	{
		if (await Utilities.IsConnectedToInternet(this))
		{
			GD.Print("Internet connection detected. Launching game...");
			EventBus.Publish(new CutsceneStartEvent());
			// TODO: transitionsystem debug
			//TransitionSystem.RequestTransition(@"res://scenes/CutsceneManager.tscn");
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
		else
		{
			GD.Print("No internet connection detected.");
		}
		
	}
}
