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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        character_view = (MeshInstance)GetNode("mesh");

        ArborCoroutine.StartCoroutine(DoAnimation());
    }

    public void Configure(Texture pre_ko_tex, Texture ko_tex)
    {
        Name = "actor_ko";
        pre_ko_texture = pre_ko_tex;
        ko_texture = ko_tex;
    }

    IEnumerator DoAnimation()
    {
        ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
        char_mat.SetShaderParam("fade_amount ", 0.8f);
        char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);
        char_mat.SetShaderParam("texture_albedo", pre_ko_texture);
        character_view.SetSurfaceMaterial(0, char_mat);

        GD.Print("actor ko");
        yield return ArborCoroutine.WaitForSeconds(2.0f);

        velocity = 0.2f;
        gravity = true;
        char_mat.SetShaderParam("texture_albedo", ko_texture);
        character_view.SetSurfaceMaterial(0, char_mat);

        void DoFade(float progress)
        {

        }

        yield return ArborCoroutine.DoOverTime(DoFade, 2.0f);
        GameOver.PerformGameOver(new GameOverRequest());
    }
    float velocity = 0.0f;

    bool gravity = false;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (gravity)
        {
            velocity -= 0.005f;
            GlobalTranslation += Vector3.Up * velocity;
        }
    }
}
