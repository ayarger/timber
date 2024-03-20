using Godot;
using System;

public class NameTag : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    Subscription<EditModeEvent> editModeEvent;
    MeshInstance target_mesh;
    Label name_label;
    [Export]
    string actor_name;
    [Export]
    public float scaling_factor = 6f;

    public override void _Ready()
    {
        editModeEvent = EventBus.Subscribe<EditModeEvent>(HideAndShow);
        name_label = GetNode<Label>("NameLabel");
        name_label.Text = actor_name;
        //this.Visible = false;
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
        //this.Visible = editModeEvent.activated;
    }

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

    private void PursueTarget()
    {
        Camera cam = (Camera)GameplayCamera.GetGameplayCamera();
        Vector3 desired_position = target_mesh.GetParent<Spatial>().GlobalTranslation;
        var screenPosition = cam.UnprojectPosition(desired_position);
        RectGlobalPosition = screenPosition;
        //UI elements Sclaing
        float distance = target_mesh.GlobalTransform.origin.DistanceTo(cam.GlobalTransform.origin);
        float characterSize = target_mesh.Scale.y;
        float newScale = (characterSize / distance) * scaling_factor;
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
        GD.Print($"{new_tag.Name} {new_tag} created");
        GD.Print($"tag is under: {new_tag.GetParent().Name}");
        return new_tag;
    }
}
