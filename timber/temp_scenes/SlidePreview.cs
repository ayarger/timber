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
        orderInput.Value = sceneData.Index;
        previewImage.Texture = sceneData.Image;
        transitionDropdown.Text =  sceneData.TransitionStyle;
        displayDropdown.Text =sceneData.DisplayStyle;
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