using System.Collections;
using Godot;

public class SoundAsset : Asset
{
    private AudioStream _audioStream;
    private bool _isLoading = false;
    private TextureRect _loadingOverlay;

    public SoundAsset(string filePath) 
        : base(filePath, "res://icons/audio_icon.png") // Default sound preview icon
    {
    }

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
            if (!_isLoading)
            {
                GD.Print($"Fetching sound file from: {FilePath}");
                _isLoading = true;

                // Show the loading overlay
                ShowLoadingOverlay();

                // Start loading the sound
                ArborCoroutine.StartCoroutine(LoadSound(), this);
            }
            else
            {
                GD.Print($"Sound is still loading: {FilePath}");
            }
            return;
        }

        GD.Print($"Playing sound: {FilePath}");
        ArborAudioManager.PreviewSFX(_audioStream);
    }

    private IEnumerator LoadSound()
    {
        ArborResource.Load<AudioStream>(FilePath);
        yield return ArborResource.WaitFor(FilePath);

        _audioStream = ArborResource.Get<AudioStream>(FilePath);
        _isLoading = false;

        if (_audioStream == null)
        {
            GD.PrintErr($"Failed to load sound: {FilePath}");
            HideLoadingOverlay(); // Remove loading overlay even on failure
            yield break;
        }

        GD.Print($"Sound loaded successfully: {FilePath}");

        // Remove the loading overlay and play the sound
        HideLoadingOverlay();
        ArborAudioManager.PreviewSFX(_audioStream);
    }

    private void ShowLoadingOverlay()
    {
        if (_loadingOverlay == null)
        {
            _loadingOverlay = new TextureRect
            {
                Texture = (Texture)ResourceLoader.Load("res://icons/loading.png"), // Replace with actual path
                Expand = true,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Modulate = new Color(1, 1, 1, 0.8f) // Slight transparency
            };
        }

        GetParent().AddChild(_loadingOverlay);
    }

    private void HideLoadingOverlay()
    {
        _loadingOverlay?.QueueFree();
    }
}
