using System.Collections;
using Godot;

public class ActorAsset : Asset
{
    private ActorConfig _actorConfig;

    // Parameterless constructor required by Godot's Mono bridge
    public ActorAsset() : base("", "res://icons/actor_icon.png") { }

    public ActorAsset(string filePath) 
        : base(filePath, "res://icons/actor_icon.png") { } // Default placeholder icon

    public override IEnumerator LoadAsset()
    {
        GD.Print($"Loading actor config from AWS S3: {FilePath}");

        // Load the .actor file
        ArborResource.Load<ActorConfig>(FilePath);
        yield return ArborResource.WaitFor(FilePath);

        _actorConfig = ArborResource.Get<ActorConfig>(FilePath);
        if (_actorConfig == null)
        {
            GD.PrintErr($"Skipping ActorAsset due to missing config: {FilePath}");
            yield break;
        }

        _actorConfig.__filePath = FilePath;
        GD.Print($"Loaded actor: {_actorConfig.name}");

        // Load actor preview image (Idle sprite)
        string imagePath = $"images/{_actorConfig.idle_sprite_filename}";
        ArborResource.Load<Texture>(imagePath);
        yield return ArborResource.WaitFor(imagePath);

        _thumbnail = ArborResource.Get<Texture>(imagePath) ?? _defaultPreviewTexture;
    }

    public override Button CreatePreviewButton()
    {
        Button button = base.CreatePreviewButton();

        TextureRect badge = new TextureRect
        {
            Texture = (Texture)GD.Load("res://icons/actor_badge_icon.png"),
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            RectMinSize = new Vector2(16, 16),
            RectScale = new Vector2(0.5f, 0.5f)
        };

        badge.SetAnchorsAndMarginsPreset(Control.LayoutPreset.TopLeft);
        badge.MarginRight = 4;
        badge.MarginTop = 4;
        button.AddChild(badge);

        return button;
    }

    public override void OnButtonPressed()
    {
        if (_thumbnail == null)
        {
            GD.PrintErr($"Cannot preview actor, missing preview image: {FilePath}");
            return;
        }

        GD.Print($"Showing actor preview: {_actorConfig.name}");

        PopupManager.ShowActor(_thumbnail, _actorConfig);
    }
}