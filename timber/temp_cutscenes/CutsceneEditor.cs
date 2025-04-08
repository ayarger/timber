using Godot;
using System.Collections.Generic;


public partial class CutsceneEditor : Control
{
    public static CutsceneEditor Instance { get; private set; } 
    private const string ConfigPath = "res://config/intro_cutscene_config.json";
    [Export] public List<CutsceneImageResource> CutsceneImages = new List<CutsceneImageResource>();
    [Export] private CutsceneManager cutsceneManager;
    [Signal] public delegate void CutsceneUpdated();
    private bool isVisible;

    [Export]private ItemList sceneList;
    [Export]private VBoxContainer vBox;
    [Export]private GridContainer grid;
    [Export]private TextureRect previewImage;
    [Export]private LineEdit imagePathInput;
    [Export]private SpinBox indexInput;
    [Export]private OptionButton transitionDropdown, displayDropdown;
    [Export]private PackedScene slidePreviewScene;
    [Export]private SlidePreview previewToDelete;
    [Export] private ConfirmationDialog deleteConfirmDialog;
    [Export] private TextureButton addSlideButton;
    [Export] private Button chooseImageButton;
    [Export] private Button uploadImageButton;
    private bool deleteConfirmed = false;

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
        
        deleteConfirmDialog = GetNode<ConfirmationDialog>("DeleteConfirmDialog");
        deleteConfirmDialog.Connect("confirmed", this, nameof(OnDeleteConfirmed));
        deleteConfirmDialog.Connect("popup_hide", this, nameof(OnDeleteCanceled));
        
        addSlideButton = GetNode<TextureButton>("ScrollContainer/GridContainer/AddSlide/AddSlideButton");
        chooseImageButton = GetNode<Button>("ScrollContainer/GridContainer/AddSlide/ChooseButton");
        uploadImageButton = GetNode<Button>("ScrollContainer/GridContainer/AddSlide/UploadButton");
        chooseImageButton.Hide();
        uploadImageButton.Hide();
        addSlideButton.Connect("pressed", this, nameof(OnAddSlidePressed));
        
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
       RefreshList();
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

    private void RefreshList()
    {
        EmitSignal(nameof(CutsceneUpdated));
        foreach (Node child in grid.GetChildren())
        {
            if (child is SlidePreview) // or use `is SlidePreview`
            {
                child.QueueFree();
            }
        }

        foreach (var image in CutsceneManager.Instance.cutsceneImages)
        {
            SlidePreview slidePreview = (SlidePreview)slidePreviewScene.Instance();
            GD.Print(slidePreview + "added"); 
            grid.AddChild(slidePreview);
            slidePreview.cutsceneImageResource = image;
            slidePreview.CallDeferred(nameof(SlidePreview.SetPreview), image);
        }
        grid.MoveChild(addSlideButton.GetParent(), grid.GetChildCount()-1);
    }

    public void UpdateList()
    { 
        EmitSignal(nameof(CutsceneUpdated));
        //TODO
        int index = 0;
        grid = GetNode<GridContainer>("ScrollContainer/GridContainer");
        GD.Print("cutsceneImages count:" + CutsceneManager.Instance.cutsceneImages.Count);
        GD.Print( "grid childcount: " + grid.GetChildCount());
        var children = grid.GetChildren();

        for (int i = 0; i < children.Count - 1; i++)
        {
            if (children[i] is SlidePreview preview)
            {
                preview.UpdatePreview(CutsceneManager.Instance.cutsceneImages[index]);
                index++;
                GD.Print("SlidePreview #" + i + ": " + preview.cutsceneImageResource.ImagePath);
            }
        }
        grid.MoveChild(addSlideButton.GetParent(), grid.GetChildCount()-1);
      
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
            slidePreview.CallDeferred(nameof(SlidePreview.SetPreview), image);
        }
        grid.MoveChild(addSlideButton.GetParent(), grid.GetChildCount()-1);
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
    
