using System.Collections;
using Godot;

public class ImageAsset : Asset
{
    public ImageAsset(string filePath) 
        : base(filePath, "res://icons/image_icon.png") { } // Default icon (not needed, but consistent)

    public override IEnumerator LoadAsset()
    {
        GD.Print($"Loading image from AWS S3: {FilePath}");

        ArborResource.Load<Texture>(FilePath);

        GD.Print($"Texture loaded from {FilePath}");

        yield return ArborResource.WaitFor(FilePath);

        GD.Print($"Finished wait for from {FilePath}");
        
        _thumbnail = ArborResource.Get<Texture>(FilePath);

        GD.Print($"Get Texture succeeded from {FilePath}");

        if (_thumbnail == null)
        {
            GD.PrintErr($"Failed to load image: {FilePath}");
        }
    }
}
