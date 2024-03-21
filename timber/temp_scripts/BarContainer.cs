using Godot;
using System;

public class BarContainer : Control
{

    HasStats target_data;
    MeshInstance target_mesh;
    public MeshInstance target_shadow;
    IsSelectable target_selection;
    Spatial target_view;
    Actor target_actor;
    public Vector3 relativeToshadow;
    int count = 0;
    public float scaling_factor = 6f;
    bool displayOn;

    public override void _Ready()
    {
        // get target_mesh
        if (IsInstanceValid(target_data) && IsInstanceValid(target_data.GetParent()))
        {
            target_view = target_data.GetParent().GetNode<Spatial>("view");
            target_mesh = target_data.GetParent().GetNode<MeshInstance>("view/mesh");
            target_actor = (Actor)(target_data.GetParent());
            //config bar_container
            target_actor.bar_container = this;
            target_shadow = target_data.GetParent().GetNode<MeshInstance>("shadow");
            target_selection = target_data.GetParent().GetNode<IsSelectable>("IsSelectable");
            Vector3 shadow_position = target_shadow.GlobalTranslation;
            AABB aabb = target_mesh.GetAabb();
            float localHeight = aabb.Size.y;
            Vector3 globalScale = target_mesh.GlobalTransform.basis.Scale;
            float GlobalHeight = localHeight * globalScale.y;

            Vector3 mesh_position = target_mesh.GlobalTransform.Xform(new Vector3(0, GlobalHeight, 0)); ;
            relativeToshadow = mesh_position - shadow_position;
        }
    }

    public void Configure(HasStats _target) { 
       
        target_data = _target;
    }


    public override void _Process(float delta)
    {

        /*Vector3 shadow_position = target_shadow.GlobalTranslation;
        Vector3 mesh_position = target_mesh.GlobalTransform.Xform(new Vector3(0,target_mesh.Scale.y/1.05f, 0)); ;
        relativeToshadow = mesh_position - shadow_position;
        */

        displayOn = target_selection.am_i_selected || target_selection.AmIHovered();
        if (displayOn)
        {
            ToggleVisibilityOn();
        }

        else
        {
            ToggleVisibilityOff();
        }

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
        if (IsInstanceValid(target_shadow))
            desired_position = (target_shadow.GlobalTranslation + relativeToshadow);

        var screenPosition = cam.UnprojectPosition(desired_position);
        RectGlobalPosition = screenPosition;
        RectScale = Vector2.One * 0.15f;
        //scaling
        float distance = target_mesh.GlobalTranslation.DistanceTo(cam.GlobalTranslation);
        float characterSize = target_mesh.Scale.y;
        float newScale = (characterSize / distance) * scaling_factor;
        RectScale = new Vector2(newScale, newScale);
    }

    void ToggleVisibilityOn()
    {
        // TODO:FadeIn
        Bar child, child1, child2;
        VBoxContainer container1 = GetNode<VBoxContainer>("container1");
        child = (Bar)container1.GetChild(0);
        child.FadeIn(0.2f);

        /*VBoxContainer container2 = GetNode<VBoxContainer>("container1/container2");
        if((Bar)container2.GetChild(0)!= null)
        {
            child1 = (Bar)container2.GetChild(0);
            child1.FadeIn(0.2f);
        }

        if((Bar)container2.GetChild(0) != null)
        {
            child2 = (Bar)container2.GetChild(1);
            child2.FadeIn(0.2f);
        }*/
    }

    void ToggleVisibilityOff()
    {
        Bar child, child1, child2;
        VBoxContainer container1 = GetNode<VBoxContainer>("container1");
        child = (Bar)container1.GetChild(0);
        child.FadeOut(0.2f);

        /*VBoxContainer container2 = GetNode<VBoxContainer>("container1/container2");
        if ((Bar)container2.GetChild(0) != null)
        {
            child1 = (Bar)container2.GetChild(0);
            child1.FadeOut(0.2f);
        }
        if ((Bar)container2.GetChild(0) != null)
        {
            child2 = (Bar)container2.GetChild(1);
            child2.FadeOut(0.2f);
        }*/
    }

    // Editing Mode Functionality
    // Create & Remove Bars

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
        new_bar.OnCreate();
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
        new_bar.OnCreate();

        return new_bar;
    }

    public void RemovePrimary()
    {
        VBoxContainer container1 = GetNode<VBoxContainer>("container1");
        Bar child = (Bar)container1.GetChild(0);
        child.FadeOut(0.5f);
        if (child.Modulate.a == 0)
        {
            RemoveChild(child);
            child.QueueFree();
        }

    }

    public void RemoveSecondary(int index)
    {
        VBoxContainer container2 = GetNode<VBoxContainer>("container1/container2");
        if (index < (container2.GetChildCount()))
        {
            Bar child = (Bar)container2.GetChild(index);
            child.FadeOut(0.3f);
            if (child.Modulate.a == 0)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }

    }
}