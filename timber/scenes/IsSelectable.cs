using Godot;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;

public class IsSelectable : Node
{
    static HashSet<IsSelectable> selectables = new HashSet<IsSelectable>();
    public static HashSet<IsSelectable> GetSelectables() { return selectables; }
    private bool isRemovingParent = false;
    public bool first_time_placement = false;

    public static HashSet<IsSelectable> GetSelectablesWithinRegion(SelectionRegion region) 
    {
        HashSet<IsSelectable> result = new HashSet<IsSelectable>();
        foreach(IsSelectable selectable in selectables)
        {
            bool within_region = region.IsPointWithinRegion(selectable.GetParent<Spatial>().GlobalTranslation);
            if (within_region)
                result.Add(selectable);
        }

        return result;
    }

    MeshInstance shadow_view;
    ShaderMaterial shadow_material;
    Spatial parent;

    Subscription<EventSelectionBegun> sub_EventSelectionBegun;
    Subscription<EventSelectionFinished> sub_EventSelectionFinished;


    public override void _Ready()
    {
        selectables.Add(this);
        parent = GetParent<Spatial>();
        shadow_view = GetNode<MeshInstance>("../shadow");

        sub_EventSelectionBegun = EventBus.Subscribe<EventSelectionBegun>(OnEventSelectionBegun);
        sub_EventSelectionFinished = EventBus.Subscribe<EventSelectionFinished>(OnEventSelectionFinished);

        InitShadow();
    }

    void InitShadow()
    {
        shadow_material = (ShaderMaterial)shadow_view.GetSurfaceMaterial(0).Duplicate();
        //shadow_material.ParamsAlphaScissorThreshold = 0.5f;
        //shadow_material.FlagsTransparent = true;

        shadow_view.SetSurfaceMaterial(0, shadow_material);
    }

    public override void _ExitTree()
    {
        if (sub_EventSelectionBegun != null)
            EventBus.Unsubscribe(sub_EventSelectionBegun);
        if (sub_EventSelectionFinished != null)
            EventBus.Unsubscribe(sub_EventSelectionFinished);

        selectables.Remove(this);

        base._ExitTree();
    }

    public override void _Process(float delta)
    {
        ProcessShadow();
    }

    void ProcessShadow()
    {
        if (am_i_selected)
        {
            shadow_material.SetShaderParam("color", new Vector3(1.0f, 1.0f, 1.0f));
            shadow_material.SetShaderParam("alpha", 1.0f);
        }
        else if (AmIHovered())
        {
            shadow_material.SetShaderParam("color", new Vector3(1.0f, 1.0f, 1.0f));
            shadow_material.SetShaderParam("alpha", 0.4f);
        }
        else
        {
            shadow_material.SetShaderParam("color", new Vector3(0.0f, 0.0f, 0.0f));
            shadow_material.SetShaderParam("alpha", 0.4f);
        }

        shadow_view.MaterialOverride = shadow_material;
    }

   public bool AmIHovered()
    {
        SelectionRegion selection_region = SelectionSystem.GetCurrentSelectionRegion();
        return !isRemovingParent && selection_region.IsPointWithinRegion(parent.GlobalTranslation);
    }

    public bool am_i_selected = false;
    void OnEventSelectionFinished(EventSelectionFinished e)
    {
        if (e.selection_region.IsPointWithinRegion(parent.GlobalTranslation))
        {
            am_i_selected = true;
        }
        else
            am_i_selected = false;
    }

    void OnEventSelectionBegun(EventSelectionBegun e)
    {
        am_i_selected = false;
    }

    public void OnRemovingParent()
    {
        isRemovingParent = true;
    }
}
