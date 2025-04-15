using Godot;
using System;
using System.Collections;

public class TestAssetPicker : Node2D
{
    private Button _pickButton;

    public override void _Ready()
    {
        _pickButton = GetNode<Button>("PickAssetButton");
        _pickButton.Connect("pressed", this, nameof(OnPickAssetPressed));
    }

    private async void OnPickAssetPressed()
    {
        Asset result = await ArborResource.PickAsync(AssetType.Actor);
        if (result != null)
            GD.Print("You picked: " + result.FilePath);
        else
            GD.Print("No file picked.");

    }


}
