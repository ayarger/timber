using Godot;
using System;
using System.Text;

public class PopupManager : Node
{
    private static PopupManager _instance;

    // Image Preview Elements
    private WindowDialog _imagePopup;
    private TextureRect _imagePreview;

    // Actor Preview Elements
    private WindowDialog _actorPopup;
    private TextureRect _actorImage;
    private RichTextLabel _actorTextBox;

    public override void _Ready()
    {
        if (_instance != null)
        {
            QueueFree(); // Prevent multiple instances
            return;
        }

        _instance = this;
        GD.Print("PopupManager initialized.");

        // Find the image preview window and elements
        _imagePopup = GetTree().Root.GetNodeOrNull<WindowDialog>("EditorTabSystem/PreviewWindow");
        _imagePreview = GetTree().Root.GetNodeOrNull<TextureRect>("EditorTabSystem/PreviewWindow/PreviewImage");

        if (_imagePopup == null || _imagePreview == null)
        {
            GD.PrintErr("PopupManager: Missing Image PreviewWindow elements.");
        }

        // Find the actor preview window and elements
        _actorPopup = GetTree().Root.GetNodeOrNull<WindowDialog>("EditorTabSystem/ActorPreviewWindow");
        _actorImage = GetTree().Root.GetNodeOrNull<TextureRect>("EditorTabSystem/ActorPreviewWindow/HBoxContainer/ActorImage/PreviewImage");
        _actorTextBox = GetTree().Root.GetNodeOrNull<RichTextLabel>("EditorTabSystem/ActorPreviewWindow/HBoxContainer/ActorDetails");

        if (_actorPopup == null || _actorImage == null || _actorTextBox == null)
        {
            GD.PrintErr("PopupManager: Missing ActorPreviewWindow elements.");
        }
    }

    /// <summary>
    /// Displays a simple image preview (for ImageAsset)
    /// </summary>
    public static void ShowImage(Texture texture)
    {
        if (_instance == null)
        {
            GD.PrintErr("PopupManager has not been initialized.");
            return;
        }

        if (_instance._imagePopup == null || _instance._imagePreview == null)
        {
            GD.PrintErr("PopupManager: Cannot display image. Missing nodes.");
            return;
        }

        GD.Print("Displaying image preview.");

        _instance._imagePreview.Texture = texture;
        _instance._imagePopup.PopupCentered();
    }

    /// <summary>
    /// Displays an actor preview (for ActorAsset)
    /// </summary>
    public static void ShowActor(Texture texture, ActorConfig actorConfig)
    {
        if (_instance == null)
        {
            GD.PrintErr("PopupManager has not been initialized.");
            return;
        }

        if (_instance._actorPopup == null || _instance._actorImage == null || _instance._actorTextBox == null)
        {
            GD.PrintErr("PopupManager: Cannot display actor. Missing nodes.");
            return;
        }

        GD.Print($"Displaying actor preview: {actorConfig.name}");

        _instance._actorImage.Texture = texture;

        // Generate text description for the actor
        StringBuilder actorDetails = new StringBuilder();
        actorDetails.AppendLine($"\nName: {actorConfig.name}");
        actorDetails.AppendLine($"Team: {actorConfig.team}");
        actorDetails.AppendLine($"Map Code: {actorConfig.map_code}");
        actorDetails.AppendLine($"Scale Factor: {actorConfig.aesthetic_scale_factor}");
        actorDetails.AppendLine("\nStates:");

        foreach (var state in actorConfig.stateConfigs)
        {
            actorDetails.AppendLine($"- {state.name}");
            if (state.stateStats != null)
            {
                foreach (var stat in state.stateStats)
                {
                    actorDetails.AppendLine($"   - {stat.Key}: {stat.Value}");
                }
            }
        }

        // Apply text to RichTextLabel
        _instance._actorTextBox.Text = actorDetails.ToString();

        // Show the popup
        _instance._actorPopup.PopupCentered();
    }
}
