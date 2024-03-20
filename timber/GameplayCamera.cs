using Godot;
using System;
using System.Text.RegularExpressions;

public class GameplayCamera : Camera
{
    float desired_zoom_factor = 15;
    Vector3 desired_xz_position = new Vector3(0, 0, 0);

    static GameplayCamera instance;
    public static void SetDesiredXZPosition(Vector3 pos) { instance.desired_xz_position = pos; }
    public static Vector3 GetDesiredXZPosition() { return instance.desired_xz_position; }
    public static void SetDesiredZoom(float z) { instance.desired_zoom_factor = z; }
    public static float GetDesiredZoom() { return instance.desired_zoom_factor; }

    public static GameplayCamera GetGameplayCamera() { return instance; }

    public override void _Ready()
    {
        instance = this;
        ForceNewState(new IntroCutsceneCameraState());
    }

    public override void _Process(float delta)
    {
        ProcessCurrentState(delta);
        ProcessUniversalMovement();
    }

    void ProcessUniversalMovement()
    {
        /* Position */
        desired_xz_position = new Vector3(desired_xz_position.x, 0.0f, desired_xz_position.z);

        Vector3 final_position = desired_xz_position + Transform.basis.z * desired_zoom_factor;
        GlobalTranslation += (final_position - GlobalTranslation) * 0.1f;
    }


    GameplayCameraState current_state;
    void ProcessCurrentState(float delta)
    {
        if (current_state != null)
            current_state.OnUpdate(delta);
        else
            current_state = new PlayerControlledCameraState();

        if(current_state.IsFinished())
        {
            current_state = null;
        }
    }

    void ForceNewState(GameplayCameraState new_state)
    {
        current_state = new_state;
    }

    Vector2 current_mouse_screen_position = Vector2.Zero;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            current_mouse_screen_position = eventMouseMotion.Position;
        }

        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton emb = (InputEventMouseButton)@event;
            if (emb.IsPressed())
            {
                if (emb.ButtonIndex == (int)ButtonList.WheelUp && current_state.ZoomControlsAllowed())
                {
                    desired_zoom_factor -= 3.0f;
                }
                if (emb.ButtonIndex == (int)ButtonList.WheelDown && current_state.ZoomControlsAllowed())
                {
                    desired_zoom_factor += 3.0f;
                }
            }
        }
    }
}

public abstract class GameplayCameraState
{
    public abstract void OnUpdate(float delta);
    public abstract bool IsFinished();
    public abstract bool SelectionCursorAllowed();
    public abstract bool ZoomControlsAllowed();
}

public class PlayerControlledCameraState : GameplayCameraState
{
    public override void OnUpdate(float delta)
    {
        /* XZ movement */
        Vector3 input = new Vector3();
        Vector2 window_resolution = OS.WindowSize;
        Vector2 cursor_window_position = SelectionSystem.GetCursorWindowPosition();
        bool cursor_inside_window = SelectionSystem.CursorInsideWindow();
        bool window_focused = SelectionSystem.WindowFocused();


        if (Input.IsPhysicalKeyPressed((int)KeyList.W) && false || (cursor_window_position.y < window_resolution.y * 0.1f && cursor_inside_window && window_focused))
            input += Vector3.Forward;

        if (Input.IsPhysicalKeyPressed((int)KeyList.S) && false|| (cursor_window_position.y > window_resolution.y * 0.9f && cursor_inside_window && window_focused))
            input += Vector3.Back;

        if (Input.IsPhysicalKeyPressed((int)KeyList.D) && false || (cursor_window_position.x > window_resolution.x * 0.9f && cursor_inside_window && window_focused))
            input += Vector3.Right;

        if (Input.IsPhysicalKeyPressed((int)KeyList.A) && false || (cursor_window_position.x < window_resolution.x * 0.1f && cursor_inside_window && window_focused))
            input += Vector3.Left;

        Vector3 current_desired_position = GameplayCamera.GetDesiredXZPosition();
        float current_desired_zoom = GameplayCamera.GetDesiredZoom();
        GameplayCamera.SetDesiredXZPosition(current_desired_position + input * delta * (10 + (current_desired_zoom - 25.0f)));

        /* Cap zoom */
        if (current_desired_zoom < 25.0f)
            GameplayCamera.SetDesiredZoom(25.0f);
        if (current_desired_zoom > 70.0f)
            GameplayCamera.SetDesiredZoom(70.0f);
    }

    public override bool IsFinished()
    {
        return false; // must be interrupted by force.
    }

    public override bool SelectionCursorAllowed()
    {
        return true;
    }
    public override bool ZoomControlsAllowed()
    {
        return true;
    }
}

public class IntroCutsceneCameraState : GameplayCameraState
{
    float time_to_live = 1.5f;
    public override void OnUpdate(float delta)
    {
        if (LuaLoader.most_recent_load_scene_result == null)
            return;

        Vector3 focus_position = LuaLoader.most_recent_load_scene_result.average_position_of_player_actors + new Vector3(0, 0, -2);

        GameplayCamera.SetDesiredXZPosition(focus_position);
        GameplayCamera.SetDesiredZoom(15.0f);

        time_to_live -= delta;
    }

    public override bool IsFinished()
    {
        if (time_to_live <= 0.0f)
            return true;
        return false;
    }

    public override bool SelectionCursorAllowed()
    {
        return false;
    }
    public override bool ZoomControlsAllowed()
    {
        return false;
    }
}

public class Ray
{
    public Vector3 start;
    public Vector3 direction;

    public Ray(Vector3 _start, Vector3 _direction)
    {
        start = _start;
        direction = _direction.Normalized();
    }
}