using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public class CutscenePlayer : CanvasLayer
{
    public static CutscenePlayer Instance { get; private set; }
    [Export] private float transitionDuration = 1.0f;
    [Export] public bool playAutomatically = false;
    private float autoPlayTimer = 0f;
    [Export] private float autoAdvanceDelay = 1f; // seconds per slide
    private int currentImageIndex = 0;
    private TextureRect imageDisplay;
    private Tween transitionTween;
    private bool isPlaying = false;
    private float oscillationTimer = 0; // For sin_vertical
    private float vibrateTimer = 0; // For vibrate
    private Vector2 originalPosition;


    [Export]private List<CutsceneImageResource> cutsceneImages;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree(); // Prevent multiple instances
            return;
        }
        imageDisplay = GetNode<TextureRect>("TextureRect");
        transitionTween = new Tween();
        AddChild(transitionTween);

        if (CutsceneManager.Instance != null)
        {
            cutsceneImages = CutsceneManager.Instance.cutsceneImages;
        }
        // Connecting with Editor for Cutscene Preview
       ConnectToEditor();
      this.Hide();
    }

    private void ConnectToEditor()
    {
        var editor = GetParent<CutsceneEditor>();
        if (editor != null && !editor.IsConnected("CutsceneUpdated", this, nameof(OnCutsceneUpdated)))
        {
            GD.Print("Connecting to CutsceneEditor...");
            editor.Connect("CutsceneUpdated", this, nameof(OnCutsceneUpdated));
        }
        else if (editor == null)
        {
            GD.PrintErr("CutsceneEditor.Instance is still null.");
        }
    }


    
    public override void _Process(float delta)
    {
        this.Visible = CutsceneEditor.Instance.Visible;
        if (!isPlaying) return;

        if (playAutomatically)
        {
            autoPlayTimer += delta;

            if (autoPlayTimer >= autoAdvanceDelay)
            {
                autoPlayTimer = 0f;
                GoToNextImage();
            }
        }
        if (isPlaying && currentImageIndex < cutsceneImages.Count)
        {
            var displayStyle = cutsceneImages[currentImageIndex].DisplayStyle;

            // Apply styles that require per-frame updates
            if (displayStyle == "sin_vertical")
            {
                ApplySinVerticalEffect(delta);
            }
            else if (displayStyle == "vibrate")
            {
                ApplyVibrateEffect(delta);
            }
        }
    }

    public void StartCutscene()
    {
        if (cutsceneImages == null || cutsceneImages.Count == 0)
        {
            GD.PrintErr("CutscenePlayer: No images to play.");
            return;
        }
        GD.Print("starting cutscene...");

        currentImageIndex = 0;
        isPlaying = true;
        LoadImage(cutsceneImages[0]);
        Show();
    }

    public override void _Input(InputEvent @event)
    {
        if (!playAutomatically && isPlaying && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GoToNextImage();
        }
    }

    private void GoToNextImage()
    {
        if (currentImageIndex + 1 >= cutsceneImages.Count)
        {
            EndCutscene();
            return;
        }

        currentImageIndex++;
        LoadImage(cutsceneImages[currentImageIndex]);
    }

    private void LoadImage(CutsceneImageResource cutsceneImage)
    {
        imageDisplay.Texture = cutsceneImage.Image;
        ApplyTransition(cutsceneImage.TransitionStyle,cutsceneImage.DisplayStyle);
    }

    private void ApplyTransition(string transitionStyle,string displayStyle)
    {
        switch (transitionStyle)
        {
            case "fade_up":
                FadeUpTransition();
                break;
            case "bounce":
                BounceTransition();
                break;
            case "instant":
                InstantTransition();
                break;
            default:
                GD.PrintErr($"Unknown transition style: {transitionStyle}");
                InstantTransition();
                break;
        }
        ApplyDisplayStyle(displayStyle);
    }

    private void FadeUpTransition()
    {
        imageDisplay.Modulate = new Color(1, 1, 1, 0);
        transitionTween.InterpolateProperty(
            imageDisplay, "modulate:a", 0, 1, transitionDuration, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        transitionTween.Start();
    }

    private void BounceTransition()
    {
        imageDisplay.RectScale = new Vector2(0.8f*0.3f, 0.8f *0.3f);
        transitionTween.InterpolateProperty(
            imageDisplay, "rect_scale", new Vector2(0.8f * 0.3f, 0.8f * 0.3f), new Vector2(1*0.3f, 1*0.3f), transitionDuration,
            Tween.TransitionType.Bounce, Tween.EaseType.Out);
        transitionTween.Start();
    }

    private void InstantTransition()
    {
        // No animation, just show the new image
    }
    
    private void ApplyDisplayStyle(string displayStyle)
    {
        switch (displayStyle)
        {
            case "standard":
                //imageDisplay.RectPosition = Vector2.Zero;
                break;
            case "shake_small":
                ShakeEffect(5);
                break;
            case "shake_large":
                ShakeEffect(20);
                break;
            case "sin_vertical":
                oscillationTimer = 0;
                break;
            case "vibrate":
                vibrateTimer = 0;
                originalPosition = imageDisplay.RectPosition;
                break;
            default:
                GD.PrintErr($"Unknown display style: {displayStyle}");
                break;
        }
    }
    
    private async void ShakeEffect(int intensity)
    {
        Vector2 originalPosition = imageDisplay.RectPosition;
        for (int i = 0; i < 10; i++)
        {
            imageDisplay.RectPosition = originalPosition + new Vector2(
                GD.Randf() * intensity - intensity / 2,
                GD.Randf() * intensity - intensity / 2
            );
            await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        }
        imageDisplay.RectPosition = originalPosition;
    }
    
    private void ApplyVibrateEffect(float delta)
    {
        vibrateTimer += delta;
        if (vibrateTimer >= 0.02f) 
        {
            vibrateTimer = 0;
            imageDisplay.RectPosition = originalPosition + new Vector2(
                GD.Randf() * 10 - 5, 
                GD.Randf() * 10 - 5 
            );
        }
    }
    private void ApplySinVerticalEffect(float delta)
    {
        oscillationTimer += delta;
        float oscillation = Mathf.Sin(oscillationTimer * 5) * 20; 
        imageDisplay.RectPosition = new Vector2(imageDisplay.RectPosition.x, oscillation);
    }
    
    private void OnCutsceneUpdated()
    {
        GD.Print("Cutscene updated — refreshing preview.");
        playAutomatically = true;
        this.Show();
        if (CutsceneManager.Instance != null)
        {
            /*string filePath = "res://temp_cutscenes/intro_cutscene_config_test.json";
            filePath = ProjectSettings.GlobalizePath(filePath);
            CutsceneManager.Instance.LoadCutseneFromJson(filePath);*/
            cutsceneImages = CutsceneManager.Instance.cutsceneImages;
            StartCutscene(); // replays from the start
        }
    }

    public void UpdateCutscenePreview()
    {
        playAutomatically = true;
        this.Show();
        if (CutsceneManager.Instance != null)
        {
            cutsceneImages = CutsceneManager.Instance.cutsceneImages;
            StartCutscene(); // replays from the start
        }
    }

    private void EndCutscene()
    {
        GD.Print("Cutscene Finished");
        isPlaying = false;
        TransitionSystem.RequestTransition("res://Main.tscn");
    }
    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
