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
    private ActorConfig _currentActorConfig;

    public override void _Ready()
    {
        if (_instance != null)
        {
            QueueFree(); // Prevent multiple instances
            return;
        }

        _instance = this;
        GD.Print("PopupManager initialized.");

        _imagePopup = GetTree().Root.GetNodeOrNull<WindowDialog>("EditorTabSystem/PreviewWindow");
        _imagePreview = GetTree().Root.GetNodeOrNull<TextureRect>("EditorTabSystem/PreviewWindow/PreviewImage");

        if (_imagePopup == null || _imagePreview == null)
        {
            GD.PrintErr("PopupManager: Missing Image PreviewWindow elements.");
        }

        _actorPopup = GetTree().Root.GetNodeOrNull<WindowDialog>("EditorTabSystem/ActorPreviewWindow");
        _actorImage = GetTree().Root.GetNodeOrNull<TextureRect>("EditorTabSystem/ActorPreviewWindow/HBoxContainer/ActorImage/PreviewImage");

        if (_actorPopup == null || _actorImage == null)
        {
            GD.PrintErr("PopupManager: Missing ActorPreviewWindow elements.");
        }
    }

    public static void ShowImage(Texture texture)
    {
        if (_instance == null || _instance._imagePopup == null || _instance._imagePreview == null)
        {
            GD.PrintErr("PopupManager: Cannot display image.");
            return;
        }

        GD.Print("Displaying image preview.");
        _instance._imagePreview.Texture = texture;
        _instance._imagePopup.PopupCentered();
    }

    public static void ShowActor(Texture texture, ActorConfig actorConfig)
    {
        if (_instance == null || _instance._actorPopup == null || _instance._actorImage == null)
        {
            GD.PrintErr("PopupManager: Cannot display actor.");
            return;
        }

        GD.Print($"Displaying actor preview: {actorConfig.name}");

        _instance._actorImage.Texture = texture;
        _instance._currentActorConfig = actorConfig;

        ScrollContainer scrollContainer = _instance._actorPopup.GetNodeOrNull<ScrollContainer>("HBoxContainer/ActorDetailsScroll");
        VBoxContainer actorDetailsContainer = scrollContainer?.GetNodeOrNull<VBoxContainer>("ActorDetails");

        if (scrollContainer == null || actorDetailsContainer == null)
        {
            GD.PrintErr("PopupManager: 'ActorDetailsScroll' or 'ActorDetails' node not found.");
            return;
        }

        foreach (Node child in actorDetailsContainer.GetChildren())
        {
            child.QueueFree();
        }

        AddEditableField(actorDetailsContainer, "Name", actorConfig.name, value => { actorConfig.name = value; SaveActor(actorConfig); });
        AddEditableField(actorDetailsContainer, "Team", actorConfig.team, value => { actorConfig.team = value; SaveActor(actorConfig); });
        AddEditableField(actorDetailsContainer, "Map Code", actorConfig.map_code.ToString(), value => { actorConfig.map_code = value[0]; SaveActor(actorConfig); });
        AddEditableField(actorDetailsContainer, "Scale Factor", actorConfig.aesthetic_scale_factor.ToString(), value => {
            if (float.TryParse(value, out float result)) actorConfig.aesthetic_scale_factor = result;
            SaveActor(actorConfig);
        }, true); // should autosave

        Label statesLabel = new Label { Text = "States:" };
        actorDetailsContainer.AddChild(statesLabel);

        foreach (var state in actorConfig.stateConfigs)
        {
            Label stateLabel = new Label { Text = $"- {state.name}" };
            actorDetailsContainer.AddChild(stateLabel);

            if (state.stateStats != null)
            {
                foreach (var stat in state.stateStats)
                {
                    AddEditableField(actorDetailsContainer, $"      {stat.Key}", stat.Value.ToString(), value => {
                        if (float.TryParse(value, out float result))
                            state.stateStats[stat.Key] = result;
                        SaveActor(actorConfig);
                    }, true);
                }
            }
        }

        scrollContainer.RectMinSize = new Vector2(200, 400);
        _instance._actorPopup.PopupCentered();
    }

    private static void AddEditableField(VBoxContainer container, string label, string value, Action<string> onValueChanged = null, bool isNumeric = false)
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
                MaxValue = 100,
                Step = 0.1f
            };
            if (onValueChanged != null)
                numericInput.Connect("value_changed", _instance, nameof(OnNumericFieldChanged));
            numericInput.Connect("value_changed", _instance, nameof(OnNumericFieldChanged));
            numericInput.SetMeta("callback", onValueChanged);
            fieldContainer.AddChild(numericInput);
        }
        else
        {
            LineEdit textInput = new LineEdit { Text = value };
            if (onValueChanged != null)
                textInput.Connect("text_changed", _instance, nameof(OnTextFieldChanged));
            textInput.SetMeta("callback", onValueChanged);
            fieldContainer.AddChild(textInput);
        }

        container.AddChild(fieldContainer);
    }

    private static void OnTextFieldChanged(string newValue, Node control)
    {
        if (control.HasMeta("callback"))
        {
            var callback = control.GetMeta("callback") as Action<string>;
            callback?.Invoke(newValue);
        }
    }

    // function not found? maybe something with the callback
    private static void OnNumericFieldChanged(float newValue, Node control)
    {
        if (control.HasMeta("callback"))
        {
            var callback = control.GetMeta("callback") as Action<string>;
            callback?.Invoke(newValue.ToString());
        }
    }

    private static void SaveActor(ActorConfig actorConfig)
    {
        if (string.IsNullOrEmpty(actorConfig.__filePath))
        {
            GD.PrintErr("PopupManager: ActorConfig missing __filePath for saving.");
            return;
        }

        ArborResource.WriteObject<ActorConfig>(actorConfig.__filePath, actorConfig);
        GD.Print("Auto-saved: " + actorConfig.__filePath);
    }
}
