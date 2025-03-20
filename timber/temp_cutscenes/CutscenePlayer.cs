using Godot;
using System.Collections.Generic;

public class CutscenePlayer : CanvasLayer
{
    [Export] private float transitionDuration = 1.0f;
    private int currentImageIndex = 0;
    private TextureRect imageDisplay;
    private Tween transitionTween;
    private bool isPlaying = false;

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

    private void EndCutscene()
    {
        GD.Print("Cutscene Finished");
        isPlaying = false;
        TransitionSystem.RequestTransition("res://Main.tscn");
    }
}
