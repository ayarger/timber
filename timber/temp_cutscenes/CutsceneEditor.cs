using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

public partial class CutsceneEditor : Control
{
    public static CutsceneEditor Instance { get; private set; } 
    private const string ConfigPath = "res://config/intro_cutscene_config.json";
    [Export] public List<CutsceneImageResource> CutsceneImages = new List<CutsceneImageResource>();
    [Export] private CutsceneManager cutsceneManager;
    private bool isVisible = false;

    [Export]private ItemList sceneList;
    [Export]private VBoxContainer vBox;
    [Export]private GridContainer grid;
    [Export]private TextureRect previewImage;
    [Export]private LineEdit imagePathInput;
    [Export]private SpinBox indexInput;
    [Export]private OptionButton transitionDropdown, displayDropdown;
    [Export]private PackedScene slidePreviewScene;

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
        
        cutsceneManager = GetParent<CutsceneManager>();
        //GD.Print("Cutscene Manager is found:" + (cutsceneManager != null).ToString());
        sceneList = GetNode<ItemList>("ItemList");
        vBox = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
        grid = GetNode<GridContainer>("ScrollContainer/GridContainer");
        //string filePath = "res://temp_cutscenes/intro_cutscene_config.json";
        //filePath = ProjectSettings.GlobalizePath(filePath);
        //cutsceneManager.LoadCutsceneFromJsonS3(filePath);
        CutsceneImages = cutsceneManager.cutsceneImages;
        slidePreviewScene = (PackedScene)ResourceLoader.Load("res://scenes/SlidePreview.tscn");
        GD.Print(("packedscene slide preview loaded" + (slidePreviewScene != null)));
        //SetupUI();
        PopulateList();
        this.Hide();
    }

    public  override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("toggle_cutscene_editor"))
        {
            ToggleEditor();
        }
        CutsceneImages = cutsceneManager.cutsceneImages;
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
       
        foreach (var slide in CutsceneManager.Instance.cutsceneImages)
        {
            sceneList.AddItem($"Scene {slide.Index}: {slide.ImagePath}");
        }
    }

    public void UpdateList()
    { 
        //TODO
        int index = 0;
        grid = GetNode<GridContainer>("ScrollContainer/GridContainer");
        GD.Print("cutsceneImages count:" + CutsceneManager.Instance.cutsceneImages.Count);
        GD.Print( "grid childcount: " + grid.GetChildCount());
        foreach (SlidePreview currSlide in grid.GetChildren())
        {
            if (currSlide.IsVisibleInTree())
            {
                currSlide.UpdatePreview(CutsceneManager.Instance.cutsceneImages[index]);
                index++;
            }
        }
    }

    private void PopulateList()
    {
        string filePath = "res://temp_cutscenes/intro_cutscene_config.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        cutsceneManager.LoadCutsceneFromJsonS3(filePath);
        GD.Print("Populating Cutscene List");
        foreach (var image in cutsceneManager.cutsceneImages)
        {
            SlidePreview slidePreview = (SlidePreview)slidePreviewScene.Instance();
            GD.Print(slidePreview + "added"); 
            //sceneList.AddChild(slidePreview);
            //vBox.AddChild(slidePreview);
            grid.AddChild(slidePreview);
            slidePreview.cutsceneImageResource = image;
            slidePreview.SetPreview(image);
        }
    }

    private void OnSceneSelected(int index)
    {
        var selectedScene = cutsceneManager.cutsceneImages[index];
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

    public void AddCutscene()
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

    public void RemoveCutscene(SlidePreview currSlide)
    {
        if (currSlide != null)
        {
            grid.RemoveChild(currSlide);
        }
        UpdateList();
        //RefreshSceneList();
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

