using Godot;
using System;

public class PreviewRect : TextureRect
{
    private Tween tween;
    [Export] private WindowDialog popup;
    [Export]public Color DefaultColor = new Color(0.5f, 0.5f, 0.5f, 1);
    [Export]public Color HoverColor = new Color(1, 1, 1, 1);
    [Export]public Color PressedColor = new Color(1, 1, 1, 1);
    

    public override void _Ready()
    {
        // Get the Tween node (Make sure it's a child of this node)
        tween = new Tween();
        popup = GetParent().GetNode<WindowDialog>("WindowDialog");
        AddChild(tween);

        // Connect hover signals
        Connect("mouse_entered", this, nameof(OnMouseEnter));
        Connect("mouse_exited", this, nameof(OnMouseExit));
        
        Modulate = DefaultColor;
    }

    private void OnMouseEnter()
    {
        // Lerp from Grey to White
        GD.Print("Mouse entered ");
        tween.InterpolateProperty(this, "modulate", DefaultColor, HoverColor, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
    }

    private void OnMouseExit()
    {
        // Lerp from White to Grey
        GD.Print("Mouse entered ");
        tween.InterpolateProperty(this, "modulate", HoverColor, DefaultColor, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == (int)ButtonList.Left)
        {
            tween.InterpolateProperty(this, "modulate", HoverColor, PressedColor, 0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
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


}
