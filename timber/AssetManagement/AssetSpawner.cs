using System.Collections;
using System.Collections.Generic;
using Godot;

public class AssetSpawner : Control
{
    private GridContainer _gridContainer;
    private List<string> _assetFiles = new List<string>();

    public override void _Ready()
    {
        GD.Print("AssetSpawner is starting!");

        _gridContainer = GetNode<GridContainer>("ScrollContainer/GridContainer");
        if (_gridContainer == null)
        {
            GD.PrintErr("ERROR: GridContainer not found!");
            return;
        }

        // Load asset file list from AWS S3
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

        // Load assets, will add more file types after testing
        _assetFiles = manifest.Search(".*\\.(png|wav)")
            .FindAll(file => !file.EndsWith(".import") && !file.EndsWith("images/gameover_background.png"));

        GD.Print($"Found {_assetFiles.Count} assets.");
        ArborCoroutine.StartCoroutine(SpawnAssets(), this);
    }

    private IEnumerator SpawnAssets()
    {
        GD.Print($"Assets found:");
        foreach (string filePath in _assetFiles)
        {
            GD.Print($"- {filePath}");
        }
        foreach (string filePath in _assetFiles)
        {
            GD.Print($"Creating {filePath} asset...");
            Asset asset = AssetFactory.CreateAsset(filePath);
            yield return asset.LoadAsset();
            Control previewButton = asset.CreatePreviewButton();
            _gridContainer.AddChild(previewButton);
        }
    }
}
