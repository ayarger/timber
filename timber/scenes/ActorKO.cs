using Amazon.Runtime;
using Godot;
using System;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

public class ActorKO : Spatial
{
    MeshInstance character_view;

    Texture pre_ko_texture;
    Texture ko_texture;
    Actor killedBy;


    bool endGame, defaultAnimation = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        character_view = (MeshInstance)GetNode("rotationPoint/view/mesh");
        GD.Print("starting soon");
        ArborCoroutine.StartCoroutine(DoAnimation());
    }

    public void Configure(Texture pre_ko_tex, Texture ko_tex, bool gameOver = false, Actor source = null)
    {
        Name = "actor_ko";
        pre_ko_texture = pre_ko_tex;
        ko_texture = ko_tex;
        endGame = gameOver;
        killedBy = source;
    }

    IEnumerator DoAnimation()
    {
        
        ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
        char_mat.SetShaderParam("fade_amount ", 0.8f);
        char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);
        char_mat.SetShaderParam("texture_albedo", pre_ko_texture);
        character_view.SetSurfaceMaterial(0, char_mat);
        character_view.Translation = new Vector3(0, character_view.Scale.y/2, 0);

        if (endGame)
        {
            yield return ArborCoroutine.WaitForSeconds(2.0f);
            velocity = 0.2f;
            gravity = true;
            char_mat.SetShaderParam("texture_albedo", ko_texture);
            character_view.SetSurfaceMaterial(0, char_mat);
        }
        else
        {
            if(killedBy!=null)
                blastDirection = (GlobalTranslation - killedBy.GlobalTranslation).Normalized();
            blastSpeed = 6;
            blastRotationSpeed = 15;
            velocity = 0.2f;
            gravity = true;
        }

        void DoFade(float progress)
        {

        }

        yield return ArborCoroutine.DoOverTime(DoFade, 10.0f);

        if (endGame)
        {
            GameOver.PerformGameOver(new GameOverRequest());
        }
        else
        {
            QueueFree();
        }
        
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
            velocity -= 0.005f;
            GlobalTranslation += Vector3.Up * velocity;
        }

        GlobalTranslation += blastDirection * blastSpeed * delta;
        character_view.Rotation += Vector3.Back * blastRotationSpeed * delta;//BUG rotation point squish sprite
    }
}
