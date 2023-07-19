using Amazon.S3.Model;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

public class GameOver : Node
{
    [Export]
    Curve ease_out_curve;

    [Export]
    Curve ease_in_out;

    [Export]
    Curve ease_bounce;

    [Export]
    List<NodePath> continue_text_letters = new List<NodePath>();

    Button retry_button;
    Button give_up_button;
    TextureRect background_image;
    TextureRect game_over_image;

    Label countdown_number_text;

    Sprite character;
    Sprite character_view;
    Sprite spotlight;
    CPUParticles2D spotlight_particles;

    Camera2D camera;

    static int continues = 2;

    float camera_starting_height = -1000;
    float camera_final_height = 400;

    static GameOverRequest current_request = null;

    public static void PerformGameOver(GameOverRequest request)
    {
        if (request.fast_mode == false)
        {
            AudioStream lose_bgm = ArborResource.Get<AudioStream>("sounds/bgm_btd_defeat.ogg");
            ArborAudioManager.RequestBGM(lose_bgm);
        }

        

        current_request = request;
        UIManager.ClearAllMenus();
        TransitionSystem.RequestTransition(@"res://scenes/GameOver.tscn");
    }

    public override void _Ready()
    {
        retry_button = GetNode<Button>("ui_foreground/retry_button");
        countdown_number_text = GetNode<Label>("ui_foreground/countdown_number");
        background_image = GetNode<TextureRect>("ui_backround/game_over_background");
        game_over_image = GetNode<TextureRect>("ui_foreground/game_over_image");
        character = GetNode<Sprite>("scene2d/character");
        character_view = GetNode<Sprite>("scene2d/character/view");
        spotlight = GetNode<Sprite>("scene2d/spotlight");
        camera = GetNode<Camera2D>("scene2d/Camera2D");
        spotlight_particles = GetNode<CPUParticles2D>("scene2d/spotlight_particles");

        background_image.Modulate = new Color(1, 1, 1, 1);
        game_over_image.Modulate = new Color(1, 1, 1, 0);
        spotlight.Modulate = new Color(1, 1, 1, 1);
        character_view.Modulate = new Color(1, 1, 1, 1);
        camera.Position = new Vector2(1000, camera_starting_height);

        retry_button.Visible = false;
        ToggleContinueLetters(false);

        retry_button.GetNode<Label>("continue_count").Text = continues.ToString();

        ArborCoroutine.StartCoroutine(DoIntro(), this);
    }

    void ToggleContinueLetters(bool visible)
    {
        foreach (var labelPath in continue_text_letters)
        {
            Label label = GetNode<Label>(labelPath);
            label.Visible = visible;
        }
    }

