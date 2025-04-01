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
    
    private Timer hoverDelayTimer;
    private bool isHovered = false;

  
    

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

        // Connect hover signals
        Connect("mouse_entered", this, nameof(OnMouseEnter));
        Connect("mouse_exited", this, nameof(OnMouseExit));
        
        // Set hover delay timer
        hoverDelayTimer = new Timer();
        hoverDelayTimer.WaitTime = 0.05f;
        hoverDelayTimer.OneShot = true;
        hoverDelayTimer.Connect("timeout", this, nameof(EvaluateHoverEnd));
        AddChild(hoverDelayTimer);

        
        Modulate = DefaultColor;
        deleteButton.Visible = false;
    }

    private void OnMouseEnter()
    {
        isHovered = true;
        ApplyHover(true);
        hoverDelayTimer.Stop();
    }

    private void OnMouseExit()
    {
        isHovered = false;
        hoverDelayTimer.Start(); // delay to avoid premature unhover
    }
    
    private void EvaluateHoverEnd()
    {
        if (!isHovered && !deleteButton.IsHovered())
        {
            ApplyHover(false);
        }
    }
    
    private void ApplyHover(bool hover)
    {
        Color from = hover ? DefaultColor : HoverColor;
        Color to = hover ? HoverColor : DefaultColor;
        tween.InterpolateProperty(this, "self_modulate", from, to, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();

        deleteButton.Visible = hover;
    }
    
    public override void _GuiInput(InputEvent @event)
    {
       /* if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == (int)ButtonList.Left)
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
        }*/
    }
    
    //drag and drop
    public override bool CanDropData(Vector2 position, object data)
    {
        return data is SlidePreview;
    }

    public override object GetDragData(Vector2 position)
    {
        var dragPreview = currSlide.Duplicate() as Control;
        SetDragPreview(dragPreview);
        return currSlide;
    }

    public override void DropData(Vector2 position, object data)
    {
        if (data is SlidePreview draggedSlide && draggedSlide != currSlide)
        {
            CutsceneEditor.Instance.OnSlideDropped(draggedSlide, currSlide);
        }
    }
}
