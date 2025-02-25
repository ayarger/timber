using System.Collections;
using Godot;

public class ImageAsset : Asset
{
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

        GD.Print($"Opening image preview: {FilePath}");

        // Create a full-screen popup
        WindowDialog popup = new WindowDialog
        {
            RectMinSize = new Vector2(600, 600),
            RectSize = new Vector2(600, 600),
            WindowTitle = $"{FileName} Image Preview"
        };

        TextureRect previewImage = new TextureRect
        {
            Texture = _thumbnail,
            Expand = true,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            RectSize = new Vector2(600, 600)
        };

        popup.AddChild(previewImage);
        GetTree().Root.AddChild(popup);
        popup.PopupCentered();
    }
}
