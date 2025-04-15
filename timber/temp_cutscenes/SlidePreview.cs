using Godot;
using System;

public partial class SlidePreview : Control
{
    [Export] public TextureRect previewImage;
    [Export] public LineEdit imagePathInput;
    [Export] public OptionButton transitionDropdown;
    [Export] public OptionButton displayDropdown;
    [Export] public RichTextLabel orderLabel;
    [Export] public TextureRect popupPreview;
    [Export] public RichTextLabel popupOrderLbel;
    [Export] public CutsceneImageResource cutsceneImageResource;
    [Export] public WindowDialog PopupDialog;
    [Export] public Button saveButton;
    [Export] public Button cancelButton;
    [Signal] public delegate void CutsceneUpdated();
  
    private Tween tween;
    
    private string[] transitionStyles = { "fade_up", "bounce", "instant" };
    private string[] displayStyles = { "standard", "shake_small", "shake_large", "sin_vertical", "vibrate" };
    
    public CutsceneImageResource originalSceneData; // Stores the original scene data
    public CutsceneImageResource tempSceneData; // Stores temporary changes


    public override void _Ready()
    {
        // Get references to child nodes
        previewImage = GetNode<TextureRect>("TextureRect");
        //imagePathInput = GetNode<LineEdit>("ImagePath");
        //orderInput = GetNode<SpinBox>("Order");
        orderLabel = GetNode<RichTextLabel>("OrderLabel");
        PopupDialog = GetNode<WindowDialog>("WindowDialog");
        
        transitionDropdown = GetNode<OptionButton>("WindowDialog/TransitionVBox/TransitionDropdown");
        displayDropdown = GetNode<OptionButton>("WindowDialog/DisplayVBox/DisplayDropdown");
        popupPreview = GetNode<TextureRect>("WindowDialog/PopupPreview");
        popupOrderLbel = GetNode<RichTextLabel>("WindowDialog/PopupOrderLabel");
        
        saveButton = GetNode<Button>("WindowDialog/ButtonHBox/Save");
        cancelButton = GetNode<Button>("WindowDialog/ButtonHBox/Cancel");
        
        tween = new Tween();
        AddChild(tween);
        
        
        // Populate dropdowns
        PopulateDropdown(transitionDropdown, transitionStyles);
        PopulateDropdown(displayDropdown, displayStyles);
        
        // Connet signals for buttons
        saveButton.Connect("pressed", this, nameof(SaveChanges));
        cancelButton.Connect("pressed", this, nameof(CancelChanges));
      
        
        // Connect signals for when user selects a new option
        transitionDropdown.Connect("item_selected", this, nameof(OnTransitionSelected));
        displayDropdown.Connect("item_selected", this, nameof(OnDisplaySelected));
    }
    
    private void PopulateDropdown(OptionButton optionButton, string[] options)
    {
        optionButton.Clear();
        for (int i = 0; i < options.Length; i++)
        {
            optionButton.AddItem(options[i], i);
        }
    }

    public void SetPreview(CutsceneImageResource sceneData)
    {
        if (sceneData == null)
        {
            GD.PrintErr("SetPreview: sceneData is null!");
            return;
        }
        if (previewImage == null)
        {
            GD.PrintErr("SetPreview: previewImage is null!");
            return;
        }
        //imagePathInput.Text = sceneData.ImagePath;
        cutsceneImageResource = sceneData;
        tempSceneData = GenerateTempCutsceneImageResource(sceneData); 
        originalSceneData = sceneData; // Store the original scene data
        
        orderLabel.Text = (sceneData.Index + 1).ToString();
        popupOrderLbel.Text = (sceneData.Index + 1).ToString();
        transitionDropdown.Text =  sceneData.TransitionStyle;
        displayDropdown.Text =sceneData.DisplayStyle;
        //use resource
        ArborResource.UseResource <Texture>(sceneData.ImagePath, texture =>
            {
                if (texture != null)
                {
                    previewImage.Texture = texture;
                    popupPreview.Texture = texture;
                }
                else
                {
                    GD.PrintErr("Failed to load texture.");
                }
            }, this); 
     
    }

    public void UpdatePreview(CutsceneImageResource sceneData)
    {
        orderLabel.Text = (sceneData.Index + 1).ToString();
        popupOrderLbel.Text = (sceneData.Index + 1).ToString();
        transitionDropdown.Text =  sceneData.TransitionStyle;
        displayDropdown.Text =sceneData.DisplayStyle;
        previewImage.Texture = sceneData.Image;
        popupPreview.Texture = sceneData.Image;
    }
    
    private CutsceneImageResource GenerateTempCutsceneImageResource(CutsceneImageResource source)
    {
        return new CutsceneImageResource
        {
            ImagePath = source.ImagePath,
            TransitionStyle = source.TransitionStyle,
            DisplayStyle = source.DisplayStyle,
            Index = source.Index,
            Image = source.Image
        };
    }
    
    private int GetOptionIndex(OptionButton optionButton, string value)
    {
        for (int i = 0; i < optionButton.GetItemCount(); i++)
        {
            if (optionButton.GetItemText(i) == value)
                return i;
        }
        return 0; // Default to first item if not found
    }
    
