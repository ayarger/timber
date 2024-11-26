using System;
using Godot;
using System.Collections.Generic;

/// <summary>
/// A cutscene manager that manages and displays sequential cutscenes with a variety of transition and display styles.
/// </summary>
public class CutsceneManager : Control
{
    // Define a data structure for each image and its associated styles.
    private class CutsceneImage
    {
        public Texture Image { get; set; }
        public string TransitionStyle { get; set; }
        public string DisplayStyle { get; set; }
    }

    [Export] private float transitionDuration = 1.0f;
    private int currentImageIndex = 0;

    private TextureRect imageDisplay;
    private Tween transitionTween;
    private float oscillationTimer = 0; // For sin_vertical
    private float vibrateTimer = 0; // For vibrate
    private Vector2 originalPosition;

    public static CutsceneManager Instance { get; private set; }
    private bool isPlaying = false;

    // List of cutscene images with their associated styles.
    [Export]
    private List<CutsceneImage> cutsceneImages = new List<CutsceneImage>();

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
            return;
        }

        imageDisplay = GetNode<TextureRect>("TextureRect");
        transitionTween = new Tween();
        AddChild(transitionTween);
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GoToNextImage();
        }
    }

    public override void _Process(float delta)
    {
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

    public void StartCutscene(List<CutsceneImage> cutsceneImages)
    {
        if (cutsceneImages == null || cutsceneImages.Count == 0)
        {
            GD.PrintErr("CutsceneManager: No images provided for the cutscene.");
            return;
        }

        this.cutsceneImages = cutsceneImages;
        currentImageIndex = 0;
        isPlaying = true;

        var currentCutsceneImage = cutsceneImages[0];
        imageDisplay.Texture = currentCutsceneImage.Image;
        TransitionToImage(currentCutsceneImage);
        Show();
    }

    private void GoToNextImage()
    {
        if (currentImageIndex + 1 >= cutsceneImages.Count)
        {
            GD.Print("Cutscene Finished");
            EndCutscene();
            return;
        }

        currentImageIndex++;
        var nextCutsceneImage = cutsceneImages[currentImageIndex];
        TransitionToImage(nextCutsceneImage);
    }

    private void TransitionToImage(CutsceneImage cutsceneImage)
    {
        switch (cutsceneImage.TransitionStyle)
        {
            case "fade_up":
                FadeUpTransition(cutsceneImage.Image);
                break;
            case "bounce":
                BounceTransition(cutsceneImage.Image);
                break;
            case "instant":
                InstantTransition(cutsceneImage.Image);
                break;
            default:
                GD.PrintErr($"Unknown transition style: {cutsceneImage.TransitionStyle}");
                InstantTransition(cutsceneImage.Image);
                break;
        }

        ApplyDisplayStyle(cutsceneImage.DisplayStyle);
    }

    private void FadeUpTransition(Texture newImage)
    {
        imageDisplay.Modulate = new Color(1, 1, 1, 0); // Start fully transparent
        imageDisplay.Texture = newImage;

        transitionTween.InterpolateProperty(
            imageDisplay, "modulate:a", 0, 1, transitionDuration, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        transitionTween.Start();
    }

    private void BounceTransition(Texture newImage)
    {
        imageDisplay.RectScale = new Vector2(0.5f, 0.5f); // Start small
        imageDisplay.Texture = newImage;

        transitionTween.InterpolateProperty(
            imageDisplay, "rect_scale", new Vector2(0.5f, 0.5f), new Vector2(1, 1), transitionDuration,
            Tween.TransitionType.Bounce, Tween.EaseType.Out);
        transitionTween.Start();
    }

    private void InstantTransition(Texture newImage)
    {
        imageDisplay.Texture = newImage;
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

    private void ApplySinVerticalEffect(float delta)
    {
        oscillationTimer += delta;
        float oscillation = Mathf.Sin(oscillationTimer * 5) * 20; // Oscillate vertically by ±20 pixels
        imageDisplay.RectPosition = new Vector2(imageDisplay.RectPosition.x, oscillation);
    }

    private void ApplyVibrateEffect(float delta)
    {
        vibrateTimer += delta;
        if (vibrateTimer >= 0.02f) 
        {
            vibrateTimer = 0;
            imageDisplay.RectPosition = originalPosition + new Vector2(
                GD.Randf() * 10 - 5, // Random jitter between -5 and +5 horizontally
                GD.Randf() * 10 - 5  // Random jitter between -5 and +5 vertically
            );
        }
    }

    private void EndCutscene()
    {
        GD.Print("Cutscene Finished");
        isPlaying = false;
        Hide();
    }
}
