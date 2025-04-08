using Godot;
using System;

public class TestAssetPicker : Node2D
{
    private Button _pickButton;
    private AssetPickerPopup _pickerPopup;

    public override void _Ready()
    {
        _pickButton = GetNode<Button>("PickAssetButton");
        _pickerPopup = GetNode<AssetPickerPopup>("AssetPickerPopup");

        _pickButton.Connect("pressed", this, nameof(OnPickAssetPressed));
        //_pickerPopup.Connect("AssetSelected", this, nameof(OnAssetSelected));
    }

    private void OnPickAssetPressed()
    {
        _pickerPopup.Open(OnAssetSelected);
    }

    private void OnAssetSelected(string filePath)
    {
        GD.Print($"Picked asset: {filePath}");
        // Replace later with actual stuff
    }
}
