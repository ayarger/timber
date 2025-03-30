using Godot;
using System.Collections.Generic;

public class CutscenePlayer : CanvasLayer
{
    [Export] private float transitionDuration = 1.0f;
    private int currentImageIndex = 0;
    private TextureRect imageDisplay;
    private Tween transitionTween;
    private bool isPlaying = false;
    private float oscillationTimer = 0; // For sin_vertical
    private float vibrateTimer = 0; // For vibrate
    private Vector2 originalPosition;


    private List<CutsceneImageResource> cutsceneImages;

    public override void _Ready()
    {
        imageDisplay = GetNode<TextureRect>("TextureRect");
        transitionTween = new Tween();
        AddChild(transitionTween);

        if (NewCutsceneManager.Instance != null)
        {
            cutsceneImages = NewCutsceneManager.Instance.CutsceneImages;
        }
    }

    public void StartCutscene()
    {
        if (cutsceneImages == null || cutsceneImages.Count == 0)
        {
            GD.PrintErr("CutscenePlayer: No images to play.");
            return;
        }

        currentImageIndex = 0;
        isPlaying = true;
        LoadImage(cutsceneImages[0]);
        Show();
    }

    public override void _Input(InputEvent @event)
    {
        if (isPlaying && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
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
        ApplyTransition(cutsceneImage.TransitionStyle);
    }

    private void ApplyTransition(string transitionStyle)
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
        imageDisplay.RectScale = new Vector2(0.8f, 0.8f);
        transitionTween.InterpolateProperty(
            imageDisplay, "rect_scale", new Vector2(0.8f, 0.8f), new Vector2(1, 1), transitionDuration,
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
                imageDisplay.RectPosition = Vector2.Zero;
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

    private void EndCutscene()
    {
        GD.Print("Cutscene Finished");
        isPlaying = false;
        TransitionSystem.RequestTransition("res://Main.tscn");
    }
}
