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
        // Locate the GridContainer inside the Sprites tab
        _gridContainer = GetNode<GridContainer>("ScrollContainer/GridContainer");

        // Start fetching images from AWS S3 using ArborResource
        ArborCoroutine.StartCoroutine(LoadSprites(), this);
    }

    IEnumerator LoadSprites()
    {
        // Step 1: Load the mod file manifest to retrieve available image files
        ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        yield return ArborResource.WaitFor("mod_file_manifest.json");
        ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");

        // Step 2: Filter for PNG files in the "resources/images/" folder
        List<string> spriteFiles = manifest.Search("resources/images/.*\\.png");

        GD.Print($"Found {spriteFiles.Count} sprite images in S3 bucket");
        AddTextButton($"Found {spriteFiles.Count} sprite images in S3 bucket");

        // Step 3: Load each PNG file asynchronously
        foreach (string spriteFile in spriteFiles)
        {
            ArborResource.Load<Texture>(spriteFile);
        }

        // Step 4: Wait for each image to be loaded and create buttons
        foreach (string spriteFile in spriteFiles)
        {
            yield return ArborResource.WaitFor(spriteFile);

            Texture texture = ArborResource.Get<Texture>(spriteFile);
            if (texture != null)
            {
                AddSpriteButton(spriteFile, texture);
            }
            else
            {
                GD.PrintErr($"Failed to load texture: {spriteFile}");
            }
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
        // Create a new button
        Button spriteButton = new Button();
        spriteButton.RectMinSize = new Vector2(100, 100); // Set button size
        spriteButton.Icon = texture; // Set the image as button icon
        spriteButton.HintTooltip = spritePath; // Tooltip for image name

        // Connect button click event
        spriteButton.Connect("pressed", this, nameof(OnSpriteButtonPressed), new Godot.Collections.Array { spritePath });

        // Add to the grid container
        _gridContainer.AddChild(spriteButton);
    }

    void OnSpriteButtonPressed(string spritePath)
    {
        GD.Print($"Sprite button clicked: {spritePath}");
        // Handle sprite selection logic here
        AddTextButton("Button Clicked!");
    }
}
