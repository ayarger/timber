using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

public class ButtonSpawner : Control
{
    private GridContainer _gridContainer;
    private Timer _timer;
    private int _buttonCount = 1;

    private Texture _buttonIconTexture;

    //private const string ImageFilePath = "resources/images/chunk_idle.png";
    private const string ImageFilePath = "images/chunk_idle.png";


    public override void _Ready()
    {
        GD.Print("ButtonSpawner _Ready() is running!");

        _gridContainer = GetNode<GridContainer>("ScrollContainer/GridContainer");

        if (_gridContainer == null)
        {
            GD.PrintErr("ERROR: GridContainer not found!");
            return;
        }

        GD.Print("GridContainer found, setting up Timer...");

        _timer = new Timer();
        _timer.WaitTime = 2.0f; // 2 seconds
        _timer.Autostart = true;
        _timer.OneShot = false;
        _timer.Connect("timeout", this, nameof(OnTimerTimeout));
        AddChild(_timer);

        GD.Print("Timer Started!");

        ArborCoroutine.StartCoroutine(LoadButtonIcon(), this);
    }

    IEnumerator LoadButtonIcon()
    {
        GD.Print($"Loading icon from path: {ImageFilePath}");

        // ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        // yield return ArborResource.WaitFor("mod_file_manifest.json");
        // ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");

        //   // try to find all png files in images folder

        // List<string> spriteFiles = manifest.Search("resources/images/.*\\.png");

        //ArborResource.Load<Texture>(ImageFilePath);
        //yield return ArborResource.WaitFor(ImageFilePath);

        ArborResource.UseResource(
			ImageFilePath,
			(Texture texture) => {
				Icon title_logo_node = GetNode<Icon>("Button");
				_buttonIconTexture = texture;
			},
			this
		);

        _buttonIconTexture = ArborResource.Get<Texture>(ImageFilePath);

        if (_buttonIconTexture == null)
        {
            GD.PrintErr($"Failed to load image from path: {ImageFilePath}");
            yield break;
        }

        GD.Print("Button icon successfully loaded!");

        //_timer.Start();
    }

    private void OnTimerTimeout()
    {
        
        SpawnButton();
    }

    private void SpawnButton()
    {
        Button newButton = new Button();
        newButton.Text = $"Button {_buttonCount}";
        newButton.RectMinSize = new Vector2(100, 100);

        // try set icon
        if (_buttonIconTexture != null)
        {
            newButton.Icon = _buttonIconTexture;
        }

        newButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array { _buttonCount });

        _gridContainer.AddChild(newButton);
        GD.Print($"Added Button {_buttonCount}");

        _buttonCount++;
    }

    private void OnButtonPressed(int buttonId)
    {
        GD.Print($"Button {buttonId} pressed!");
    }
    
}
