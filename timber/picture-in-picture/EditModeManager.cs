using Godot;
using System;

public class EditModeManager : Camera2D
{
    Sprite computer_screen;
    Sprite bg_screen;

    public override void _Ready()
    {
        computer_screen = GetParent().GetNode<Sprite>("computer_screen");
        bg_screen = GetParent().GetNode<Sprite>("Editor_bg");
        Zoom = Vector2.One * 0.69f;
        current_zoom_level = Zoom.x;

        // Connect the window size_changed signal
        GetViewport().Connect("size_changed", this, nameof(OnWindowSizeChanged));

        UpdateScaleAndPosition();
    }

    public static bool edit_mode = false;
    float current_zoom_level = 1.0f;
    float desired_zoom_level = 1.0f;

    Vector2 game_mode_position;
    Vector2 edit_mode_position;

    float game_mode_zoom_level = 1.0f;
    float edit_mode_zoom_level = 1.0f;

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("toggle_edit_mode"))
        {
            edit_mode = !edit_mode;
            UpdateScaleAndPosition(); // Recalculate positions and zoom levels when toggling
        }

        // Smoothly interpolate zoom and position
        current_zoom_level += (desired_zoom_level - current_zoom_level) * 0.1f;
        Zoom = Vector2.One * current_zoom_level;

        Position = Position.LinearInterpolate(edit_mode ? edit_mode_position : game_mode_position, 0.1f);
    }

    private void UpdateScaleAndPosition()
    {
        var windowSize = OS.WindowSize;

        if (edit_mode)
        {
            // Use bg_screen size to calculate zoom and position
            Vector2 bg_tex_size = bg_screen.Texture.GetSize();
            Vector2 bg_scaled_size = new Vector2(bg_tex_size.x * bg_screen.Scale.x, bg_tex_size.y * bg_screen.Scale.y);

            // Calculate the zoom level to fit the entire bg_screen in the window
            float zoomX = windowSize.x / bg_scaled_size.x;
            float zoomY = windowSize.y / bg_scaled_size.y;
            edit_mode_zoom_level = Mathf.Min(zoomX, zoomY);

            // Calculate the center position for bg_screen
            edit_mode_position = bg_screen.Position + bg_tex_size * bg_screen.Scale * 0.5f;
            desired_zoom_level = edit_mode_zoom_level;
        }
        else
        {
            // Use computer_screen size for game mode zoom level and position
            Vector2 comp_tex_size = computer_screen.Texture.GetSize();
            Vector2 comp_scaled_size = new Vector2(comp_tex_size.x * computer_screen.Scale.x, comp_tex_size.y * computer_screen.Scale.y);

            // Calculate the zoom level for game mode
            float screen_to_window_ratio = comp_scaled_size.x / windowSize.x;
            game_mode_zoom_level = screen_to_window_ratio;

            // Center position on computer_screen with offset
            game_mode_position = computer_screen.Position + (comp_tex_size * computer_screen.Scale * 0.5f);
            desired_zoom_level = game_mode_zoom_level;
        }
    }

    // This method is called whenever the window size changes
    private void OnWindowSizeChanged()
    {
        GD.Print(OS.WindowSize);
        UpdateScaleAndPosition();
    }
}
