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

        // ArborResource.UseResource(
		// 	"sounds/vocal_gameover.wav", 
		// 	(AudioStream audio) => {
		// 		GD.Print("playing title music.");
		// 		ArborAudioManager.RequestSFX(_audioStream);
		// 	},
		// 	this
			
		// );

        // // add audio player if none
        // if (_player == null)
        // {
        //     GD.Print("Adding new audio player.");
        //     _player = new AudioStreamPlayer();
        //     GetTree().Root.AddChild(_player);
        // }

        // _player.Stream = _audioStream;

        // // stop current audio before restarting
        // if (_player.Playing)
        // {
        //     _player.Stop();
        // }

        // _player.Play();
    }

}
