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
			TransitionSystem.RequestTransition(@"res://Main.tscn");
		}
		else
		{
			GD.Print("No internet connection detected.");
		}
	}
}
