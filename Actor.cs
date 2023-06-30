using Godot;
using System;
using System.Collections.Generic;
using System.Collections;

public class Actor : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    ActorConfig config;

    Spatial view; // Good for scaling operations.
    MeshInstance character_view;
    SpatialMaterial character_material;
    MeshInstance shadow_view;

    IsSelectable selectable;

    Texture idle_sprite;
    Vector3 initial_scale = Vector3.One;

    void ChangeSprite ()
    {

    }

    public void Configure(ActorConfig info)
    {
        ArborCoroutine.StartCoroutine(DoConfigure(info), this);
    }

    IEnumerator DoConfigure(ActorConfig info)
    {
        config = info;

        view = (Spatial)GetNode("view");
        character_view = (MeshInstance)GetNode("view/mesh");
        
        ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();

        shadow_view = (MeshInstance)GetNode("shadow");

        ArborResource.Load<Texture>("images/" + config.idle_sprite_filename);
        yield return ArborResource.WaitFor("images/" + config.idle_sprite_filename);
        Texture idle_sprite = ArborResource.Get<Texture>("images/" + config.idle_sprite_filename);

        /* Scale */
        view.Scale = new Vector3(idle_sprite.GetWidth(), idle_sprite.GetHeight(), 1.0f) * 0.01f;
        view.Scale = view.Scale * config.aesthetic_scale_factor;
        initial_scale = view.Scale;

        shadow_view.Scale = new Vector3(Mathf.Min(2.0f, view.Scale.x), shadow_view.Scale.y, shadow_view.Scale.z);

        char_mat.SetShaderParam("texture_albedo", idle_sprite);
        char_mat.SetShaderParam("fade_amount ", 0.8f);
        char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);

        //character_material.AlbedoTexture = idle_sprite;
        character_view.SetSurfaceMaterial(0, char_mat);
    }

    IEnumerator DoConfigure_backup(ActorConfig info)
    {
        config = info;

        view = (Spatial)GetNode("view");
        character_view = (MeshInstance)GetNode("view/mesh");
        character_material = (SpatialMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
        character_material.ParamsUseAlphaScissor = true;
        character_material.ParamsAlphaScissorThreshold = 0.5f;
        //character_material.FlagsTransparent = true;


        shadow_view = (MeshInstance)GetNode("shadow");
        GD.Print("SHADOW [" + shadow_view + "]");

        ArborResource.Load<Texture>("images/" + config.idle_sprite_filename);
        yield return ArborResource.WaitFor("images/" + config.idle_sprite_filename);
        Texture idle_sprite = ArborResource.Get<Texture>("images/" + config.idle_sprite_filename);

        /* Scale */
        view.Scale = new Vector3(idle_sprite.GetWidth(), idle_sprite.GetHeight(), 1.0f) * 0.01f;
        view.Scale = view.Scale * config.aesthetic_scale_factor;
        initial_scale = view.Scale;

        shadow_view.Scale = new Vector3(Mathf.Min(2.0f, view.Scale.x), shadow_view.Scale.y, shadow_view.Scale.z);

        character_material.AlbedoTexture = idle_sprite;
        character_view.SetSurfaceMaterial(0, character_material);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        selectable = GetNode<IsSelectable>("IsSelectable");
        time = GlobalTranslation.x + GlobalTranslation.z;
        animation_offset = GD.Randf() * 100.0f;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        time += delta;
        ProcessAnimation();
        ProcessColor();
    }

    float animation_offset = 0;

    float time = 0.0f;
    void ProcessAnimation()
    {
        /* idle animation */
        float idle_scale_impact = (1.0f + Mathf.Sin(time * 4 + animation_offset) * 0.025f);

        /* apply */
        view.Scale = new Vector3(initial_scale.x, initial_scale.y * idle_scale_impact, initial_scale.z);
    }

    void ProcessColor()
    {
        Color desired_color = new Color(1, 1, 1, 1.0f);

        /* Fade if cursor behind actor (helps player see behind tall actors) */
        if (view.Scale.y >= 2.0f)
        {
            float distance_to_cursor = GlobalTranslation.DistanceTo(SelectionSystem.GetTilePosition());
            float z_axis_difference = SelectionSystem.GetTilePosition().z - GlobalTranslation.z;

            if (distance_to_cursor <= 4.1f && z_axis_difference < -0.55f)
                desired_color.a = 0.35f;
            else
                desired_color.a = 1.0f;
        }

        //character_material.AlbedoColor += (desired_color - character_material.AlbedoColor) * 0.1f;
    }
}
