using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;

public class MainMenu : Control
{
    TextureRect title_logo_node;
    TextureRect bg_node;

    public override void _Ready()
    {
        ArborCoroutine.StartCoroutine(DoIntro(), this);
    }

    IEnumerator DoIntro()
    {
        title_logo_node = GetNode<TextureRect>("title_logo");
        bg_node = GetNode<TextureRect>("bg");

        ArborResource.Load<GameConfig>("game.config");
        yield return ArborResource.WaitFor("game.config");
        GameConfig game_config = ArborResource.Get<GameConfig>("game.config");

        ArborResource.Load<AudioStream>("sounds/bgm_title.ogg");
        ArborResource.Load<Texture>("images/" + game_config.title_screen_logo_image);
        ArborResource.Load<Texture>("images/" + game_config.title_screen_background_image);

        yield return ArborResource.WaitFor("images/" + game_config.title_screen_logo_image);
        yield return ArborResource.WaitFor("images/" + game_config.title_screen_background_image);
        yield return ArborResource.WaitFor("sounds/bgm_title.ogg");

        Texture logo_tex = ArborResource.Get<Texture>("images/" + game_config.title_screen_logo_image);
        Texture bg_tex = ArborResource.Get<Texture>("images/" + game_config.title_screen_background_image);
        AudioStream title_bgm = ArborResource.Get<AudioStream>("sounds/bgm_title.ogg");
        ArborAudioManager.RequestBGM(title_bgm);

        title_logo_node.Texture = logo_tex;
        bg_node.Texture = bg_tex;
    }
}

