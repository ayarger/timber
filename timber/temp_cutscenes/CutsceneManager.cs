using System;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

public class CutsceneStartEvent
{
    public CutsceneStartEvent()
    {
        
    }
}

public class CutsceneLoadedEvent
{
    public CutsceneLoadedEvent()
    {
        
    }
}

public class CutsceneImageData
{
    public string ImagePath { get; set; } 
    public string TransitionStyle { get; set; }
    public string DisplayStyle { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// A cutscene manager that manages and displays sequential cutscenes with a variety of transition and display styles.
/// </summary>
public class CutsceneManager : CanvasLayer
{
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
    public List<CutsceneImageResource> cutsceneImages = new List<CutsceneImageResource>();

    private Subscription<CutsceneStartEvent> cutsceneStatEvent_sub;

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

        /*string filePath = "res://temp_cutscenes/intro_resources.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        //ConvertCutsceneToJson(filePath);
        LoadCutseneFromJson(filePath);*/
        
        string filePath = "res://temp_cutscenes/intro_cutscene_config.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        // shoud have all image resources loaded correctly
        LoadCutsceneFromJsonS3(filePath);
        
        imageDisplay = GetNode<TextureRect>("TextureRect");
        transitionTween = new Tween();
        AddChild(transitionTween);
        //StartCutscene();
        cutsceneStatEvent_sub = EventBus.Subscribe<CutsceneStartEvent>(StartCurrentCutscnene);
    }

    public void StartCurrentCutscnene(CutsceneStartEvent e)
    {
        StartCutscene();
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

    public void StartCutscene()
    {
        //LoadCutSceneImage("images/victory_bg.png");
        if (cutsceneImages == null || cutsceneImages.Count == 0)
        {
            GD.PrintErr("CutsceneManager: No images provided for the cutscene.");
            return;
        }

        this.cutsceneImages = cutsceneImages;
        currentImageIndex = 0;
        isPlaying = true;

        var currentCutsceneImage = cutsceneImages[0];
        
        LoadCutSceneImage(currentCutsceneImage.ImagePath, texture =>
        {
            imageDisplay.Texture = texture;
            TransitionToImage(currentCutsceneImage);
            Show();
        });
        //TransitionToImage(currentCutsceneImage);
        GD.Print("Starting cutscene");
    }

    private void GoToNextImage()
    {
        if (currentImageIndex + 1 >= cutsceneImages.Count)
        {
            EndCutscene();
            return;
        }

        currentImageIndex++;
        var nextCutsceneImage = cutsceneImages[currentImageIndex];
        TransitionToImage(nextCutsceneImage);
    }

    private void TransitionToImage(CutsceneImageResource cutsceneImage)
    {
        //Texture currCutsceneImage = ResourceLoader.Load<Texture>(cutsceneImage.ImagePath);
        //Texture currCutsceneImage = LoadCutSceneImage(cutsceneImage.ImagePath);
        Texture currCutsceneImage = cutsceneImage.Image;
 
        switch (cutsceneImage.TransitionStyle)
        {
            case "fade_up":
                FadeUpTransition(currCutsceneImage);
                break;
            case "bounce":
                BounceTransition(currCutsceneImage);
                break;
            case "instant":
                InstantTransition(currCutsceneImage);
                break;
            default:
                GD.PrintErr($"Unknown transition style: {cutsceneImage.TransitionStyle}");
                InstantTransition(currCutsceneImage);
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
        imageDisplay.RectScale = new Vector2(0.8f, 0.8f); // Start small
        imageDisplay.Texture = newImage;
  
        imageDisplay.RectPivotOffset = imageDisplay.RectSize / 2;

        transitionTween.InterpolateProperty(
            imageDisplay, "rect_scale", new Vector2(0.8f, 0.8f), new Vector2(1, 1), transitionDuration,
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
        float oscillation = Mathf.Sin(oscillationTimer * 5) * 20; 
        imageDisplay.RectPosition = new Vector2(imageDisplay.RectPosition.x, oscillation);
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

    public void LoadCutSceneImage(string imagePath, Action<Texture> onLoaded)
    {
        ArborResource.UseResource<Texture>(imagePath, texture =>
        {
            if (texture != null)
            {
                onLoaded(texture);
            }
            else
            {
                GD.PrintErr($"Failed to load texture: {imagePath}");
            }
        }, this);
    }
    

    private void EndCutscene()
    {
        GD.Print("Cutscene Finished");
        isPlaying = false;
        //Hide();
        TransitionSystem.RequestTransition(@"res://Main.tscn"); // TODO: dynamically load the next scene
    }

    public void ConvertCutsceneToJson(string filePath)
    {
        GD.Print("start converting intro cutscenes to Json");
        List<CutsceneImageData> jsonData = new List<CutsceneImageData>();
        foreach (var imageResource in cutsceneImages)
        {
            jsonData.Add(new CutsceneImageData
            {
                ImagePath = imageResource.ImagePath, // Save only the file path
                TransitionStyle = imageResource.TransitionStyle,
                DisplayStyle = imageResource.DisplayStyle,
                Order = imageResource.Index
            });
        }

        string json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        System.IO.File.WriteAllText(filePath, json);
        GD.Print("cutscene info saved to: " + filePath);
    }

    public void LoadCutseneFromJson(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            GD.PrintErr("JSON file not found: " + filePath);
            cutsceneImages.Clear();
        }
        string json = System.IO.File.ReadAllText(filePath);
        List<CutsceneImageData> jsonData = JsonConvert.DeserializeObject<List<CutsceneImageData>>(json);
        cutsceneImages.Clear();
        cutsceneImages = new List<CutsceneImageResource>();
        
        GD.Print("start loading");
        foreach (var data in jsonData)
        {
            CutsceneImageResource cutsceneImage = new CutsceneImageResource
            {
                
                ImagePath = data.ImagePath,
                TransitionStyle = data.TransitionStyle,
                DisplayStyle = data.DisplayStyle,
                Index = data.Order
            };
            LoadCutSceneImage(data.ImagePath, texture =>
            {
                cutsceneImage.Image = texture;
            });
            cutsceneImages.Add(cutsceneImage);
        }
        GD.Print("cutscene info loaded from: " + filePath);
    }

    public void LoadCutsceneFromJsonS3(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            GD.PrintErr("JSON file not found: " + filePath);
            cutsceneImages.Clear();
        }
        string json = System.IO.File.ReadAllText(filePath);
        List<CutsceneImageData> jsonData = JsonConvert.DeserializeObject<List<CutsceneImageData>>(json);
        cutsceneImages.Clear();
        cutsceneImages = new List<CutsceneImageResource>();
        
        GD.Print("start loading");
        foreach (var data in jsonData)
        {
            CutsceneImageResource cutsceneImage = new CutsceneImageResource
            {
                
                ImagePath = data.ImagePath,
                TransitionStyle = data.TransitionStyle,
                DisplayStyle = data.DisplayStyle,
                Index = data.Order
            };
            LoadCutSceneImage(data.ImagePath, texture =>
            {
                cutsceneImage.Image = texture;
            });
            cutsceneImages.Add(cutsceneImage);
        }
        GD.Print("cutscene info loaded from: " + filePath);
        //EventBus.Publish(new CutsceneLoadedEvent());
        //EventBus.Publish(new CutsceneStartEvent());
        StartCutscene();
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe(cutsceneStatEvent_sub);
        base._ExitTree();
        if (Instance == this)
            Instance = null;
    }
}


