using Godot;
using System;

public partial class SlidePreview : Control
{
    private TextureRect previewImage;
    private LineEdit imagePathInput;
    private SpinBox orderInput;
    private OptionButton transitionDropdown;
    private OptionButton displayDropdown;

    public override void _Ready()
    {
        // Get references to child nodes
        previewImage = GetNode<TextureRect>("TextureRect");
        imagePathInput = GetNode<LineEdit>("ImagePath");
        orderInput = GetNode<SpinBox>("Order");
        transitionDropdown = GetNode<OptionButton>("TransitionDropdown");
        displayDropdown = GetNode<OptionButton>("DisplayDropdown");
    }

    public void SetPreview(CutsceneImageResource sceneData)
    {
        imagePathInput.Text = sceneData.ImagePath;
        orderInput.Value = sceneData.Index + 1; // index + 1 
        transitionDropdown.Text =  sceneData.TransitionStyle;
        displayDropdown.Text =sceneData.DisplayStyle;
        //use resource
        ArborResource.UseResource <Texture>(sceneData.ImagePath, texture =>
            {
                if (texture != null)
                {
                    previewImage.Texture = texture;
                }
                else
                {
                    GD.PrintErr("Failed to load texture.");
                }
            }, this); 
     
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
}