using Godot;
using System;

public class hasShadowUI : Control
{
    [Export] private Vector2 shadowOffset = new Vector2(5, 5);
    [Export] private Color shadowColor = new Color(0, 0, 0, 0f);

    public override void _Ready()
    {
        // Duplicate the TextureRect
        TextureRect shadow = Duplicate() as TextureRect;

        // Configure the duplicate as a shadow
        if (shadow != null)
        {
            shadow.Modulate = shadowColor;  
            shadow.RectPosition += shadowOffset;  // Offset the shadow
            GetParent().AddChild(shadow);
            shadow.GetParent().MoveChild(shadow, 0); // move shadow beneath everything else
        }
    }
}
