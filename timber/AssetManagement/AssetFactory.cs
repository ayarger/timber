using System;

public static class AssetFactory
{
    public static Asset CreateAsset(string filePath)
    {
        if (filePath.EndsWith(".png") || filePath.EndsWith(".jpg"))
            return new ImageAsset(filePath);

        if (filePath.EndsWith(".wav") || filePath.EndsWith(".mp3"))
            return new SoundAsset(filePath);

        // if (filePath.StartsWith("actor_definitions/"))
        //     return new ActorAsset(filePath);

        throw new Exception("Unsupported asset type: " + filePath);
    }
}
