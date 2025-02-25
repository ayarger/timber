using System.Collections;
using Godot;

public class SoundAsset : Asset
{
    private AudioStream _audioStream;

    public SoundAsset(string filePath) 
        : base(filePath, "res://icons/audio_icon.png") { } 
        // default sound preview icon

    public override IEnumerator LoadAsset()
    {
        GD.Print($"Loading sound from AWS S3: {FilePath}");

        ArborResource.Load<AudioStream>(FilePath);
        yield return ArborResource.WaitFor(FilePath);

        _audioStream = ArborResource.Get<AudioStream>(FilePath);
        if (_audioStream == null)
        {
            GD.PrintErr($"Failed to load sound: {FilePath}");
        }
    }

    public void PlaySound()
    {
        if (_audioStream != null)
        {
            AudioStreamPlayer player = new AudioStreamPlayer();
            player.Stream = _audioStream;
            AddChild(player);
            player.Play();
        }
    }
}
