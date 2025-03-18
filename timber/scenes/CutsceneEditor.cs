using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public partial class CutsceneEditor : Control
{
    [Export]private List<CutsceneImageResource> CutsceneImages = new List<CutsceneImageResource>();
    private const string ConfigPath = "res://config/intro_cutscene_config.json";
    [Export] private CutsceneManager cutsceneManager;
    private bool isVisible = false;

    [Export]private ItemList sceneList;
    [Export] private VBoxContainer vBox;
    [Export]private TextureRect previewImage;
    [Export]private LineEdit imagePathInput;
    [Export]private SpinBox indexInput;
    [Export]private OptionButton transitionDropdown, displayDropdown;
    [Export] private PackedScene slidePreviewScene;
    
    public override void _Ready()
    { 
        cutsceneManager = GetParent<CutsceneManager>();
        GD.Print("Cutscene Manager is found:" + (cutsceneManager != null).ToString());
        sceneList = GetNode<ItemList>("ItemList");
        vBox = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
        //string filePath = "res://temp_cutscenes/intro_cutscene_config.json";
        //filePath = ProjectSettings.GlobalizePath(filePath);
        //cutsceneManager.LoadCutsceneFromJsonS3(filePath);
        CutsceneImages = cutsceneManager.cutsceneImages;
        slidePreviewScene = (PackedScene)ResourceLoader.Load("res://temp_scenes/SlidePreview.tscn");
        GD.Print(("packedscene slide preview loaded" + (slidePreviewScene != null)));
        //SetupUI();
        PopulateList();
    }

    public  override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("toggle_cutscene_editor"))
        {
            ToggleEditor();
        }
    }

    private void LoadCutscenes()
    {
        //TODO read from config
        RefreshSceneList();
    }

    private void SetupUI()
    {
        /*sceneList = GetNode<ItemList>("SceneList");
        previewImage = GetNode<TextureRect>("PreviewImage");
        imagePathInput = GetNode<LineEdit>("ImagePath");
        indexInput = GetNode<SpinBox>("Index");
        transitionDropdown = GetNode<OptionButton>("TransitionStyle");
        displayDropdown = GetNode<OptionButton>("DisplayStyle");

        sceneList.Connect("item_selected", this, nameof(OnSceneSelected));*/
    }

    private void RefreshSceneList()
    {
        sceneList.Clear();
        foreach (var slide in CutsceneImages)
        {
            sceneList.AddItem($"Scene {slide.Index}: {slide.ImagePath}");
        }
    }

    private void PopulateList()
    {
        string filePath = "res://temp_cutscenes/intro_cutscene_config.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        cutsceneManager.LoadCutsceneFromJsonS3(filePath);
        CutsceneImages = cutsceneManager.cutsceneImages;
        GD.Print("Populating Cutscene List");
        foreach (var image in CutsceneImages)
        {
            SlidePreview slidePreview = (SlidePreview)slidePreviewScene.Instance();
            GD.Print(slidePreview + "added"); 
            //sceneList.AddChild(slidePreview);
            vBox.AddChild(slidePreview);
            slidePreview.SetPreview(image);
        }
    }

    private void OnSceneSelected(int index)
    {
        var selectedScene = CutsceneImages[index];
        imagePathInput.Text = selectedScene.ImagePath;
        indexInput.Value = selectedScene.Index;
        transitionDropdown.Text = selectedScene.TransitionStyle;
        displayDropdown.Text = selectedScene.DisplayStyle;

        // Load preview
        var img = ResourceLoader.Load<Texture>(selectedScene.ImagePath);
        if (img != null) previewImage.Texture = img;
    }

    private void SaveCutscenes()
    {
        // TODO write to config
        GD.Print("Cutscene config saved!");
    }

    private void AddCutscene()
    {
        var newSlide = new CutsceneImageResource()
        {
            Index = CutsceneImages.Count,
            ImagePath = "cutscenes/new.png",
            TransitionStyle = "fade_up",
            DisplayStyle = "standard",
        };
        
       CutsceneImages.Add(newSlide);
        RefreshSceneList();
    }

    private void RemoveCutscene()
    {
        if (sceneList.GetSelectedItems().Length == 0) return;

        int selectedIndex = sceneList.GetSelectedItems()[0];
        CutsceneImages.RemoveAt(selectedIndex);
        RefreshSceneList();
    }
    
    private int GetOptionButtonIndex(OptionButton optionButton, string value)
    {
        for (int i = 0; i < optionButton.GetItemCount(); i++)
        {
            if (optionButton.GetItemText(i) == value)
                return i;
        }
        return 0; 
    }

    
    private void ToggleEditor()
    {
        isVisible = !isVisible;
        Visible = isVisible;
        GetTree().Paused = isVisible;
        GD.Print(isVisible ? "Cutscene Editor Opened" : "Cutscene Editor Closed");
        
    }
}

