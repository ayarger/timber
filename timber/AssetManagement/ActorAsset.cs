using System.Collections;
using Godot;

public class ActorAsset : Asset
{
    private ActorConfig _actorConfig;
    private PackedScene _actorScene;

    public ActorAsset(string filePath) 
        : base(filePath, "res://icons/actor_icon.png") { } // Default placeholder icon

    public override IEnumerator LoadAsset()
    {
        GD.Print($"Loading actor config from AWS S3: {FilePath}");

        ArborResource.Load<ActorConfig>(FilePath);
        yield return ArborResource.WaitFor(FilePath);

        _actorConfig = ArborResource.Get<ActorConfig>(FilePath);
        if (_actorConfig == null)
        {
            GD.PrintErr($"Skipping ActorAsset due to missing config: {FilePath}");
            yield break;
        }

        // // Load actor model (scene)
        // string actorScenePath = $"actors/{_actorConfig.name}.tscn";
        // ArborResource.Load<PackedScene>(actorScenePath);
        // yield return ArborResource.WaitFor(actorScenePath);
        // _actorScene = ArborResource.Get<PackedScene>(actorScenePath);

        // Load actor preview image
        string imagePath = $"images/{_actorConfig.name}.png";
        ArborResource.Load<Texture>(imagePath);
        yield return ArborResource.WaitFor(imagePath);
        _thumbnail = ArborResource.Get<Texture>(imagePath) ?? _defaultPreviewTexture;
    }

    public override void OnButtonPressed()
    {
        if (_actorScene == null)
        {
            GD.PrintErr($"Cannot load actor, missing file: {FilePath}");
            return;
        }

        GD.Print($"Spawning actor: {_actorConfig.name}");

        // Node actorInstance = _actorScene.Instance();
        // GetTree().Root.AddChild(actorInstance);
    }
}
