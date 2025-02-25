using Godot;
using System;
using System.Collections;

public abstract class Asset : Control
{
    public string FilePath { get; private set; }
    public string FileName => FilePath.GetFile(); // Extracts just the file name
    protected Texture _thumbnail;
    protected Texture _defaultPreviewTexture; // Default icon if no preview is available

    protected Asset(string filePath, string defaultPreviewPath)
    {
        FilePath = filePath;
        _defaultPreviewTexture = (Texture)ResourceLoader.Load(defaultPreviewPath);
    }

    // Abstract method for loading the asset
    public abstract IEnumerator LoadAsset();

    // Creates a UI button with a preview and text
    public Button CreatePreviewButton()
    {
        Button button = new Button
        {
            RectMinSize = new Vector2(150, 200),
            ExpandIcon = true
        };

        // set theme
        Theme buttonTheme = (Theme)ResourceLoader.Load("res://new_theme.tres");
        if (buttonTheme != null)
        {
            button.Theme = buttonTheme;
        }

        VBoxContainer container = new VBoxContainer();

        // image preview
        TextureRect textureRect = new TextureRect
        {
            Texture = _thumbnail ?? _defaultPreviewTexture, // Use asset preview or fallback to default
            Expand = true,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            RectMinSize = new Vector2(150, 150)
        };

        container.AddChild(textureRect);

        // file name text
        Label nameLabel = new Label
        {
            Text = FileName,
            Align = Label.AlignEnum.Center,
            RectMinSize = new Vector2(150, 30),
            ClipText = true // truncate text if too long
        };

        container.AddChild(nameLabel);
        button.AddChild(container);

        return button;
    }
}
