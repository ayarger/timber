using Godot;

public class MenuHeader : Control
{
    public Control top_window_control;

    public override void _Ready()
    {
        // Connect the gui_input signal to the OnGuiInput method
        Connect("gui_input", this, nameof(OnGuiInput));
    }

    bool dragging = false;

    private void OnGuiInput(InputEvent @event)
    {
        // Check if the input event is a mouse button event
        if (@event is InputEventMouseButton eventMouseButton)
        {
            // Check if the left mouse button was pressed
            if (eventMouseButton.ButtonIndex == (int)ButtonList.Left && eventMouseButton.Pressed)
            {
                GD.Print("header Control was clicked!");
                dragging = true;
            }
        }
    }

    public void Configure(Control _top_window_control)
    {
        top_window_control = _top_window_control;
    }

    Vector2 previous_position = Vector2.Zero;
    public override void _Process(float delta)
    {
        base._Process(delta);

        CalculateMouseMovement();
    }

    void CalculateMouseMovement()
    {
        Vector2 new_cursor_pos = GetGlobalMousePosition();

        if (dragging && Input.IsMouseButtonPressed((int)ButtonList.Left))
        {
            Vector2 cursor_delta = new_cursor_pos - previous_position;
            top_window_control.RectPosition += cursor_delta;
        }
        else
        {
            dragging = false;
        }

        previous_position = new_cursor_pos;
    }
}