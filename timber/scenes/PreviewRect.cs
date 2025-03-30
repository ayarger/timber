using Godot;
using System;
using System.Diagnostics;
using Yarn.Compiler;

public class PreviewRect : TextureRect
{
    private Tween tween;
    [Export] private SlidePreview currSlide;
    [Export] private WindowDialog popup;
    [Export] public TextureButton deleteButton;
    [Export]public Color DefaultColor = new Color(0.5f, 0.5f, 0.5f, 1);
    [Export]public Color HoverColor = new Color(1, 1, 1, 1);
    [Export]public Color PressedColor = new Color(1, 1, 1, 1);
  
    

    public override void _Ready()
    {
        // Get the Tween node (Make sure it's a child of this node)
        tween = new Tween();
        currSlide = (SlidePreview)GetParent();
        GD.Print("currSlide: " + currSlide);
        deleteButton = GetNode<TextureButton>("DeleteButton");
        GD.Print("deleteButton: " + deleteButton);
        popup = GetParent().GetNode<WindowDialog>("WindowDialog");
        AddChild(tween);
        
        // Delete button signals
        deleteButton.Connect("pressed", this, nameof(OnDeletePressed));

        // Connect hover signals
        Connect("mouse_entered", this, nameof(OnMouseEnter));
        Connect("mouse_exited", this, nameof(OnMouseExit));
        
        Modulate = DefaultColor;
        deleteButton.Visible = false;
    }

    private void OnMouseEnter()
    {
        tween.InterpolateProperty(this, "self_modulate", DefaultColor, HoverColor, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        deleteButton.Visible = true;
    }

    private void OnMouseExit()
    {
        tween.InterpolateProperty(this, "self_modulate", HoverColor, DefaultColor, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        deleteButton.Visible = false;
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == (int)ButtonList.Left)
        {
            tween.InterpolateProperty(this, "self_modulate", HoverColor, PressedColor, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            if (popup != null)
            {
                popup.PopupCentered(); 
            }
            else
            {
                GD.PrintErr("PopupEditor not found! Make sure it's added as a child.");
            }
        }
    }
    
    private void OnDeletePressed()
    {
        if (currSlide.originalSceneData == null)
        {
            GD.PrintErr("Cannot delete: originalSceneData is null.");
            return;
        }

        var cutsceneList = CutsceneManager.Instance.cutsceneImages;

        // Remove from the list
        cutsceneList.Remove(currSlide.tempSceneData);

        // Optionally, reindex the remaining slides
        for (int i = 0; i < cutsceneList.Count; i++)
        {
            cutsceneList[i].Index = i;
        }
        // Remove from UI
        QueueFree();

        GD.Print("Slide removed and JSON updated.");
    }


}
