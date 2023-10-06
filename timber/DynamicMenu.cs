using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class DynamicMenu : Control
{
    public MenuHeader header;
    public VBoxContainer content;
    public Vector2? _size;
    public Vector2? _location;
    public DynamicMenu _parent;
    public bool _disabled = false;
    private Action OnMenuClose;

    public DynamicMenu(Vector2? location, Vector2? size, DynamicMenu parent, params Control[] components)
    {
        _size = size;
        _location = location;
        _parent = parent;

        InitMenu();
        foreach (Control component in components)
        {
            component.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
            component.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
            component.RectMinSize = new Vector2(_size.Value.x, (_size.Value.y - 50.0f) / 8.0f);
            content.AddChild(component);
        }
    }

    public DynamicMenu(Vector2 location, Vector2 size, params Control[] components) : this(location, size, null, components) { }

    public DynamicMenu(DynamicMenu parent, params Control[] components) : this(null, null, parent, components) { }

    public DynamicMenu(params Control[] components) : this(null, null, null, components) { }


    private void InitMenu()
    {
        if (_parent != null)
        {
            _parent._disabled = true;
            _parent.Visible = false;
        }

        _size = _size ?? new Vector2(400.0f, 650.0f);
        _location = _location ?? OS.WindowSize * 0.5f - _size * 0.5f;

        UIManager.instance.AddChild(this);

        // Create a MarginContainer to position the Panel
        MarginContainer marginContainer = new MarginContainer();
        marginContainer.RectPosition = (Vector2)_location;
        marginContainer.RectSize = (Vector2)_size;

        this.AddChild(marginContainer);

        // Create a Panel to serve as a background
        // DESIGN STUFF: Maybe make all of these customizable in the future
        Panel panel = new Panel();
        panel.RectSize = (Vector2)_size;
        StyleBoxFlat styleBox = new StyleBoxFlat();
        styleBox.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        styleBox.BgColor = new Color(40.0f / 255.0f, 36.0f / 255.0f, 44.0f / 255.0f, 1.0f);
        styleBox.ShadowOffset = new Vector2(10, 10); // Shadow offset (10px to the right and down)
        styleBox.ShadowSize = 10;

        // Set the StyleBoxFlat to the panel
        panel.AddStyleboxOverride("panel", styleBox);

        marginContainer.AddChild(panel);

        // Create a VBoxContainer inside the Panel
        VBoxContainer vbox = new VBoxContainer();
        panel.RectSize = (Vector2)_size;
        panel.AddChild(vbox);

        // Create a container for the header and close button
        MenuHeader headerContainer = new MenuHeader();
        headerContainer.Configure(_top_window_control: marginContainer);
        headerContainer.RectSize = new Vector2(_size.Value.x, 50.0f);
        headerContainer.RectMinSize = new Vector2(_size.Value.x, 50.0f);

        vbox.AddChild(headerContainer);

        // Create a close button
        Button closeButton = new Button();
        closeButton.Text = "X";
        closeButton.Align = Button.TextAlign.Center;
        closeButton.AnchorRight = 1.0f;
        closeButton.AnchorLeft = 0.9f;
        closeButton.Connect("pressed", this, nameof(CloseMenu));
        headerContainer.AddChild(closeButton);

        // Create a ScrollContainer
        ScrollContainer scrollContainer = new ScrollContainer();
        scrollContainer.RectMinSize = new Vector2(_size.Value.x, _size.Value.y - 50.0f);
        scrollContainer.RectSize = new Vector2(_size.Value.x, _size.Value.y - 50.0f);
        vbox.AddChild(scrollContainer);

        // Create another VBoxContainer inside the ScrollContainer for the content
        VBoxContainer contentBox = new VBoxContainer();
        contentBox.RectMinSize = new Vector2(_size.Value.x, scrollContainer.RectMinSize.y);
        contentBox.RectSize = new Vector2(_size.Value.x, scrollContainer.RectMinSize.y);
        GD.Print(contentBox.RectMinSize.y);
        scrollContainer.AddChild(contentBox);

        header = headerContainer;
        content = contentBox;
    }

    // Add to header
    public void AddToHeader(params Control[] components)
    {
        foreach (Control component in components)
        {
            header.AddChild(component);
        }
    }

    // New row
    public static HBoxContainer MenuRow(params Control[] components)
    {
        HBoxContainer row = new HBoxContainer();
        foreach (Control component in components)
        {
            component.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
            component.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
            row.AddChild(component);
        }
        return row;
    }

    // Spacer
    public static Control MenuSpacer()
    {
        Control spacer = new Control();
        return spacer;
    }

    // Text label (Text)
    public static Label MenuLabel(string text)
    {
        Label label = new Label();
        label.Align = Label.AlignEnum.Center;
        label.Valign = Label.VAlign.Center;
        label.Text = text;
        return label;
    }

    // Buttons (Text, action - on press)
    public static CustomButton MenuButton(string text, Action onPress)
    {
        CustomButton button = new CustomButton(onPress);
        button.Text = text;
        return button;
    }

    // Toggles (Text, action - on press)
    public static Toggle MenuToggle(bool initSelected = false)
    {
        Toggle toggle = new Toggle(initSelected);
        return toggle;
    }

    // Toggles (Text, action - on press)
    public ToggleGroup AddToggleGroup(bool selectAtLeastOne, bool selectOnlyOne, params Toggle[] toggles)
    {
        ToggleGroup toggleGroup = new ToggleGroup(selectAtLeastOne, selectOnlyOne, toggles);
        content.AddChild(toggleGroup);
        return toggleGroup;
    }

    public ToggleGroup AddToggleGroup(params Toggle[] toggles)
    {
        ToggleGroup toggleGroup = new ToggleGroup(false, false, toggles);
        content.AddChild(toggleGroup);
        return toggleGroup;
    }

    // Sliders (Min value, max value, start value, action - on value changed)
    public static CustomSlider MenuSlider(float minVal, float maxVal, float startVal, Action<float> onValueChanged)
    {
        CustomSlider slider = new CustomSlider(onValueChanged);
        slider.MinValue = 0;
        slider.MaxValue = 100;
        slider.Value = startVal;
        slider.Connect("value_changed", slider, nameof(onValueChanged));
        return slider;
    }

    public void SetOnMenuClose(Action action)
    {
        OnMenuClose = action;
    }

    public void CloseMenu()
    {
        OnMenuClose?.Invoke();
        GD.Print("DELETING MENU");
        QueueFree();
    }

    public override void _ExitTree()
    {
        if (_parent != null)
        {
            _parent._disabled = false;
            _parent.Visible = true;
        }

        base._ExitTree();
    }
}

