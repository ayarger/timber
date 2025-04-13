using Amazon.S3.Model;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

public class VictoryScene : Node
{
    [Export]
    Curve ease_out_curve;

    [Export]
    List<NodePath> text_letters = new List<NodePath>();

    TextureRect background_image;

    Sprite character;
    Sprite character_view;
    Sprite spotlight;
    CPUParticles2D spotlight_particles;

    Camera2D camera;

    float camera_starting_height = -1000;
    float camera_final_height = 400;

    public static void PerformVictory()
    {
        UIManager.ClearAllMenus();
        TransitionSystem.RequestTransition(@"res://scenes/Victory.tscn");
    }

    public override void _Ready()
    {
        background_image = GetNode<TextureRect>("ui_backround/bg");
        character = GetNode<Sprite>("scene2d/character");
        character_view = GetNode<Sprite>("scene2d/character/view");
        spotlight = GetNode<Sprite>("scene2d/spotlight");
        camera = GetNode<Camera2D>("scene2d/Camera2D");
        spotlight_particles = GetNode<CPUParticles2D>("scene2d/spotlight_particles");

        background_image.Modulate = new Color(1, 1, 1, 1);
        spotlight.Modulate = new Color(1, 1, 1, 1);
        character_view.Modulate = new Color(1, 1, 1, 1);
        camera.Position = new Vector2(1000, camera_starting_height);

        ToggleLetters(false);

        ArborCoroutine.StartCoroutine(DoIntro(), this);
    }

    void ToggleLetters(bool visible)
    {
        foreach (var labelPath in text_letters)
        {
            Label label = GetNode<Label>(labelPath);
            label.Visible = visible;
        }
    }

    IEnumerator DoIntro()
    {
        TransitionSystem.RequestHold();

        /* Load */
        ArborResource.Load<AudioStream>("public/sounds/bgm_victory.ogg");
        ArborResource.Load<Texture>("public/images/spot_victory.png");
        ArborResource.Load<Texture>("public/images/victory_bg.png");

        GD.Print("waiting...");
        yield return ArborResource.WaitFor("public/sounds/bgm_victory.ogg");
        yield return ArborResource.WaitFor("public/images/spot_victory.png");
        yield return ArborResource.WaitFor("public/images/victory_bg.png");

        GD.Print("done waiting...");


        ShaderMaterial mat = (ShaderMaterial)background_image.Material;
        mat.SetShaderParam("blur_amount", 0.1f);
        mat.SetShaderParam("texture_resolution", new Vector2(1920, 1080));

        character_view.Texture = ArborResource.Get<Texture>("public/images/spot_victory.png");
        background_image.Texture = ArborResource.Get<Texture>("public/images/victory_bg.png");
        ArborAudioManager.RequestBGM(ArborResource.Get<AudioStream>("public/sounds/bgm_victory.ogg"));

        TransitionSystem.RemoveHold();

        yield return ArborCoroutine.WaitForSeconds(0.5f);
        yield return ArborCoroutine.WaitForMouseClick();

        /* Fade out background */
        Color faded_color = new Color(1, 1, 1, 0.1f);
        Color full_color = new Color(1, 1, 1, 1);
        void FadeOutBackground(float progress)
        {
            mat.SetShaderParam("blur_amount", 0.1f + progress * 10.0f);
            background_image.Modulate = full_color.LinearInterpolate(faded_color, progress);
        }

        yield return ArborCoroutine.DoOverTime(FadeOutBackground, 0.5f);

        Vector2 camera_starting_position = new Vector2(1500, camera_starting_height);
        Vector2 camera_final_position = new Vector2(1500, camera_final_height);

        /* Mode down to character. */
        spotlight_particles.Visible = true;
        yield return ArborCoroutine.MoveOverTime(camera, 1.0f, camera_starting_position, camera_final_position, ease_out_curve);

        UIManager.RequestFlash();
        ToggleLetters(true);

        yield return ArborCoroutine.WaitForMouseClick();

        TransitionSystem.RequestTransition(@"res://Main.tscn");
    }

    public override void _Process(float delta)
    {
        CharacterAnimation();
    }

    void CharacterAnimation()
    {
        float y_scale = 1.0f + Mathf.Abs(Mathf.Sin(3.0f * OS.GetTicksMsec() / 1000.0f)) * 0.025f;
        character.GlobalScale = new Vector2(character.GlobalScale.x, y_scale);
    }

    public override void _Input(InputEvent @event)
    {
        // Check if the event is an InputEventKey
        if (@event is InputEventKey eventKey)
        {
            // Check if the escape key was just pressed
            if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
            {
                //ArborCoroutine.StartCoroutine(DoCountdownTextFall(countdown), this);
            }
        }
    }
}
