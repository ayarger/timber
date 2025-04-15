using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class AssetPickerPopup : WindowDialog
{
    [Signal]
    public delegate void AssetSelected(string filePath);

    private OptionButton _filterDropdown;
    private GridContainer _assetListContainer;
    private string _currentFilter = "All";
    private List<ModFile> _allAssets = new List<ModFile>();
    private Action<Asset> _onAssetSelectedCallback;
    private Timer _loadDelayTimer;

    public override void _Ready()
    {
        InstantiatePopup();
        //PickAsset(AssetType.Actor);
        
    }

    private void InstantiatePopup()
    {
        _filterDropdown = GetNode<OptionButton>("VBoxContainer/FilterDropdown");
        _assetListContainer = GetNode<ScrollContainer>("VBoxContainer/ScrollContainer")
                                .GetNode<GridContainer>("AssetListContainer");

        _filterDropdown.AddItem("All");
        _filterDropdown.AddItem("Image");
        _filterDropdown.AddItem("Sound");
        _filterDropdown.AddItem("Actor");
        _filterDropdown.AddItem("Script");

        _filterDropdown.Connect("item_selected", this, nameof(OnFilterChanged));

        // Add a small delay before loading assets
        _loadDelayTimer = new Timer();
        _loadDelayTimer.WaitTime = 0.5f; // Half a second delay
        _loadDelayTimer.OneShot = true;
        _loadDelayTimer.Connect("timeout", this, nameof(OnStartLoadAssets));
        AddChild(_loadDelayTimer);
    }

    private void OnStartLoadAssets()
    {
        ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        ArborCoroutine.StartCoroutine(LoadAssets(), this);
    }

    private void OnFilterChanged(int index)
    {
        _currentFilter = _filterDropdown.GetItemText(index);
        RefreshAssetList();
    }

    public void SetFilter(string filter)
    {
        for (int i = 0; i < _filterDropdown.GetItemCount(); i++)
        {
            if (_filterDropdown.GetItemText(i) == filter)
            {
                _filterDropdown.Select(i);
                _currentFilter = filter;
                RefreshAssetList();
                return;
            }
        }

        GD.PrintErr($"AssetPickerPopup: Filter '{filter}' not found in dropdown.");
    }


    private IEnumerator LoadAssets()
    {
        yield return ArborResource.WaitFor("mod_file_manifest.json");

        ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");
        _allAssets = manifest.Search(".*\\.(png|wav|actor|lua)")
            .FindAll(file => !file.name.EndsWith(".import") && !file.name.EndsWith("images/gameover_background.png"));

        RefreshAssetList();
    }

    private void RefreshAssetList()
    {
        foreach (Node child in _assetListContainer.GetChildren())
            child.QueueFree();

        foreach (ModFile modFile in _allAssets)
        {
            if (!IsMatch(modFile.name, _currentFilter)) continue;

            AddAssetButton(modFile.name);
        }
    }

    private void AddAssetButton(string filePath)
    {
        Asset asset = AssetFactory.CreateAsset(filePath);
        if (asset == null)
        {
            GD.PrintErr($"Failed to create asset for {filePath}");
            return;
        }

        if(filePath.EndsWith(".wav"))
            ArborCoroutine.StartCoroutine(LoadAndDisplaySound(asset), this);
        else
            ArborCoroutine.StartCoroutine(LoadAndDisplayAsset(asset), this);
    }

    private bool IsMatch(string filePath, string filter)
    {
        if (filter == "All") return true;
        if (filter == "Image" && filePath.EndsWith(".png")) return true;
        if (filter == "Sound" && filePath.EndsWith(".wav")) return true;
        if (filter == "Actor" && filePath.EndsWith(".actor")) return true;
        if (filter == "Script" && filePath.EndsWith(".lua")) return true;
        return false;
    }

    private void OnAssetGuiInput(InputEvent @event, Asset filePath)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Doubleclick)
        {
            EmitSignal(nameof(AssetSelected), filePath);
            Hide();

            _onAssetSelectedCallback?.Invoke(filePath);
        }
    }

    public void Open(Action<Asset> onAssetSelected = null)
    {
        _onAssetSelectedCallback = onAssetSelected;
        PopupCentered();
        _loadDelayTimer.Start();
    }

    private IEnumerator LoadAndDisplayAsset(Asset asset)
    {
        yield return asset.LoadAsset();

        Control previewButton = asset.CreatePreviewButton();
        previewButton.Disconnect("pressed", asset, nameof(asset.OnButtonPressed));
        previewButton.Connect("gui_input", this, nameof(OnAssetGuiInput), new Godot.Collections.Array { asset });
        _assetListContainer.AddChild(previewButton);

        GD.Print($"Added {asset.FilePath} to UI.");
    }

    private IEnumerator LoadAndDisplaySound(Asset asset)
    {
        Control previewButton = asset.CreatePreviewButton();
        previewButton.Disconnect("pressed", asset, nameof(asset.OnButtonPressed));
        previewButton.Connect("gui_input", this, nameof(OnAssetGuiInput), new Godot.Collections.Array { asset.FilePath });
        _assetListContainer.AddChild(previewButton);

        yield return asset.LoadAsset();

        GD.Print($"Added {asset.FilePath} to UI.");
    }
}
