using Godot;
using System;
using System.Collections;

public abstract class Asset : Control
{
    public string FilePath { get; private set; }
    public string FileName => FilePath.GetFile();
    protected Texture _thumbnail;
    protected Texture _defaultPreviewTexture;
    protected Asset(string filePath, string defaultPreviewPath)
    {
        FilePath = filePath;
        _defaultPreviewTexture = (Texture)ResourceLoader.Load(defaultPreviewPath);
    }

    public abstract IEnumerator LoadAsset();

    public abstract void OnButtonPressed();

    public virtual Button CreatePreviewButton()
    {
        Button button = new Button
        {
            RectMinSize = new Vector2(300, 200),
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
            // use preview from asset or default if none
            Texture = _thumbnail ?? _defaultPreviewTexture, 
            Expand = true,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            RectMinSize = new Vector2(300, 150)
        };

        container.AddChild(textureRect);

        // file name text
        Label nameLabel = new Label
        {
            Text = FileName,
            Align = Label.AlignEnum.Center,
            RectMinSize = new Vector2(300, 30),
            ClipText = true // truncates if too long
        };

        container.AddChild(nameLabel);
        button.AddChild(container);

        button.Connect("pressed", this, nameof(OnButtonPressed));

        return button;
    }
}

public enum AssetType
{
    All,
    Image,
    Sound,
    Actor,
    Script
}
