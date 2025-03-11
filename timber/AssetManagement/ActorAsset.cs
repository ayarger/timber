using System.Collections;
using Godot;

public class ActorAsset : Asset
{
    private ActorConfig _actorConfig;

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

        GD.Print($"Loaded actor: {_actorConfig.name}");

        // Load actor preview image (Idle sprite)
        string imagePath = $"images/{_actorConfig.idle_sprite_filename}";
        ArborResource.Load<Texture>(imagePath);
        yield return ArborResource.WaitFor(imagePath);

        _thumbnail = ArborResource.Get<Texture>(imagePath) ?? _defaultPreviewTexture;
    }

    public override void OnButtonPressed()
    {
        if (_thumbnail == null)
        {
            GD.PrintErr($"Cannot preview actor, missing preview image: {FilePath}");
            return;
        }

        GD.Print($"Showing actor preview: {_actorConfig.name}");

        PopupManager.ShowImage(_thumbnail);
    }
}
