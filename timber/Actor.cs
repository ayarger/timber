using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using Amazon.S3.Model;
using static System.Net.Mime.MediaTypeNames;

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

    public TileData currentTile;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        view = (Spatial)GetNode("view");
        character_view = (MeshInstance)GetNode("view/mesh");
        selectable = GetNode<IsSelectable>("IsSelectable");
        time = GlobalTranslation.x + GlobalTranslation.z;
        animation_offset = GD.Randf() * 100.0f;
        EventBus.Subscribe<TileDataLoadedEvent>((TileDataLoadedEvent e) =>
        {
            TileData td = Grid.Get(new Coord(Mathf.RoundToInt(GlobalTranslation.x / 2f), Mathf.RoundToInt(GlobalTranslation.z / 2f)));
            currentTile = td;
            td.actor = this;
        });

    }

    public void Configure(ActorConfig info)
    {
        config = info;
        GetNode<HasTeam>("HasTeam").team = config.team;
        if (config.team == "player")
        {
            EventBus.Publish<SpawnLightSourceEvent>(new SpawnLightSourceEvent(this));
        }

        if (config.pre_ko_sprite_filename != null && config.pre_ko_sprite_filename != "")
            ArborResource.Load<Texture>("images/" + config.pre_ko_sprite_filename);
        if (config.ko_sprite_filename != null && config.ko_sprite_filename != "")
            ArborResource.Load<Texture>("images/" + config.ko_sprite_filename);

        ArborResource.UseResource<Texture>(
            "images/" + config.idle_sprite_filename,
            (Texture tex) =>
            {
                ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
                shadow_view = (MeshInstance)GetNode("shadow");

                /* Scale */
                view.Scale = new Vector3(tex.GetWidth(), tex.GetHeight(), 1.0f) * 0.01f;
                view.Scale = view.Scale * config.aesthetic_scale_factor;
                initial_scale = view.Scale;

                shadow_view.Scale = new Vector3(Mathf.Min(2.0f, view.Scale.x), shadow_view.Scale.y, shadow_view.Scale.z);

                char_mat.SetShaderParam("texture_albedo", tex);
                char_mat.SetShaderParam("fade_amount ", 0.8f);
                char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);

                //character_material.AlbedoTexture = idle_sprite;
                character_view.SetSurfaceMaterial(0, char_mat);
            },
            this
        );
    }



//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        time += delta;
        ProcessAnimation();
        ProcessColor();
        SetShaderParams();
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

    void SetShaderParams()
    {
        ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0);

        char_mat.SetShaderParam("fowTexture",FogOfWar.instance.GetTexture());
        char_mat.SetShaderParam("world_position", GlobalTranslation);
        char_mat.SetShaderParam("screenWidth", FogOfWar.instance.screenWidth);
        char_mat.SetShaderParam("screenHeight", FogOfWar.instance.screenHeight);
        char_mat.SetShaderParam("screenPosX", FogOfWar.instance.screenPosX);
        char_mat.SetShaderParam("screenPosZ", FogOfWar.instance.screenPosZ);
    }

    public void Kill()
    {
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://scenes/ActorKO.tscn");
        ActorKO new_ko = (ActorKO)scene.Instance();
        GetParent().AddChild(new_ko);
        new_ko.GlobalTranslation = GlobalTranslation;
        new_ko.GlobalRotation = GlobalRotation;
        new_ko.Scale = Scale;
        Visible = false;
        new_ko.Configure(ArborResource.Get<Texture>("images/" + config.pre_ko_sprite_filename), ArborResource.Get<Texture>("images/" + config.ko_sprite_filename));
    }
}
