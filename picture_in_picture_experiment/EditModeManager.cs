using Godot;
using System;

public class EditModeManager : Camera2D
{
    Sprite computer_screen;

    public override void _Ready()
    {
        computer_screen = GetParent().GetNode<Sprite>("computer_screen");
        Zoom = Vector2.One * 0.69f;
        current_zoom_level = Zoom.x;

        UpdateScaleAndPosition();
    }

    public static bool edit_mode = false;
    float current_zoom_level = 1.0f;
    float desired_zoom_level = 1.0f;

    float game_mode_zoom_level = 1.0f;
    float edit_mode_zoom_level = 1.0f;

    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.Tab))
        {
            edit_mode = true;
        }
        else
            edit_mode = false;

        if (edit_mode)
            desired_zoom_level = edit_mode_zoom_level;
        else
            desired_zoom_level = game_mode_zoom_level;

        current_zoom_level += (desired_zoom_level - current_zoom_level) * 0.1f;

        UpdateScaleAndPosition(); // Continuously update the scale and position to match window size
    }

    private void UpdateScaleAndPosition()
    {
        Vector2 tex_size = computer_screen.Texture.GetSize();
        Vector2 scaled_size = new Vector2 (tex_size.x * computer_screen.Scale.x, tex_size.y * computer_screen.Scale.y);

        var windowSize = OS.WindowSize;
        //float zoomX = windowSize.x / scaled_size.x;
        //float zoomY = windowSize.y / scaled_size.y;

        
        //Zoom = new Vector2(zoom, zoom);
        GD.Print("tex size : " + tex_size + " size required : " + scaled_size + " window size : " + windowSize);

        float screen_to_window_ratio = scaled_size.x / windowSize.x;
        game_mode_zoom_level = screen_to_window_ratio;
        edit_mode_zoom_level = game_mode_zoom_level * 2f;
        //Zoom = Vector2.One * game_mode_zoom_level;
        
        Position = computer_screen.Position;
        Zoom = Vector2.One * current_zoom_level;

        // Scale
        //var scaleX = windowSize.x / Texture.GetWidth();
        //var scaleY = windowSize.y / Texture.GetHeight();
        //Scale = new Vector2(scaleX, scaleY) * current_scale_factor;
    }
}