    IEnumerator DoIntro()
    {
        countdown_number_text.Visible = false;
        ShaderMaterial mat = (ShaderMaterial)background_image.Material;
        mat.SetShaderParam("blur_amount", 0.1f);
        mat.SetShaderParam("texture_resolution", new Vector2(1920, 1080));

        if (current_request.fast_mode)
        {
            ArborCoroutine.StartCoroutine(ShowFinalImage(), this);
            yield break;
        }

        /* Load */
        TransitionSystem.RequestHold();
        ArborResource.Load<GameConfig>("game.config");
        ArborResource.Load<AudioStream>("sounds/bgm_continue.ogg");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_1.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_2.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_3.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_4.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_5.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_6.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_7.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_8.wav");
        ArborResource.Load<AudioStream>("sounds/mtd_vocal_9.wav");
        ArborResource.Load<Texture>("images/spot_continue_1.png");
        ArborResource.Load<Texture>("images/spot_continue_2.png");
        ArborResource.Load<Texture>("images/spot_continue_3.png");
        ArborResource.Load<Texture>("images/spot_victory.png");
        ArborResource.Load<Texture>("images/lose_bg.png");

        yield return ArborResource.WaitFor("sounds/bgm_continue.ogg");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_1.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_2.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_3.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_4.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_5.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_6.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_7.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_8.wav");
        yield return ArborResource.WaitFor("sounds/mtd_vocal_9.wav");

        yield return ArborResource.WaitFor("images/spot_continue_1.png");
        yield return ArborResource.WaitFor("images/spot_continue_2.png");
        yield return ArborResource.WaitFor("images/spot_continue_3.png");
        yield return ArborResource.WaitFor("images/spot_victory.png");

        yield return ArborResource.WaitFor("images/lose_bg.png");

        if(continues > 0)
            character_view.Texture = ArborResource.Get<Texture>("images/spot_continue_3.png");
        else
            character_view.Texture = ArborResource.Get<Texture>("images/spot_continue_1.png");
        background_image.Texture = ArborResource.Get<Texture>("images/lose_bg.png");

        TransitionSystem.RemoveHold();



        yield return ArborCoroutine.WaitForMouseClick();

        /* Fade out background */
        Color faded_color = new Color(1, 1, 1, 0.1f);
        Color full_color = new Color(1, 1, 1, 1);
        void FadeOutBackground(float progress)
        {
            mat.SetShaderParam("blur_amount", 0.1f + progress * 10.0f);
            background_image.Modulate = full_color.LinearInterpolate(faded_color, progress);
        }

        yield return ArborCoroutine.DoOverTime(FadeOutBackground, 1.0f);

        bool can_continue = continues > 0;

        Vector2 camera_starting_position = new Vector2(1500, camera_starting_height);
        Vector2 camera_final_position = new Vector2(1500, camera_final_height);

        if (!can_continue)
        {
            camera_starting_position = new Vector2(1000, camera_starting_height);
            camera_final_position = new Vector2(1000, camera_final_height);
        }


        /* Mode down to character. */
        spotlight_particles.Visible = true;
        yield return ArborCoroutine.MoveOverTime(camera, 1.0f, camera_starting_position, camera_final_position, ease_out_curve);

        if(can_continue)
            UIManager.RequestFlash(0.1f);

        character_view.Texture = ArborResource.Get<Texture>("images/spot_continue_1.png");

        if (!can_continue)
        {
            ArborCoroutine.StartCoroutine(DoOnGiveUpPressed(0.5f), this);
        }
        else
        {
            ArborCoroutine.StartCoroutine(DoContinueCountdown(), this);
        }
    }

