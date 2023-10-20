using Godot;
using System;

public class UIBar : Control
{

    HasStats target_data;

    String data_name;

    int index;

    TextureProgress tex_progress;

    MeshInstance target_mesh;

    Tween ui_tween;

    public float scaling_factor = 6f;

    public override void _Ready()
    {
        // get tex_progress
        tex_progress = GetNode<TextureProgress>("FrontProgress");
        // get target_mesh
        if (IsInstanceValid(target_data) && IsInstanceValid(target_data.GetParent()))
            target_mesh = target_data.GetParent().GetNode<MeshInstance>("view/mesh");
        // get target_tween
        ui_tween = GetNode<Tween>("UITween");
        // connect stats change event here
        target_data.Connect("stat_change", this, nameof(OnUIStatChange));
    }

    public void Configure(HasStats _target, string _data_name, int _index)
    {
        target_data = _target;
        data_name = _data_name;
        index = _index;
    }

    public override void _Process(float delta)
    {
        if (IsInstanceValid(target_data))
            PursueTarget();
        else
            QueueFree();
    }

    /// <summary>
    /// Move UI to the target
    /// </summary>
    void PursueTarget()
    {
        Camera cam = (Camera)GameplayCamera.GetGameplayCamera();
        Vector3 desired_position = target_data.GetParent<Spatial>().GlobalTranslation;

        // TODO: UI position should be based on the top of the character
        if (IsInstanceValid(target_mesh))
            desired_position = target_mesh.GlobalTransform.Xform(new Vector3(0, 1, 0));

        var screenPosition = cam.UnprojectPosition(desired_position);
        RectGlobalPosition = screenPosition;
        RectScale = Vector2.One * 0.15f;
        //scaling
        float distance = target_mesh.GlobalTransform.origin.DistanceTo(cam.GlobalTransform.origin);
        float characterSize = target_mesh.Scale.y;
        float newScale = (characterSize / distance) * scaling_factor;
        RectScale = new Vector2(newScale, newScale);
        if (index > 0)
        {
            RectScale = new Vector2(newScale * 0.8f, newScale * 0.8f);

            if (index == 1)
            {
                RectGlobalPosition = new Vector2(RectGlobalPosition.x - 7.5f, RectGlobalPosition.y + 10);
            }

            else
            {
                RectGlobalPosition = new Vector2(RectGlobalPosition.x - 7.5f, RectGlobalPosition.y + 10 + 6);
            
            }
        }
        tex_progress.Value = target_data.health_ratio * 100;
    }

    /// <summary>
    /// Create UIBar
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
    public static UIBar Create(HasStats stats, string stat_name, int index)
    {
        // load scene
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://scenes/UIBar.tscn");
        UIBar new_bar = (UIBar)scene.Instance();
        new_bar.Configure(stats, stat_name, index);
        new_bar.Name = stat_name + " bar";
        PrimaryCanvas.AddChildNode(new_bar);

        // TODO: create container upon adding the first UI bar
        if (index == 0)
        {
            Node newContainer = new Container();
        }
        // resize UIbar& bar pos based on bar index
        if (index > 0)
        {
            Vector2 init_pos = new_bar.RectPosition;
            new_bar.RectPosition = new Vector2(init_pos.x, init_pos.y - 40);
            if(index == 1)
            {
                new_bar.tex_progress.SelfModulate = new Color(0, 0.3f, 0.6f, 1);
            }
            else
            {
                new_bar.tex_progress.SelfModulate = new Color(0.3f, 0.5f, 0, 1);
            }
        }
        return new_bar;
    }

    public void ChangeColor(Color _color)
    {
        tex_progress.SelfModulate = _color;
    }

    /// <summary>
    /// Update UI on health change
    /// </summary>
    public void OnUIStatChange(string stat_name)
    {
        GD.Print("curr_stat_name: " + data_name, " incoming singal name: " + stat_name);
        if (stat_name == data_name)
        {
            float target_value = target_data.health_ratio * 100;
            // Stop any ongoing tween operation.
            ui_tween.StopAll();
            //GD.Print(tex_progress.Value);
            ui_tween.InterpolateProperty(
                tex_progress,
                "value",
                tex_progress.Value,
                target_value,
                2f,
                Tween.TransitionType.Linear,
                Tween.EaseType.InOut
            );
            // start tween 
            ui_tween.Start();
        }
    }
}