using Godot;
using System;

public class BarContainer : Control
{

    HasStats target_data;
    MeshInstance target_mesh;
    IsSelectable target_selection;
    public float scaling_factor = 6f;
    bool displayOn;

    public override void _Ready()
    {
        // get target_mesh
        if (IsInstanceValid(target_data) && IsInstanceValid(target_data.GetParent()))
        {
            target_mesh = target_data.GetParent().GetNode<MeshInstance>("view/mesh");
            target_selection = target_data.GetParent().GetNode<IsSelectable>("IsSelectable");
            GD.Print("container created " + "fecthing selection " + target_selection);
        }
    }

    public void Configure(HasStats _target)
    {
        target_data = _target;
    }

    public override void _Process(float delta)
    {

        displayOn = target_selection.am_i_selected || target_selection.AmIHovered();
        Visible = displayOn;
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
    }

    /// <summary>
    /// Create UIBar
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
    public static BarContainer Create(HasStats stats)
    {
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://temp_scenes/BarContainer.tscn");
        BarContainer new_containter = (BarContainer)scene.Instance();
        new_containter.Configure(stats);
        PrimaryCanvas.AddChildNode(new_containter);
        return new_containter;
    }

    public Bar CreatePrimary(string data_name)
    {
        PackedScene bar = (PackedScene)ResourceLoader.Load("res://temp_scenes/bar.tscn");
        Bar new_bar = (Bar)bar.Instance();
        new_bar.Configure(target_data, data_name);
        VBoxContainer container1 = GetNode<VBoxContainer>("container1");
        container1.AddChild(new_bar);
        // move the primary bar to top
        new_bar.GetParent().MoveChild(new_bar, 0);
        return new_bar;
    }

    public Bar CreateSecondary(string data_name)
    {
        PackedScene bar2 = (PackedScene)ResourceLoader.Load("res://temp_scenes/bar2.tscn");
        Bar new_bar = (Bar)bar2.Instance();
        new_bar.Configure(target_data, data_name);
        VBoxContainer container2 = GetNode<VBoxContainer>("container1/container2");
        container2.Visible = true;
        container2.AddChild(new_bar);
        return new_bar;
    }
}