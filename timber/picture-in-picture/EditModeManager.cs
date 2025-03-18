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
            UpdateScaleAndPosition();
        }

        // interpolate zoom and position
        current_zoom_level += (desired_zoom_level - current_zoom_level) * 0.1f;
        Zoom = Vector2.One * current_zoom_level;

        Position = Position.LinearInterpolate(edit_mode ? edit_mode_position : game_mode_position, 0.1f);
    }

    private void UpdateScaleAndPosition()
    {
        Vector2 viewportSize = GetViewportRect().Size; // Use viewport size instead of window size

        if (!edit_mode)
        {
            Vector2 comp_tex_size = computer_screen.Texture.GetSize();
            Vector2 comp_scaled_size = new Vector2(comp_tex_size.x * computer_screen.Scale.x, comp_tex_size.y * computer_screen.Scale.y);

            float zoomX = viewportSize.x / comp_scaled_size.x;
            float zoomY = viewportSize.y / comp_scaled_size.y;
            game_mode_zoom_level = Mathf.Min(zoomX, zoomY);

            game_mode_position = computer_screen.Position + (comp_tex_size * computer_screen.Scale * 0.5f);
            desired_zoom_level = game_mode_zoom_level * 0.33f; // fix zoom
        }
        else // Edit mode
        {
            Vector2 bg_tex_size = bg_screen.Texture.GetSize();
            Vector2 bg_scaled_size = new Vector2(bg_tex_size.x * bg_screen.Scale.x, bg_tex_size.y * bg_screen.Scale.y);

            float zoomX = viewportSize.x / bg_scaled_size.x;
            float zoomY = viewportSize.y / bg_scaled_size.y;
            edit_mode_zoom_level = Mathf.Min(zoomX, zoomY);


            edit_mode_position = bg_screen.Position + bg_tex_size * bg_screen.Scale * 0.5f;
            desired_zoom_level = edit_mode_zoom_level;
        }
    }

    private void OnWindowSizeChanged()
    {
        UpdateScaleAndPosition();
    }

    // public Vector2 UpdateCursorPosition(Vector2 rawCursorPosition)
    // {
    //     Vector2 viewportSize = GetViewportRect().Size;
    //     Vector2 cursorPos;
    //     GD.Print("Edit Mode Active: " + edit_mode);
    //     // if (!edit_mode)
    //     // {
    //     //     // Calculate cursor pos relative to computer_screen
    //     //     Vector2 comp_tex_size = computer_screen.Texture.GetSize();
    //     //     Vector2 comp_scaled_size = comp_tex_size * computer_screen.Scale;

    //     //     Vector2 comp_top_left = computer_screen.GlobalPosition - (comp_scaled_size * 0.5f);
    //     //     Vector2 normalizedCursorPos = (rawCursorPosition - comp_top_left) / comp_scaled_size;

    //     //     cursorPos = normalizedCursorPos * comp_scaled_size;
    //     // }
    //     // else
    //     // {
    //     //     // Calculate  cursor pos relative to bg_screen
    //     //     Vector2 bg_tex_size = bg_screen.Texture.GetSize();
    //     //     Vector2 bg_scaled_size = bg_tex_size * bg_screen.Scale;

    //     //     Vector2 bg_top_left = bg_screen.GlobalPosition - (bg_scaled_size * 0.5f);
    //     //     Vector2 normalizedCursorPos = (rawCursorPosition - bg_top_left) / bg_scaled_size;

    //     //     cursorPos = normalizedCursorPos * bg_scaled_size;
    //     // }
    //     Vector2 comp_tex_size = computer_screen.Texture.GetSize();
    //     Vector2 comp_scaled_size = comp_tex_size * computer_screen.Scale;

    //     // Calculate the top-left corner of the computer_screen in global space
    //     Vector2 comp_top_left = computer_screen.GlobalPosition - (comp_scaled_size * 0.5f);

    //     // Normalize the cursor position relative to the computer screen
    //     Vector2 normalizedCursorPos = (rawCursorPosition - comp_top_left) / comp_scaled_size;

    //     // Final cursor position relative to computer_screen
    //     Vector2 intendedPos = normalizedCursorPos * comp_tex_size;

    //     Vector2 adjustedScale = intendedPos / rawCursorPosition;

    //     if (!edit_mode)
    //     {
    //         // Calculate cursor position relative to computer_screen
            
    //         cursorPos = intendedPos / adjustedScale;
            
    //     }
    //     else
    //     {
    //         // Calculate cursor position relative to bg_screen
    //         Vector2 bg_tex_size = bg_screen.Texture.GetSize();
    //         Vector2 bg_scaled_size = bg_tex_size * bg_screen.Scale;

    //         // Calculate the top-left corner of the bg_screen in global space
    //         Vector2 bg_top_left = bg_screen.GlobalPosition - (bg_scaled_size * 0.5f);

    //         // Normalize the cursor position relative to the background screen
    //         normalizedCursorPos = (rawCursorPosition - bg_top_left) / bg_scaled_size;

    //         // Final cursor position relative to bg_screen
    //         cursorPos = normalizedCursorPos * bg_tex_size;
    //         cursorPos /= adjustedScale;
    //     }

    //     return cursorPos;
    // }

    public Vector2 UpdateCursorPosition(Vector2 rawCursorPosition)
    {
        // temp
        return rawCursorPosition;

        // If not in edit mode, return the cursor position as-is
        if (!edit_mode)
        {
            return rawCursorPosition;
        }

        // Calculate cursor position relative to bg_screen
        Vector2 bg_tex_size = bg_screen.Texture.GetSize();
        Vector2 bg_scaled_size = bg_tex_size * bg_screen.Scale;

        // Top-left corner of the bg_screen in global space
        Vector2 bg_top_left = bg_screen.GlobalPosition - (bg_scaled_size * 0.5f);

        // Map the raw cursor position to the screen's local coordinate space
        Vector2 normalizedCursorPos = (rawCursorPosition - bg_top_left) / bg_scaled_size;

        // Convert back to the background screen's coordinate space
        Vector2 transformedCursorPos = normalizedCursorPos * bg_tex_size;

        return transformedCursorPos;
    }



    public Vector2 GetCursorGridPosition(Vector2 rawCursorPosition)
    {
        Vector2 viewportSize = GetViewportRect().Size;

        if (!edit_mode)
        {
            // Compute position relative to computer_screen
            Vector2 comp_tex_size = computer_screen.Texture.GetSize();
            Vector2 comp_scaled_size = comp_tex_size * computer_screen.Scale;

            Vector2 comp_top_left = computer_screen.GlobalPosition - (comp_scaled_size * 0.5f);
            Vector2 normalizedCursorPos = (rawCursorPosition - comp_top_left) / comp_scaled_size;

            // Convert to grid-aligned position
            return normalizedCursorPos * comp_scaled_size;
        }
        else
        {
            // Compute position relative to bg_screen
            Vector2 bg_tex_size = bg_screen.Texture.GetSize();
            Vector2 bg_scaled_size = bg_tex_size * bg_screen.Scale;

            Vector2 bg_top_left = bg_screen.GlobalPosition - (bg_scaled_size * 0.5f);
            Vector2 normalizedCursorPos = (rawCursorPosition - bg_top_left) / bg_scaled_size;

            // Convert to grid-aligned position
            return normalizedCursorPos * bg_scaled_size;
        }
    }


}
