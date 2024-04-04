using Godot;
using System;
using System.Collections.Generic;

public class EditUI : Control
{
    HasStats target_data;
    public MeshInstance target_mesh;
    public Vector3 relativeToShadow;
    Subscription<EditModeEvent> editModeEvent;
    Tween ui_tween;
    Dictionary<string, Stat> statsDict = new Dictionary<string, Stat>();
    OptionButton optionButton;

    public override void _Ready()
    {
        editModeEvent = EventBus.Subscribe<EditModeEvent>(HideAndShow);
        ui_tween = GetNode<Tween>("Tween");
        optionButton = GetNode<OptionButton>("OptionButton");
    }

    public void Configure(MeshInstance _target_mesh, HasStats _target_data)
    {
        target_mesh = _target_mesh;
        target_data = _target_data;
    }

    public static EditUI Create( MeshInstance _mesh, HasStats _stats)
    {
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://UI/EditUI/EditUI.tscn");
        EditUI new_edit_ui = (EditUI)scene.Instance();
        new_edit_ui.Configure(_mesh, _stats);
        PrimaryCanvas.AddChildNode(new_edit_ui);
        new_edit_ui.GetEditOptions();
        return new_edit_ui;
    }

    public void GetEditOptions()
    {
        statsDict = target_data.Stats;
        foreach (string statName in statsDict.Keys)
        {
            optionButton.AddItem(statName);
            GD.Print("Option " + statName + "added\n");
        }
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
        float distance = target_mesh.GlobalTransform.origin.DistanceTo(cam.GlobalTransform.origin);
    }

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
