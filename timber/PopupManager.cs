using Godot;
using System;

public class PopupManager : Node
{
    private static PopupManager _instance;
    private WindowDialog _popup;
    private TextureRect _previewImage;

    public override void _Ready()
    {
        if (_instance != null)
        {
            QueueFree(); // Prevent multiple instances
            return;
        }

        _instance = this;

        GD.Print("PopupManager initialized.");

        // Ensure there is a preview window in the scene
        _popup = GetTree().Root.GetNodeOrNull<WindowDialog>("EditorTabSystem/PreviewWindow");
        _previewImage = GetTree().Root.GetNodeOrNull<TextureRect>("EditorTabSystem/PreviewWindow/PreviewImage");

        if (_popup == null || _previewImage == null)
        {
            GD.PrintErr("PopupManager: Missing PreviewWindow or PreviewImage in scene.");
        }
    }

    public static void ShowImage(Texture texture)
    {
        if (_instance == null)
        {
            GD.PrintErr("PopupManager has not been initialized.");
            return;
        }

        if (_instance._popup == null || _instance._previewImage == null)
        {
            GD.PrintErr("PopupManager: Cannot display image. Missing nodes.");
            return;
        }

        GD.Print("Displaying image in popup.");
        _instance._previewImage.Texture = texture;
        _instance._popup.PopupCentered();
    }
}