public class CustomButton : Button
{
    private readonly Action _action;

    public CustomButton(Action action)
    {
        _action = action;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            _action?.Invoke();
            base._GuiInput(@event);
        }
    }
}

public class Toggle : CheckBox
{
    private Action _action;
    public bool selected;
    public ToggleGroup toggleGroup;

    public Toggle(bool initSelected = false)
    {
        selected = initSelected;
        this.Pressed = selected;
    }

    public void SetOnValueChanged(Action action)
    {
        _action = action;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            GD.Print(toggleGroup.ValidateChange(selected));
            if(toggleGroup != null && toggleGroup.ValidateChange(selected))
            {
                selected = !selected;
                _action?.Invoke();
            }
            this.Pressed = selected;
        }
    }
}

public class ToggleGroup : Control
{
    private List<Toggle> _toggles = new List<Toggle>();
    public bool selectAtLeastOne;
    public bool selectOnlyOne;

    public ToggleGroup(bool atLeastOne, bool onlyOne, params Toggle[] toggles)
    {
        selectAtLeastOne = atLeastOne;
        selectOnlyOne = onlyOne;

        if(toggles.Length <= 0)
        {
            return;
        }

        int countSelected = 0;
        foreach(Toggle toggle in toggles)
        {
            _toggles.Add(toggle);
            toggle.toggleGroup = this;
            if (toggle.selected)
            {
                ++countSelected;
            }
        }
        if ((selectAtLeastOne || selectOnlyOne) && countSelected < 1)
        {
            _toggles[0].selected = true;
            _toggles[0].Pressed = true;
        }
        else if (selectOnlyOne && countSelected > 1)
        {
            bool selected = false;
            foreach (Toggle toggle in _toggles)
            {
                if (toggle.selected && !selected)
                {
                    selected = true;
                } else if (toggle.selected)
                {
                    toggle.selected = false;
                    toggle.Pressed = false;
                }
            }
        }
    }

    public ToggleGroup(params Toggle[] toggles) : this(false, false, toggles) { }

    public bool ValidateChange(bool initState)
    {
        if(selectOnlyOne && !initState) // Trying to select a new option
        {
            foreach(Toggle toggle in _toggles)
            {
                if(toggle.selected)
                {
                    toggle.selected = false;
                    toggle.Pressed = false;
                }
                return true;
            }
        }
        else if (selectOnlyOne && initState) // Trying to deselect current option
        {
            return false;
        }
        else if(selectAtLeastOne && initState) // Trying to deselect current option
        {
            GD.Print("Trying to deselect at least one");
            int count = 0;
            foreach (Toggle toggle in _toggles)
            {
                if (toggle.selected)
                {
                    ++count;
                }
            }
            if(count > 1) // More than one selected so can deselect
            {
                return true;
            } else // Only one selected so can't deselect
            {
                return false;
            }
        } else
        {
            return true;
        }
        return false;
    }
}

public class CustomSlider : HSlider
{
    private readonly Action<float> _action;

    public CustomSlider(Action<float> action)
    {
        _action = action;
        Connect("value_changed", this, nameof(OnValueChanged));
    }

    private void OnValueChanged(float value)
    {
        _action?.Invoke(value);
    }
}