using Godot;
using System;

public class isDraggableUI : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Hover Effect
    // Hover Effect
    bool isMouseOver;
    float rectSizeY;
    Vector2 origPos;
    Color origColor;
    public string scenePath;
    //PackedScene packedScene;
    private bool dragging = false;
    private Vector2 dragOffset;
    private TextureRect rectUI;

    [Signal]
    public delegate void Dropped(Vector2 position);


    // Dragging
    Tween tween;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rectSizeY = RectSize.y * RectScale.y;
        tween = GetNode<Tween>("../Tween");
        origPos = rectUI.RectPosition;
        origColor = SelfModulate;
        rectUI = (TextureRect)GetParent();
        // Load PackedScene 
        //packedScene = (PackedScene)ResourceLoader.Load(scenePath);
        SetMouseFilter(MouseFilterEnum.Stop);
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
            Vector2 localMousePos = (eventMouseMotion.Position);

            if (GetRect().HasPoint(localMousePos))
            {
                //TODO: onMouse Enter
                //hover effect
                isMouseOver = true;
                //Modulate = new Color(0, 0, 0, 0.5f);
                if (!dragging)
                {
                    OnHover();
                }
            }

            else
            {
                //revert hover effect
                tween.StopAll();
                isMouseOver = false;
                Modulate = new Color(1, 1, 1, 1);
                //OnHoverOver();
                RectPosition = origPos;
            }
        }

        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == (int)ButtonList.Left)
        {
            if (mouseButtonEvent.Pressed)
            {
                // Start dragging
                dragging = GetRect().HasPoint(mouseButtonEvent.Position);
                dragOffset = mouseButtonEvent.Position - RectPosition;
            }
            else if (dragging)
            {
                // Stop dragging
                dragging = false;
                EmitSignal(nameof(Dropped), RectGlobalPosition);
            }
        }

        if (dragging && @event is InputEventMouseMotion mouseMotionEvent)
        {
            // Update the position of the Control while dragging
            RectPosition = mouseMotionEvent.Position - dragOffset;
        }


    }

    public void OnHover()
    {
        GD.Print("mouse over item");
        float startValue = RectPosition.y;
        float endValue = origPos.y - (rectSizeY / 2);
        // Stop any ongoing tween operation.
        tween.StopAll();
        tween.InterpolateProperty(rectUI, "rect_position:y",
                                  startValue, endValue,
                                  0.2f,
                                  Tween.TransitionType.Linear,
                                  Tween.EaseType.In);

        tween.Start();
    }

    public void OnHoverOver()
    {
        float startValue = RectPosition.y;
        float endValue = origPos.y;
        // Stop any ongoing tween operation.
        tween.StopAll();
        tween.InterpolateProperty(rectUI, "rect_position:y",
                                  startValue, endValue,
                                  0.1f,
                                  Tween.TransitionType.Linear,
                                  Tween.EaseType.Out);

        tween.Start();
    }

    public void OnSelected()
    {
        PackedScene unit = (PackedScene)ResourceLoader.Load("res://temp_scenes/unit.tscn");
        Spatial curr_unit = (Spatial)unit.Instance();
        Spatial Node = GetParent().GetParent().GetParent().GetNode<Spatial>("Node");
        Node.AddChild(curr_unit);
    }
}
