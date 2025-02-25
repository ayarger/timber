using System.Collections;
using System.Collections.Generic;
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

        // Load the actor config
        ArborResource.Load<ActorConfig>(FilePath);
        yield return ArborResource.WaitFor(FilePath);

        _actorConfig = ArborResource.Get<ActorConfig>(FilePath);

        if (_actorConfig == null)
        {
            GD.PrintErr($"Failed to load ActorConfig: {FilePath}");
            yield break;
        }

        // Set team and stats
        if (_actorConfig.team == "player")
        {
            _actorConfig.statConfig = LoadPlayerStats();
        }
        else
        {
            _actorConfig.statConfig = LoadEnemyStats();
        }

        // Assign actor preview image
        string imagePath = $"images/{_actorConfig.name}.png";
        ArborResource.Load<Texture>(imagePath);
        yield return ArborResource.WaitFor(imagePath);
        _thumbnail = ArborResource.Get<Texture>(imagePath) ?? _defaultPreviewTexture;
    }

    public void SpawnActor()
    {
        if (_actorScene != null)
        {
            Node actorInstance = _actorScene.Instance();
            GetTree().Root.AddChild(actorInstance);
        }
    }

    private StatConfig LoadPlayerStats()
    {
        StatConfig config = new StatConfig();
        config.stats["health"] = 100;
        return config;
    }

    private StatConfig LoadEnemyStats()
    {
        StatConfig config = new StatConfig();
        config.stats["health"] = 50;
        return config;
    }
}
