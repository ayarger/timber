using Godot;

public class ButtonSpawner : Control
{
    private GridContainer _gridContainer;
    private Timer _timer;
    private int _buttonCount = 1;

    public override void _Ready()
    {
        GD.Print("ButtonSpawner _Ready() is running!");

        // Locate the GridContainer inside SpritesTab
        _gridContainer = GetNode<GridContainer>("ScrollContainer/GridContainer");

        if (_gridContainer == null)
        {
            GD.PrintErr("ERROR: GridContainer not found!");
            return;
        }

        GD.Print("GridContainer found, setting up Timer...");

        // Create and configure the Timer
        _timer = new Timer();
        _timer.WaitTime = 2.0f; // 2 seconds
        _timer.Autostart = true;
        _timer.OneShot = false;
        _timer.Connect("timeout", this, nameof(OnTimerTimeout));
        AddChild(_timer);

        GD.Print("Timer started, buttons will spawn every 2 seconds...");
    }

    private void OnTimerTimeout()
    {
        Button newButton = new Button();
        newButton.Text = $"Button {_buttonCount}";
        newButton.RectMinSize = new Vector2(100, 100);
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