    public void AddSlideFromPath(string imagePath)
    {
        EmitSignal(nameof(CutsceneUpdated));
        var newSlide = new CutsceneImageResource
        {
            Index = CutsceneManager.Instance.cutsceneImages.Count,
            ImagePath = ProjectSettings.LocalizePath(imagePath),
            TransitionStyle = "instant",
            DisplayStyle = "standard"
        };

        CutsceneManager.Instance.cutsceneImages.Add(newSlide);

        var slidePreview = (SlidePreview)slidePreviewScene.Instance();
        slidePreview.cutsceneImageResource = newSlide;
        slidePreview.CallDeferred(nameof(SlidePreview.SetPreview), newSlide);
        grid.AddChild(slidePreview);
        grid.MoveChild(addSlideButton.GetParent(), grid.GetChildCount()-1);
        UpdateList();

        string filePath = "res://temp_cutscenes/intro_cutscene_config_test.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        CutsceneManager.Instance.ConvertCutsceneToJson(filePath);
    }
    
    private void OnAddSlidePressed()
    {
        chooseImageButton.Show();
        uploadImageButton.Show();
    }
    private void OnDeleteConfirmed()
    {
        deleteConfirmed = true;
        if (previewToDelete != null)
        {
            RemoveSlidePreview(previewToDelete);
            previewToDelete = null;
        }
    }

    private void OnDeleteCanceled()
    {
        if (previewToDelete != null && !deleteConfirmed)
        {
            PreviewRect currPreviewRect = previewToDelete.GetNode<PreviewRect>("TextureRect");
            currPreviewRect.SelfModulate = currPreviewRect.DefaultColor;
            currPreviewRect.deleteButton.Visible = false;
      
        }
    }
    
    public void PromptDeleteSlide(SlidePreview preview)
    {
        previewToDelete = preview;
        deleteConfirmDialog.PopupCentered();
    }

    public void RemoveSlidePreview(SlidePreview preview)
    {
        if (preview == null || preview.cutsceneImageResource == null)
        {
            GD.PrintErr("CutsceneEditor: Invalid preview or missing resource.");
            return;
        }

        var resource = preview.cutsceneImageResource;
        var cutsceneList = cutsceneManager.cutsceneImages;

        int removedCount = cutsceneList.RemoveAll(c => c.ImagePath == resource.ImagePath);
        if (removedCount == 0)
        {
            GD.PrintErr("CutsceneEditor: No matching resource removed.");
            return;
        }

        // Remove from grid container
        grid = GetNode<GridContainer>("ScrollContainer/GridContainer");
        grid.RemoveChild(preview);
        preview.QueueFree();

        // Re-index
        for (int i = 0; i < cutsceneList.Count; i++)
        {
            cutsceneList[i].Index = i;
        }

        // Update UI preview labels
        UpdateList();

        // Save updated JSON
        string filePath = "res://temp_cutscenes/intro_cutscene_config_test.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        CutsceneManager.Instance.ConvertCutsceneToJson(filePath);

        GD.Print("Slide preview removed and cutscene JSON updated.");
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
    
    public void OnSlideDropped(SlidePreview fromSlide, SlidePreview toSlide)
    {
        if (fromSlide == toSlide) return;

        int fromIndex = grid.GetChildren().IndexOf(fromSlide);
        int toIndex = grid.GetChildren().IndexOf(toSlide);
        if (fromSlide != null)
        {
            grid.RemoveChild(fromSlide);
            grid.AddChild(fromSlide);
            grid.MoveChild(fromSlide, toIndex);
        }
        else
        {
            GD.PrintErr("CutsceneEditor: No valid slide selected.");
        }

        GD.Print("curr grid count after drag and drop: " + grid.GetChildren().Count);

        // Reorder in the cutscene list
        var list = CutsceneManager.Instance.cutsceneImages;
        // Removing by comparing image path
        int removedCount = list.RemoveAll(c => c.ImagePath == fromSlide.cutsceneImageResource.ImagePath);
        if (removedCount == 0)
        {
            GD.PrintErr("CutsceneEditor: No matching resource removed.");
            return;
        }
        //RemoveSlidePreview(fromSlide);
        list.Insert(toIndex, fromSlide.cutsceneImageResource);

        // Update indices
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Index = i;
        }
        RefreshList();
        UpdateList();

        // Save changes
        string filePath = "res://temp_cutscenes/intro_cutscene_config_test.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        CutsceneManager.Instance.ConvertCutsceneToJson(filePath);

   
        GD.Print("Slide reordered and saved.");
    }

    
    private void ToggleEditor()
    {
        isVisible = !isVisible;
        Visible = isVisible;
        GetTree().Paused = isVisible;
        GD.Print(isVisible ? "Cutscene Editor Opened" : "Cutscene Editor Closed");
        
    }
}

