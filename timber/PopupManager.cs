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
/// Displays an actor preview with editable fields (for ActorAsset)
/// </summary>
public static void ShowActor(Texture texture, ActorConfig actorConfig)
{
    if (_instance == null)
    {
        GD.PrintErr("PopupManager has not been initialized.");
        return;
    }

    if (_instance._actorPopup == null || _instance._actorImage == null)
    {
        GD.PrintErr("PopupManager: Cannot display actor. Missing nodes.");
        return;
    }

    GD.Print($"Displaying actor preview: {actorConfig.name}");

    _instance._actorImage.Texture = texture;

    // Get the container that will hold the editable fields
    VBoxContainer actorDetailsContainer = _instance._actorPopup.GetNode<VBoxContainer>("HBoxContainer/ActorDetails");

    // Clear previous content
    foreach (Node child in actorDetailsContainer.GetChildren())
    {
        child.QueueFree();
    }

    // Add editable fields for basic actor info
    AddEditableField(actorDetailsContainer, "Name", actorConfig.name);
    AddEditableField(actorDetailsContainer, "Team", actorConfig.team);
    AddEditableField(actorDetailsContainer, "Map Code", actorConfig.map_code.ToString());
    AddEditableField(actorDetailsContainer, "Scale Factor", actorConfig.aesthetic_scale_factor.ToString());

    // Add a label for "States"
    Label statesLabel = new Label { Text = "States:" };
    actorDetailsContainer.AddChild(statesLabel);

    // Create editable fields for each state and its stats
    foreach (var state in actorConfig.stateConfigs)
    {
        Label stateLabel = new Label { Text = $"- {state.name}" };
        actorDetailsContainer.AddChild(stateLabel);

        if (state.stateStats != null)
        {
            foreach (var stat in state.stateStats)
            {
                AddEditableField(actorDetailsContainer, $"  {stat.Key}", stat.Value.ToString(), isNumeric: true);
            }
        }
    }

    // Show the popup
    _instance._actorPopup.PopupCentered();
}

/// <summary>
/// Helper method to add an editable field (LineEdit for text, SpinBox for numbers)
/// </summary>
private static void AddEditableField(VBoxContainer container, string label, string value, bool isNumeric = false)
{
    HBoxContainer fieldContainer = new HBoxContainer();

    Label fieldLabel = new Label { Text = label, SizeFlagsHorizontal = (int)Control.SizeFlags.Expand };
    fieldContainer.AddChild(fieldLabel);

    if (isNumeric)
    {
        SpinBox numericInput = new SpinBox
        {
            Value = float.TryParse(value, out float result) ? result : 0,
            MinValue = 0,
            MaxValue = 100, // Adjust range as needed
            Step = 0.1f
        };
        fieldContainer.AddChild(numericInput);
    }
    else
    {
        LineEdit textInput = new LineEdit { Text = value };
        fieldContainer.AddChild(textInput);
    }

    container.AddChild(fieldContainer);
}

}
