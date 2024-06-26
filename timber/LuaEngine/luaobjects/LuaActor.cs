using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using Amazon.S3.Model;
using static System.Net.Mime.MediaTypeNames;

public class LuaActor : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    ActorConfig config;

    public Spatial view { get; private set; } // Good for scaling operations.
    MeshInstance character_view;
    SpatialMaterial character_material;
    MeshInstance shadow_view;
    StateManager state_manager;

    IsSelectable selectable;

    Texture idle_sprite;
    //Initial animation variables
    public bool initial_load = false;
    public Vector3 initial_view_scale { get; private set; } = Vector3.One;
    public Vector3 initial_rotation { get; private set; } = Vector3.Zero;
    public TileData currentTile;

    float desired_scale_x = 1.0f;
    public float GetDesiredScaleX() { return desired_scale_x; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        previous_position = GlobalTranslation;

        view = (Spatial)GetNode("view");
        //state_manager = (StateManager)GetNode("StateManager");
        character_view = (MeshInstance)GetNode("view/mesh");
        //selectable = GetNode<IsSelectable>("IsSelectable");
        //time = GlobalTranslation.x + GlobalTranslation.z;
        //animation_offset = GD.Randf() * 100.0f;
        //EventBus.Subscribe<TileDataLoadedEvent>((TileDataLoadedEvent e) =>
        //{
        //    TileData td = Grid.Get(GlobalTranslation);
        //    currentTile = td;
        //    td.actor = this;
        //});
    }

    public void Configure(ActorConfig info)
    {
        config = info;
        //GetNode<HasTeam>("HasTeam").team = config.team;
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
                initial_load = true;
                initial_view_scale = view.Scale;
                desired_scale_x = initial_view_scale.x;
                initial_rotation = view.Rotation;

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
        ProcessColor();
        SetShaderParams();

        ComputeSharedAnimationData();
    }

    Vector3 previous_position = Vector3.Zero;
    void ComputeSharedAnimationData()
    {

        /* Determine which direction the actor would generally prefer to look in. */
        Vector3 position_delta = GlobalTranslation - previous_position;
        //GD.Print("COMPUTE ANIM!!! " + GlobalTranslation.x + " vs " + previous_position.x);

        if (position_delta.x > 0.01f)
            desired_scale_x = initial_view_scale.x;
        if (position_delta.x < -0.01f)
            desired_scale_x = -initial_view_scale.x;

        previous_position = GlobalTranslation;
    }

    float animation_offset = 0;

    float time = 0.0f;

    void ProcessColor()
    {
        Color desired_color = new Color(1, 1, 1, 1.0f);

        ///* Fade if cursor behind actor (helps player see behind tall actors) */
        //if (view.Scale.y >= 2.0f)
        //{
        //    float distance_to_cursor = GlobalTranslation.DistanceTo(SelectionSystem.GetTilePosition());
        //    float z_axis_difference = SelectionSystem.GetTilePosition().z - GlobalTranslation.z;

        //    if (distance_to_cursor <= 4.1f && z_axis_difference < -0.55f)
        //        desired_color.a = 0.35f;
        //    else
        //        desired_color.a = 1.0f;
        //}

        //character_material.AlbedoColor += (desired_color - character_material.AlbedoColor) * 0.1f;
    }

    void SetShaderParams()
    {
        ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0);

        //char_mat.SetShaderParam("fowTexture", FogOfWar.actorInstance.GetTexture());
        //char_mat.SetShaderParam("world_position", GlobalTranslation);
        //char_mat.SetShaderParam("screenWidth", FogOfWar.instance.screenWidth);
        //char_mat.SetShaderParam("screenHeight", FogOfWar.instance.screenHeight);
        //char_mat.SetShaderParam("screenPosX", FogOfWar.instance.screenPosX);
        //char_mat.SetShaderParam("screenPosZ", FogOfWar.instance.screenPosZ);
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