    IEnumerator DoContinueCountdown()
    {
        yield return null;
        retry_button.Visible = true;
        countdown_number_text.Visible = true;
        ToggleContinueLetters(true);

        AudioStream continue_countdown_bgm = ArborResource.Get<AudioStream>("sounds/bgm_continue.ogg");
        ArborAudioManager.RequestBGM(continue_countdown_bgm);

        int countdown = 9;
        while(countdown > -1 && !retry_clicked)
        {  
            countdown_number_text.Text = countdown.ToString();
            ArborAudioManager.RequestSFX(ArborResource.Get<AudioStream>("sounds/mtd_vocal_" + countdown.ToString() + ".wav"));

            yield return ArborCoroutine.WaitForSecondsOrClick(1.0f);
            yield return null;
            yield return null;

            countdown--;
        }

        if (retry_clicked == false)
        {
            countdown_number_text.Visible = false;
            ToggleContinueLetters(false);
            retry_button.Visible = false;

            ArborCoroutine.StartCoroutine(DoOnGiveUpPressed(0.0f), this);
        }
        else
        {
            UIManager.RequestFlash(0.1f);

            countdown_number_text.Visible = false;
            ToggleContinueLetters(false);
            retry_button.Visible = false;
            continues--;

            character_view.Texture = ArborResource.Get<Texture>("images/spot_victory.png");
            ArborCoroutine.StartCoroutine(DoRetry(), this);
        }
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

    Vector2 countdown_number_initial_location = new Vector2(950, -500);
    Vector2 countdown_number_final_location = new Vector2(950, 0);

    IEnumerator DoOnGiveUpPressed(float initial_delay)
    {
        yield return ArborCoroutine.MoveOverTime(camera, 0.25f, camera.GlobalPosition, new Vector2(1000, camera_final_height));

        ShaderMaterial mat = (ShaderMaterial)background_image.Material;
        mat.SetShaderParam("blur_amount", 0.1f);
        background_image.Modulate = new Color(0, 0, 0, 1);
        //camera.GlobalPosition = new Vector2(1000, camera_final_height);

        yield return ArborCoroutine.WaitForSeconds(initial_delay);

        /* Character fall animation */
        character_view.Texture = ArborResource.Get<Texture>("images/spot_continue_2.png");
        yield return ArborCoroutine.WaitForSeconds(0.35f);

        character_view.Texture = ArborResource.Get<Texture>("images/spot_continue_3.png");

        yield return ArborCoroutine.WaitForSeconds(1.5f);

        /* Zoom out */
        void DoZoom(float progress)
        {
            Vector2 new_zoom = (Vector2.One * 1.5f).LinearInterpolate(Vector2.One * 5, progress);
            camera.Zoom = new_zoom;

            if(progress > 0.2f)
            {
                character_view.Modulate += (new Color(0, 0, 0, 1) - character_view.Modulate) * 0.1f;
                spotlight.Modulate += (new Color(0, 0, 0, 1) - spotlight.Modulate) * 0.1f;
            }
        }

        yield return ArborCoroutine.DoOverTime(DoZoom, 2, ease_in_out);
        spotlight_particles.Visible = false;
        character_view.Modulate = new Color(0, 0, 0, 0);
        spotlight.Modulate = new Color(0, 0, 0, 0);

        ArborCoroutine.StartCoroutine(ShowFinalImage(), this);
    }

    IEnumerator ShowFinalImage()
    {
        ShaderMaterial mat = (ShaderMaterial)background_image.Material;

        background_image.Modulate = new Color(0, 0, 0, 1);

        /* Show epilogue / game over background */
        ArborResource.Load<Texture>("images/gameover_bg.png");
        ArborResource.Load<Texture>("images/gameover.png");
        ArborResource.Load<AudioStream>("sounds/bgm_gameover_end.ogg");
        ArborResource.Load<AudioStream>("sounds/vocal_gameover.ogg");

        yield return ArborResource.WaitFor("images/gameover_bg.png");
        yield return ArborResource.WaitFor("images/gameover.png");
        yield return ArborResource.WaitFor("sounds/bgm_gameover_end.ogg");
        yield return ArborResource.WaitFor("sounds/vocal_gameover.ogg");

        background_image.Texture = ArborResource.Get<Texture>("images/gameover_bg.png");
        game_over_image.Texture = ArborResource.Get<Texture>("images/gameover.png");

        AudioStream gameover_bgm = ArborResource.Get<AudioStream>("sounds/bgm_gameover_end.ogg");
        
        //ArborAudioManager.RequestBGM(gameover_bgm);
        ArborAudioManager.RequestSFX(gameover_bgm);

        Color black_color = new Color(0, 0, 0, 1);
        Color full_color = new Color(1, 1, 1, 1);
        void FadeInBackground(float progress)
        {
            background_image.Modulate = black_color.LinearInterpolate(full_color, progress);
        }

        yield return ArborCoroutine.DoOverTime(FadeInBackground, 0.5f);

        yield return ArborCoroutine.WaitForSecondsOrClick(0.5f);

        mat.SetShaderParam("texture_resolution", new Vector2(1920, 1080));
        Color darkened_color = new Color(0.1f, 0.1f, 0.1f, 1);
        Color bright_color = new Color(1, 1, 1, 1);

        void DoChangeBlur(float progress)
        {
            mat.SetShaderParam("blur_amount", progress * 10 + 0.01f);
            background_image.Modulate = bright_color.LinearInterpolate(darkened_color, progress);
        }
        yield return ArborCoroutine.DoOverTime(DoChangeBlur, 0.5f);

        yield return ArborCoroutine.WaitForSecondsOrClick(0.5f);

        while (game_over_image.Modulate.a < 0.95f)
        {
            game_over_image.Modulate += (new Color(1, 1, 1, 1) - game_over_image.Modulate) * 0.2f;
            yield return null;
        }
        game_over_image.Modulate = new Color(1, 1, 1, 1);

        AudioStream stream = ArborResource.Get<AudioStream>("sounds/vocal_gameover.ogg");
        var audio_player = ArborAudioManager.RequestSFX(stream);

        float time_to_live = audio_player.Stream.GetLength() + 1.0f;

        yield return ArborCoroutine.WaitForSecondsOrClick(time_to_live + 0.25f);

        continues = 2;
        TransitionSystem.RequestTransition(@"res://scenes/MainMenu.tscn");
    }

    bool retry_clicked = false;
    public void OnRetryPressed()
    {
        retry_clicked = true;
    }

    IEnumerator DoRetry()
    {
        yield return ArborCoroutine.WaitForSecondsOrClick(1.0f);
        TransitionSystem.RequestTransition(@"res://Main.tscn");
    }
}

public class GameOverRequest
{
    public bool fast_mode = false;
}