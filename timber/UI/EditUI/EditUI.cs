using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public class StatSelectionEvent
{
    public string stat_name;
    public string char_name;
    public StatSelectionEvent (string _stat_name,string _char_name)
    {
        stat_name = _stat_name;
        char_name = _char_name;
    }
}

public class EditUI : Control
{
    HasStats target_data;
    string target_name;
    public MeshInstance target_mesh;
    public Vector3 relativeToShadow;
    Subscription<EditModeEvent> editModeEvent;
    Subscription<StatSelectionEvent> statSelectionEvent;
    Tween ui_tween;
    Dictionary<string, Stat> statsDict = new Dictionary<string, Stat>();
    OptionButton optionButton;
    VBoxContainer vbox;

    public override void _Ready()
    {
        editModeEvent = EventBus.Subscribe<EditModeEvent>(HideAndShow);
        statSelectionEvent = EventBus.Subscribe<StatSelectionEvent>(OnStatSelected);
        ui_tween = GetNode<Tween>("Tween");
        optionButton = GetNode<OptionButton>("OptionButton");
        vbox = GetNode<VBoxContainer>("VBoxContainer");
        //default signal when selecting an optoin
        optionButton.Connect("item_selected", this, nameof(OnOptionSelected));
    }

    private void OnOptionSelected(int index)
    {
        EventBus.Publish<StatSelectionEvent>(new StatSelectionEvent(optionButton.GetItemText(index),target_name));
    }

    private void OnStatSelected(StatSelectionEvent e) {
        //check if the event is relevant to this character
        if (e.char_name == target_name)
        {
            Stat curr_stat = target_data.Stats[e.stat_name];
            Type typeToInspect = curr_stat.GetType();
            if (typeToInspect != null)
            {
                //spawn in options if no methodbutton is present
                MethodInfo[] methods = typeToInspect.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (MethodInfo method in methods)
                {
                    PackedScene scene = (PackedScene)ResourceLoader.Load("res://UI/EditUI/methodButton.tscn");
                    Button methodButton = (Button)scene.Instance();
                    GD.Print($"{methodButton.Text}\n");
                    methodButton.Text = $"{method.Name} ({method.ReturnType.Name})\n";
                    vbox.AddChild(methodButton);
                }
            }
        }

    }

    //TODO OnMethod Selection

    public void Configure(MeshInstance _target_mesh, HasStats _target_data)
    {
        target_mesh = _target_mesh;
        target_data = _target_data;
        target_name = target_data.GetParent().Name;
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
