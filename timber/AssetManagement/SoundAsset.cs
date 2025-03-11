using System.Collections;
using Godot;

public class SoundAsset : Asset
{
    private AudioStream _audioStream;
    private AudioStreamPlayer _player;

    public SoundAsset(string filePath) 
        : base(filePath, "res://icons/audio_icon.png") { } // Default sound preview icon

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

    public override void OnButtonPressed()
    {
        if (_audioStream == null)
        {
            GD.PrintErr($"Cannot play sound, missing asset: {FilePath}");
            return;
        }

        GD.Print($"Playing sound: {FilePath}");

        ArborAudioManager.PreviewSFX(_audioStream);

    }

}
