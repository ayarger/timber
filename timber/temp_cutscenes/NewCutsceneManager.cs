using System;
using Godot;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class NewCutsceneImageData
{
    public string ImagePath { get; set; }
    public string TransitionStyle { get; set; }
    public string DisplayStyle { get; set; }
    public int Order { get; set; }
}

public class NewCutsceneManager : Node
{
    public static NewCutsceneManager Instance { get; private set; }
    public List<CutsceneImageResource> CutsceneImages { get; private set; } = new List<CutsceneImageResource>();

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

        string filePath = "res://temp_cutscenes/intro_cutscene_config.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        LoadCutsceneFromJson(filePath);
    }

    public void LoadCutsceneFromJson(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            GD.PrintErr("JSON file not found: " + filePath);
            CutsceneImages.Clear();
            return;
        }

        string json = System.IO.File.ReadAllText(filePath);
        List<CutsceneImageData> jsonData = JsonConvert.DeserializeObject<List<CutsceneImageData>>(json);
        CutsceneImages.Clear();

        foreach (var data in jsonData)
        {
            CutsceneImageResource cutsceneImage = new CutsceneImageResource
            {
                ImagePath = data.ImagePath,
                TransitionStyle = data.TransitionStyle,
                DisplayStyle = data.DisplayStyle,
                Index = data.Order
            };

            LoadCutSceneImage(data.ImagePath, texture => cutsceneImage.Image = texture);
            CutsceneImages.Add(cutsceneImage);
        }

        GD.Print("Cutscene data loaded from: " + filePath);
    }

    public void SaveCutsceneToJson(string filePath)
    {
        List<NewCutsceneImageData> jsonData = new List<NewCutsceneImageData>();
        foreach (var imageResource in CutsceneImages)
        {
            jsonData.Add(new NewCutsceneImageData
            {
                ImagePath = imageResource.ImagePath,
                TransitionStyle = imageResource.TransitionStyle,
                DisplayStyle = imageResource.DisplayStyle,
                Order = imageResource.Index
            });
        }

        string json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        System.IO.File.WriteAllText(filePath, json);
        GD.Print("Cutscene info saved to: " + filePath);
    }

    private void LoadCutSceneImage(string imagePath, Action<Texture> onLoaded)
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
}
