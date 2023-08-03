using Amazon.CloudFront.Model;
using Godot;
using System;
using System.Collections;

public class TransitionSystem : Node
{
    static TransitionSystem instance;

    Panel panel;

    ShaderMaterial panel_material;

    int hold_requests = 0; /* "Holds" may be used to delay the unveiling of a new scene */
    public static void RequestHold()
    {
        instance.hold_requests++;
    }

    public static void RemoveHold()
    {
        instance.hold_requests--;
    }

    public override void _Ready()
    {
        instance = this;
        panel = GetNode<Panel>("Panel");
        panel.Visible = true;
        panel.Modulate = new Color(0, 0, 0, 1.0f);
        panel.MouseFilter = Godot.Control.MouseFilterEnum.Stop;
        panel_material = (ShaderMaterial)panel.Material;

        hole_size_percentage = 0.0f;
        hole_position_pixels = OS.WindowSize * 0.5f;
        ArborCoroutine.StartCoroutine(DoIntro(), this);
    }

    IEnumerator DoIntro()
    {
        yield return ArborCoroutine.WaitForSeconds(0.5f);

        float loading_countdown = 0.1f;
        while (loading_countdown > 0.0f)
        {
            loading_countdown -= instance.GetProcessDeltaTime();
            if (ArborResource.NumberAssetsCurrentlyLoading())
                loading_countdown = 0.25f;

            yield return null;
        }

        hole_size_percentage = 0.0f;
        should_block = false;

        /* Fade back in */
        void DoGrowHole(float progress)
        {
            instance.hole_size_percentage = progress;
        }

        yield return ArborCoroutine.DoOverTime(DoGrowHole, 0.75f);
    }

    public static bool should_block = true;

    float hole_size_percentage = 1.0f;
    Vector2 hole_position_pixels = new Vector2(0, 0);

    public override void _Process(float delta)
    {
        if(!should_block)
        {
            
            panel.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;
        }
        else
        {
            panel.MouseFilter = Godot.Control.MouseFilterEnum.Pass;
        }

        panel_material.SetShaderParam("window_resolution", OS.WindowSize);

        float diagonal_length = Mathf.Sqrt(OS.WindowSize.x * OS.WindowSize.x + OS.WindowSize.y * OS.WindowSize.y);
        float hole_size_pixels = hole_size_percentage * diagonal_length * 0.5f;
        panel_material.SetShaderParam("hole_radius_pixels", hole_size_pixels);

        hole_position_pixels = OS.WindowSize * 0.5f;
        panel_material.SetShaderParam("hole_position_pixels", hole_position_pixels);

        Raise();
    }

    public static void RequestTransition(string scene_path)
    {
        ArborCoroutine.StartCoroutine(DoRequestTransition(scene_path), instance);
    }

    bool transitioning = false;
    public static IEnumerator DoRequestTransition(string scene_path)
    {
        if (instance.transitioning)
            yield break;
        instance.transitioning = true;

        /* Shrink hole */
        void DoShrinkHole(float progress)
        {
            instance.hole_size_percentage = 1.0f - progress;
        }

        should_block = true;
        yield return ArborCoroutine.DoOverTime(DoShrinkHole, 0.75f);

        /* Do Transition */
        if(scene_path == "quit")
        {
            instance.GetTree().Quit();
        }
        else
        {
            PackedScene new_scene = ResourceLoader.Load<PackedScene>(scene_path);
            instance.GetTree().ChangeSceneTo(new_scene);
        }

        instance.hole_size_percentage = 0.0f;

        float loading_countdown = 0.25f;
        while(loading_countdown > 0.0f)
        {
            loading_countdown -= instance.GetProcessDeltaTime();
            if (ArborResource.NumberAssetsCurrentlyLoading())
                loading_countdown = 0.25f;

            yield return null;
        }

        while (LuaLoader.IsLoadingScene())
            yield return null;

        while (instance.hold_requests > 0)
            yield return null;

        /* Fade back in */
        should_block = false;

        void DoGrowHole(float progress)
        {
            instance.hole_size_percentage = progress;
        }

        yield return ArborCoroutine.DoOverTime(DoGrowHole, 0.75f);

        instance.transitioning = false;
    }
}
