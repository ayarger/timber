using Godot;
using System;

public class UIOrb : Control
{
    HasStats target_data;

    TextureProgress tex_progress;

    MeshInstance target_mesh;

    Tween ui_tween;

    public override void _Ready()
    {
        // get tex_progress
        tex_progress = GetNode<TextureProgress>("FrontProgress");
        // get target_mesh
        if(IsInstanceValid(target_data) && IsInstanceValid(target_data.GetParent()))
            target_mesh = target_data.GetParent().GetNode<MeshInstance>("view/mesh");
        // get target_tween
        ui_tween = GetNode<Tween>("UITween");

        // connect stats change event here
        //target_data.Connect("health_change", this, nameof(OnHealthChange));
    }

    public void Configure(HasStats _target)
    {
        target_data = _target;
    }

    public override void _Process(float delta)
    {
        if (IsInstanceValid(target_data))
            PursueTarget();
        else
            QueueFree();
    }

    /// <summary>
    /// Move to UIOrb to the target
    /// </summary>
    void PursueTarget()
    {
        Camera cam = (Camera)GameplayCamera.GetGameplayCamera();
        Vector3 desired_position = target_data.GetParent<Spatial>().GlobalTranslation;

        if (IsInstanceValid(target_mesh))
            desired_position = target_mesh.GlobalTransform.Xform(new Vector3(0, 1, 0));

        var screenPosition = cam.UnprojectPosition(desired_position);
        RectGlobalPosition = screenPosition;
        RectScale = Vector2.One * 0.15f;

        tex_progress.Value = target_data.health_ratio * 100;
    }

    /// <summary>
    /// Create UIorb
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
    public static UIOrb Create(HasStats stats)
    {
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://scenes/UIOrb.tscn");
        UIOrb new_orb = (UIOrb) scene.Instance();
        new_orb.Configure(stats);
        PrimaryCanvas.AddChildNode(new_orb);
        return new_orb;
    }

    /// <summary>
    /// Update UI on health change
    /// </summary>
    public void OnHealthChange()
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