    private void OnTransitionSelected(int index)
    {
        string selectedTransition = transitionDropdown.GetItemText(index);
        tempSceneData.TransitionStyle = selectedTransition;
        ApplyTransition(selectedTransition);
        GD.Print("tempTransitionStyle:" + tempSceneData.TransitionStyle);
    }

    private void OnDisplaySelected(int index)
    {
        string selectedDisplay = displayDropdown.GetItemText(index);
        tempSceneData.DisplayStyle = selectedDisplay;
        ApplyDisplayStyle(selectedDisplay);
        GD.Print ("tempDisplayStyle: " + tempSceneData.DisplayStyle);
    }
    
    public void SaveChanges()
    {
        if (originalSceneData == null)
        {
            GD.PrintErr("No original scene data found!");
            return;
        }
        cutsceneImageResource = tempSceneData;
        var cutsceneList = CutsceneManager.Instance.cutsceneImages;
        cutsceneList[tempSceneData.Index] = tempSceneData;
        originalSceneData = tempSceneData;
        string filePath = "res://temp_cutscenes/intro_cutscene_config_test.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        CutsceneManager.Instance.ConvertCutsceneToJson(filePath);
        CutsceneManager.Instance.LoadCutsceneImagesFromJsonS3(filePath);
        GD.Print("Changes saved!");
        PopupDialog.Hide();
        CutscenePlayer.Instance.UpdateCutscenePreview();
    }
    
    public void CancelChanges()
    {
        if (originalSceneData == null)
        {
         
            GD.PrintErr("No original scene data found!");
            return;
        }

        // Restore the original scene settings
        SetPreview(originalSceneData);

        GD.Print("Changes reverted!");
        PopupDialog.Hide();
        CutscenePlayer.Instance.UpdateCutscenePreview();
    }

    public void RemoveCurrSlide(int index)
    {
        var cutsceneList = CutsceneManager.Instance.cutsceneImages;
        for (int i = 0; i < cutsceneList.Count; i++)
        {
            if (cutsceneList[i].Index == originalSceneData.Index)
            {
                cutsceneList[i] = originalSceneData;
                break;
            }
        }
    }

    
    private void ApplyTransition(string transitionType)
    {
        tween.StopAll(); // Reset previous animation

        switch (transitionType)
        {
            case "fade_up":
                popupPreview.Modulate = new Color(1, 1, 1, 0);
                tween.InterpolateProperty(popupPreview, "modulate:a", 0, 1, 0.5f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
                break;

            case "bounce":
                //popupPreview.RectPivotOffset = popupPreview.RectSize / 2; 
                popupPreview.RectScale = new Vector2(0.5f, 0.5f);
                //popupPreview.RectPivotOffset = popupPreview.RectSize / 2;
                tween.InterpolateProperty(popupPreview, "rect_scale", new Vector2(0.5f, 0.5f), new Vector2(0.62f, 0.62f), 0.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
                break;

            case "instant":
                popupPreview.Modulate = new Color(1, 1, 1, 1);
                break;

            default:
                GD.PrintErr($"Unknown transition style: {transitionType}");
                break;
        }

        tween.Start();
    }
    
    private void ApplyDisplayStyle(string displayStyle)
    {
        tween.StopAll(); // Reset previous animation

        switch (displayStyle)
        {
            case "standard":
                //popupPreview.RectPosition = Vector2.Zero;
                break;

            case "shake_small":
                ShakeEffect(5);
                break;

            case "shake_large":
                ShakeEffect(20);
                break;

            case "sin_vertical":
                ApplySinVerticalEffect();
                break;

            case "vibrate":
                ApplyVibrateEffect();
                break;

            default:
                GD.PrintErr($"Unknown display style: {displayStyle}");
                break;
        }
    }
    
    private async void ShakeEffect(int intensity)
    {
        Vector2 originalPosition = popupPreview.RectPosition;
        for (int i = 0; i < 10; i++)
        {
            popupPreview.RectPosition = originalPosition + new Vector2(GD.Randf() * intensity - intensity / 2, GD.Randf() * intensity - intensity / 2);
            await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        }
        popupPreview.RectPosition = originalPosition;
    }

    private void ApplySinVerticalEffect()
    {
        tween.InterpolateProperty(popupPreview, "rect_position:y", popupPreview.RectPosition.y, popupPreview.RectPosition.y + 20, 0.5f, Tween.TransitionType.Sine, Tween.EaseType.InOut);
        tween.InterpolateProperty(popupPreview, "rect_position:y", popupPreview.RectPosition.y + 20, popupPreview.RectPosition.y, 0.5f, Tween.TransitionType.Sine, Tween.EaseType.InOut, 0.5f);
        tween.Start();
    }

    private void ApplyVibrateEffect()
    {
        tween.InterpolateProperty(popupPreview, "rect_position:x", popupPreview.RectPosition.x - 5, popupPreview.RectPosition.x + 5, 0.05f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.InterpolateProperty(popupPreview, "rect_position:x", popupPreview.RectPosition.x + 5, popupPreview.RectPosition.x - 5, 0.05f, Tween.TransitionType.Linear, Tween.EaseType.InOut, 0.05f);
        tween.Start();
    }

    public override bool CanDropData(Vector2 position, object data)
    {
        return data is SlidePreview;
    }
}