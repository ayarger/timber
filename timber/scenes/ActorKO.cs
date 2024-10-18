using Amazon.Runtime;
using Godot;
using System;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

public class ActorKO : Spatial//TODO check if dead or not: bug - spawn multiple KO when killed at the same time
{
    MeshInstance character_view;

    Texture pre_ko_texture;
    Texture ko_texture;
    Actor killedBy;


    bool endGame, defaultAnimation = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        character_view = (MeshInstance)GetNode("view/mesh");
        ArborCoroutine.StartCoroutine(DoAnimation());
    }

    ShaderMaterial char_mat;
    Spatial view;

    public void Configure(Texture pre_ko_tex, Texture ko_tex, bool gameOver = false, Actor source = null)
    {
        Name = "actor_ko";
        pre_ko_texture = pre_ko_tex;
        ko_texture = ko_tex;
        endGame = gameOver;
        killedBy = source;

        char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
        view = (Spatial)GetNode("view");
    }

    IEnumerator DoAnimation()
    {
        char_mat.SetShaderParam("fade_amount ", 0.8f);
        char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);
        char_mat.SetShaderParam("texture_albedo", pre_ko_texture);
        character_view.SetSurfaceMaterial(0, char_mat);
        character_view.Translation = new Vector3(0, character_view.Scale.y/2, 0);

        view.Scale = new Vector3(pre_ko_texture.GetWidth(), pre_ko_texture.GetHeight(), 1.0f) * 0.01f;

        if (endGame)
        {
            EventBus.Publish(new EventPlayerDefeated(this));

            velocity = 0.075f;
            gravity = true;
        }
        else
        {
            if(killedBy!=null)
                blastDirection = (GlobalTranslation - killedBy.GlobalTranslation).Normalized();
            blastSpeed = 8;
            blastRotationSpeed = 15;
            velocity = 0.25f;
            gravity = true;
        }

        yield return ArborCoroutine.WaitForSeconds(1.0f);

        void DoFade(float progress)
        {

        }

        yield return ArborCoroutine.DoOverTime(DoFade, 1.0f);

        if (endGame)
        {
            GameOver.PerformGameOver(new GameOverRequest());
        }
        else
        {
            QueueFree();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    float velocity = 0.0f;

    Vector3 blastDirection = Vector3.Zero;
    float blastSpeed = 0;
    float blastRotationSpeed = 0;

    bool gravity = false;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (gravity)
        {
            velocity -= 0.0025f;

            if (!endGame)
                GlobalTranslation += Vector3.Up * velocity;
            else
                GlobalTranslation += GameplayCamera.GetGameplayCamera().GlobalTransform.basis.y * velocity;
        }

        if (ko_texture != null && velocity < -0.01f)
        {
            char_mat.SetShaderParam("texture_albedo", ko_texture);
            character_view.SetSurfaceMaterial(0, char_mat);
            view.Scale = new Vector3(ko_texture.GetWidth(), ko_texture.GetHeight(), 1.0f) * 0.01f;
        }

        GlobalTranslation += blastDirection * blastSpeed * delta;
        character_view.Rotation += Vector3.Back * blastRotationSpeed * delta;//BUG rotation point squish sprite
    }
}

public class EventPlayerDefeated
{
    public Spatial actor_ko;

    public EventPlayerDefeated(Spatial _actor_ko)
    {
        actor_ko = _actor_ko;
    }
}