using Godot;
using System;

public class NameTag : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    Subscription<EditModeEvent> editModeEvent;
    MeshInstance target_mesh;
    Tween ui_tween;
    Label name_label;
    [Export]
    string actor_name;
    [Export]
    public float scaling_factor = 80f;

    public override void _Ready()
    {
        editModeEvent = EventBus.Subscribe<EditModeEvent>(HideAndShow);
        name_label = GetNode<Label>("NameLabel");
        name_label.Text = actor_name;
        ui_tween = GetNode<Tween>("Tween");
        this.Visible = false;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
/// <summary>
/// Hide and show nameTags based on editMode status
/// </summary>
/// <param name="editModeEvent"></param>
    public void HideAndShow(EditModeEvent editModeEvent)
    {
        if (editModeEvent.activated)
        {
            this.Visible = true;
            FadeIn(0.4f);
        }

        else
        {
            FadeOut(0.4f);
        }
        //this.Visible = editModeEvent.activated;
    }
    /// <summary>
    /// Configure name tag info 
    /// </summary>
    /// <param name="_mesh"></param>
    /// <param name="_actor_name"></param>

    public void Configure(MeshInstance _mesh, string _actor_name)
    {
        target_mesh = _mesh;
        actor_name = _actor_name;
    }

    public override void _Process(float delta)
    {
        if (IsInstanceValid(target_mesh))
            PursueTarget();
        else
            QueueFree();
    }

    /// <summary>
    /// Overhead UI element following & scaling behavior
    /// </summary>
    private void PursueTarget()
    {
        Camera cam = (Camera)GameplayCamera.GetGameplayCamera();
        Vector3 desired_position = target_mesh.GetParent<Spatial>().GlobalTranslation;
        var screenPosition = cam.UnprojectPosition(desired_position);
        RectGlobalPosition = screenPosition;
        float distance = target_mesh.GlobalTransform.origin.DistanceTo(cam.GlobalTransform.origin);
        float characterSize = target_mesh.Scale.y;
        float newScale = (characterSize / distance)*scaling_factor;
        RectScale = new Vector2(newScale, newScale);
    }

    /// <summary>
    /// Create nameTag UI when actor is loaded into the scene
    /// </summary>
    public static NameTag Create (MeshInstance _mesh, string _actor_name)
    {
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://temp_scenes/NameTag.tscn");
        NameTag new_tag = (NameTag)scene.Instance();
        new_tag.Configure(_mesh, _actor_name);
        new_tag.Name = _actor_name + "nameLabel";
        PrimaryCanvas.AddChildNode(new_tag);
        return new_tag;
    }

    public void OnDisplay()
    {
        RectScale = new Vector2(0, 0);

        // Setup the tween to scale the child from 0 to 1 (original size).
        ui_tween.InterpolateProperty(this, "rect_scale", RectScale, new Vector2(1, 1), 0.3f, Tween.TransitionType.Back, Tween.EaseType.Out);
        ui_tween.Start(); // Start the tween.
    }

    public void FadeIn(float duration)
    {
        Color currentColor = Modulate;
        ui_tween.InterpolateProperty(this, "modulate:a", currentColor.a, 1, duration, Tween.TransitionType.Linear, Tween.EaseType.Out);
        ui_tween.Start();
    }

    public void FadeOut(float duration)
    {
        Color currentColor = Modulate;
        ui_tween.InterpolateProperty(this, "modulate:a", currentColor.a, 0, duration, Tween.TransitionType.Linear, Tween.EaseType.In);
        ui_tween.Start();
    }
}
