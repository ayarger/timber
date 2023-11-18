using Godot;
using System;

public class canBeSelected : TextureRect
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    bool isMouseOver;
    float rect_size_y;
    Vector2 origPos;
    Color origColor;
    Tween tween;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rect_size_y = RectSize.y * RectScale.y;
        tween = GetNode<Tween>("Tween");
        origPos = RectPosition;
        origColor = SelfModulate;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            // Convert global mouse position to local
            Vector2 localMousePos = (eventMouseMotion.Position);

            if (GetRect().HasPoint(localMousePos))
            {
                //TODO: onMouse Enter
                //hover effect
                isMouseOver = true;
                //Modulate = new Color(0, 0, 0, 0.5f);
                OnHover();
            }

            else
            {
                //revert hover effect
                //tween.StopAll();
                isMouseOver = false;
                //Modulate = new Color(1, 1, 1, 1);
                //OnHoverOver();
                RectPosition = origPos;
            }
        }

        if (@event is InputEventMouseButton eventMouseButton && GetRect().HasPoint(eventMouseButton.Position))
        {
            if (eventMouseButton.Pressed)
            {
                //mouse down - item clicked

            }
            else
            {
                //mouse up - item selected

            }
        }
    }

    public void OnHover()
    {
        GD.Print("mouse over item");
        float startValue = this.RectPosition.y;
        float endValue = origPos.y - (rect_size_y / 2);
        // Stop any ongoing tween operation.
        tween.StopAll();
        tween.InterpolateProperty(this, "rect_position:y",
                                  startValue, endValue,
                                  0.2f, 
                                  Tween.TransitionType.Linear,
                                  Tween.EaseType.In);

        tween.Start();
    }

    public void OnHoverOver()
    {
        float startValue = this.RectPosition.y;
        float endValue = origPos.y;
        // Stop any ongoing tween operation.
        tween.StopAll();
        tween.InterpolateProperty(this, "rect_position:y",
                                  startValue, endValue,
                                  0.1f,
                                  Tween.TransitionType.Linear,
                                  Tween.EaseType.Out);

        tween.Start();
    }
}
