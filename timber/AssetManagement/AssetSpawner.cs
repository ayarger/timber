using System.Collections;
using System.Collections.Generic;
using Godot;

public class AssetSpawner : Control
{
    private GridContainer _gridContainer;
    private List<string> _assetFiles = new List<string>();
    private Timer _initialTimer;
    private Timer _loadTimer;
    private int _currentAssetIndex = 0;

    public override void _Ready()
    {
        GD.Print("AssetSpawner is starting!");

        _gridContainer = GetNode<GridContainer>("ScrollContainer/GridContainer");
        if (_gridContainer == null)
        {
            GD.PrintErr("ERROR: GridContainer not found!");
            return;
        }
        
        // add delay before starting to load assets
        _initialTimer = new Timer();
        _initialTimer.WaitTime = 1.0f;
        _initialTimer.OneShot = true;
        _initialTimer.Connect("timeout", this, nameof(StartLoadingAssets));
        AddChild(_initialTimer);
        _initialTimer.Start();

        // asset loading timer
        _loadTimer = new Timer();
        _loadTimer.WaitTime = 0.01f; // load 1 asset every 0.01s
        _loadTimer.OneShot = false;
        _loadTimer.Connect("timeout", this, nameof(OnLoadNextAsset));
        AddChild(_loadTimer);
    }

    private void StartLoadingAssets()
    {
        GD.Print("Initial delay finished, starting asset loading...");
        ArborCoroutine.StartCoroutine(LoadAssetManifest(), this);
    }

    private IEnumerator LoadAssetManifest()
    {
        GD.Print("Fetching asset manifest...");

        ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        yield return ArborResource.WaitFor("mod_file_manifest.json");

        ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");
        if (manifest == null)
        {
            GD.PrintErr("Failed to load asset manifest!");
            yield break;
        }

        _assetFiles = manifest.Search(".*\\.(png|wav|actor)")
            .FindAll(file => !file.EndsWith(".import") && !file.EndsWith("images/gameover_background.png"));

        GD.Print($"Found {_assetFiles.Count} assets.");

        _currentAssetIndex = 0;
        _loadTimer.Start();
    }

    private void OnLoadNextAsset()
    {
        if (_currentAssetIndex >= _assetFiles.Count)
        {
            GD.Print("All assets loaded!");
            _loadTimer.Stop();
            return;
        }

        string filePath = _assetFiles[_currentAssetIndex];
        GD.Print($"Loading asset: {filePath}");

        Asset asset = AssetFactory.CreateAsset(filePath);
        if (asset == null)
        {
            GD.PrintErr($"Failed to create asset for {filePath}");
            _currentAssetIndex++;
            return;
        }

        ArborCoroutine.StartCoroutine(LoadAndDisplayAsset(asset), this);
        _currentAssetIndex++;
    }

    private IEnumerator LoadAndDisplayAsset(Asset asset)
    {
        yield return asset.LoadAsset();

        Control previewButton = asset.CreatePreviewButton();
        _gridContainer.AddChild(previewButton);

        GD.Print($"Added {asset.FilePath} to UI.");
    }
}
