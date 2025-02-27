using System.Collections;
using Godot;

public class ImageAsset : Asset
{
    AssetSpawner assetSpawner;

    public ImageAsset(string filePath) 
        : base(filePath, "res://icons/image_icon.png") { } // Default placeholder icon

    public override IEnumerator LoadAsset()
    {
        GD.Print($"Loading image from AWS S3: {FilePath}");

        ArborResource.Load<Texture>(FilePath);
        yield return ArborResource.WaitFor(FilePath);

        _thumbnail = ArborResource.Get<Texture>(FilePath);
        if (_thumbnail == null)
        {
            GD.PrintErr($"Failed to load image: {FilePath}, using default image.");
            _thumbnail = _defaultPreviewTexture;
        }
    }
public override void OnButtonPressed()
{
    if (_thumbnail == null)
    {
        GD.PrintErr($"Cannot preview image, missing asset: {FilePath}");
        return;
    }

    GD.Print($"Updating ImagePreview texture: {FilePath}");

    TextureRect previewNode = GetNode<TextureRect>("EditorTabSystem/TabContainer/Sprites/ImagePreview");

    if (previewNode != null)
    {
        previewNode.Texture = _thumbnail;
        previewNode.Visible = true;
    }
    else
    {
        GD.PrintErr("ImagePreview node not found in the scene.");
    }
}



}
