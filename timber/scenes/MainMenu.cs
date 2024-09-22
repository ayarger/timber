using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;

public class MainMenu : Control
{
	public override async void _Ready()
	{
		
		if (await Utilities.IsConnectedToInternet(this))
		{
			GD.Print("Internet connection detected. Launching game...");
		}
		else
		{
			GD.Print("No internet connection detected.");
			// TODO: show a message to warn the player about missing internet connection
			return;
		}
		ArborResource.UseResource(
			"sounds/bgm_title.ogg", 
			(AudioStream audio) => {
				GD.Print("playing title music.");
				ArborAudioManager.RequestBGM(audio, true);
			},
			this
		);
		ArborResource.UseResource(
			"images/diamond-g7915c1180_1280.png",
			(Texture texture) => {
				TextureRect title_logo_node = GetNode<TextureRect>("title_logo");
				title_logo_node.Texture = texture;
			},
			this
		);

		ArborResource.UseResource(
			"images/title_screen_background.png",
			(Texture texture) => {
				TextureRect bg_node = GetNode<TextureRect>("bg");
				bg_node.Texture = texture;
			},
			this
		);
	}
}

