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
			//TransitionSystem.RequestTransition(@"res://scenes/CutsceneManager.tscn");
			EventBus.Publish(new CutsceneStartEvent());
		}
		else
		{
			GD.Print("No internet connection detected.");
		}
	}
}
