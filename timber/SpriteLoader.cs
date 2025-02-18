using Godot;
using System.Collections;
using System.Collections.Generic;

public class SpriteLoader : Control
{
    private GridContainer _gridContainer;
    private ScrollContainer _scrollContainer;

    public override void _Ready()
    {
        AddTextButton("SpriteLoader Ready!");
        GD.Print("SpriteLoader Ready!");
        _gridContainer = GetNode<GridContainer>("ScrollContainer/GridContainer");

        ArborCoroutine.StartCoroutine(LoadSprites(), this);
    }

    // attempts to load all png files from ArborResource
    IEnumerator LoadSprites()
    {
        ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        yield return ArborResource.WaitFor("mod_file_manifest.json");
        ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");

        // try to find all png files in images folder

        List<string> spriteFiles = manifest.Search("resources/images/.*\\.png");

        GD.Print($"Found {spriteFiles.Count} sprite images in S3 bucket");
        AddTextButton($"Found {spriteFiles.Count} sprite images in S3 bucket");

        foreach (string spriteFile in spriteFiles)
        {
            ArborResource.Load<Texture>(spriteFile);
        }

        foreach (string spriteFile in spriteFiles)
        {
            yield return ArborResource.WaitFor(spriteFile);

            Texture texture = ArborResource.Get<Texture>(spriteFile);
            if (texture != null)
                AddSpriteButton(spriteFile, texture);
            else
                GD.PrintErr($"Failed to load texture: {spriteFile}");
        }
    }

    void AddTextButton(string message)
    {
        Button newButton = new Button();
        newButton.Text = $"{message}";
        newButton.RectMinSize = new Vector2(100, 100);
        // newButton.Connect("pressed", this, nameof(OnSpriteButtonPressed), new Godot.Collections.Array { spritePath });

        _gridContainer.AddChild(newButton);
    }

    void AddSpriteButton(string spritePath, Texture texture)
    {
        Button spriteButton = new Button();
        spriteButton.RectMinSize = new Vector2(100, 100);
        spriteButton.Icon = texture;
        spriteButton.HintTooltip = spritePath;

        spriteButton.Connect("pressed", this, nameof(OnSpriteButtonPressed), new Godot.Collections.Array { spritePath });

        _gridContainer.AddChild(spriteButton);
    }

    void OnSpriteButtonPressed(string spritePath)
    {
        GD.Print($"Sprite button clicked: {spritePath}");
        // fake debug
        AddTextButton("Button Clicked!");
    }
}
