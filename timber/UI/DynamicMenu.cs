using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class DynamicMenu : Control
{
    public VBoxContainer header;
    public VBoxContainer content;
    public DynamicMenu _parent;
    public bool _disabled = false;
    public ColorRect overlay;

    private Action OnMenuClose;
    private Control[] _components;

    public DynamicMenu(DynamicMenu parent, params Control[] components)
    {
        _parent = parent;
        _components = components;
    }

    public DynamicMenu(params Control[] components) : this(null, components) { }

    // Style variables
    public static Vector2? _size;
    public static Vector2? _location;

    private static string _panelImg = null;
    private static Color _bgColor = new Color(40.0f / 255.0f, 36.0f / 255.0f, 44.0f / 255.0f, 1.0f);
    private static Color _shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
    private static Color _overlayColor = new Color(0, 0, 0, 0.5f);
    private static Color _secondaryColor = new Color(72.0f / 255.0f, 67.0f / 255.0f, 77.0f / 255.0f, 1.0f);

    private static string _buttonImg = "res://UI/default_tex.png";
    private static string _font = "res://fonts/Roboto-Regular.ttf";
    private static Color _textColor = new Color(1, 1, 1, 1);

    public static void Configure(Vector2? location = null, Vector2? size = null,
        Color? bgColor = null, Color? shadowColor = null, Color? overlayColor = null, Color? secondaryColor = null, 
        string font = "res://fonts/Roboto-Regular.ttf", Color? textColor = null,
        string panelImg = null, string buttonImg = "res://UI/default_tex.png")
    {
        _size = size ?? new Vector2(400.0f, 650.0f);
        _location = location ?? OS.WindowSize * 0.5f - _size * 0.5f;

        _bgColor = bgColor ?? _bgColor;
        _shadowColor = shadowColor ?? _shadowColor;
        _overlayColor = overlayColor ?? _overlayColor;
        _secondaryColor = secondaryColor ?? _secondaryColor;

        _font = font;
        _textColor = textColor ?? _textColor;

        _panelImg = panelImg;
        _buttonImg = buttonImg;
    }

    public void CreateMenu()
    {
        InitMenu();
        foreach (Control component in _components)
        {
            component.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
            component.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
            component.RectMinSize = new Vector2(_size.Value.x, (_size.Value.y - 50.0f) / 8.0f);
            GD.Print(component.RectMinSize);
            content.AddChild(component);
        }
    }

    private void InitMenu()
    {
        if (_parent != null)
        {
            _parent._disabled = true;
            ColorRect tempOverlay = new ColorRect();
            tempOverlay.RectMinSize = OS.WindowSize; // Set to screen size
            tempOverlay.Color = _overlayColor; // Set to semi-transparent black
            _parent.AddChild(tempOverlay);
            _parent.overlay = tempOverlay;
            _parent.overlay.Raise();
            if(_parent._parent != null)
            {
                _parent._parent.overlay.QueueFree();
            }
            //_parent.Visible = false;
        }

        UIManager.instance.AddChild(this);

        // Create a MarginContainer to position the Panel
        MarginContainer marginContainer = new MarginContainer();
        marginContainer.RectPosition = (Vector2)_location;
        marginContainer.RectSize = (Vector2)_size;

        this.AddChild(marginContainer);

        // Create a Panel to serve as a background
        Panel panel = new Panel();
        panel.RectSize = (Vector2)_size;
        if (_panelImg != null)
        {
            StyleBoxTexture style = new StyleBoxTexture();
            style.Texture = (Texture)GD.Load(_panelImg);
            panel.AddStyleboxOverride("normal", style);

            // "Tint" the panel image (only works on white images rn)
            ShaderMaterial tintMaterial = new ShaderMaterial();
            tintMaterial.Shader = ResourceLoader.Load("res://UI/tint.gdshader") as Shader;
            tintMaterial.SetShaderParam("tint_color", _bgColor);
            panel.Material = tintMaterial;

            // Add shadow behind panel
            Panel shadow = new Panel();
            StyleBoxFlat shadowStyle = new StyleBoxFlat();
            shadowStyle.ShadowOffset = new Vector2(10, 10);
            shadowStyle.ShadowColor = _shadowColor;
            panel.AddChild(shadow);
            shadow.ShowBehindParent = true;
        }
        else
        {
            StyleBoxFlat style = new StyleBoxFlat();
            style.ShadowColor = _shadowColor;
            style.BgColor = _bgColor;
            style.ShadowOffset = new Vector2(10, 10); // Shadow offset (10px to the right and down)
            style.ShadowSize = 10;
            panel.AddStyleboxOverride("panel", style);
        }


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

        VBoxContainer headerContent = new VBoxContainer();
        headerContent.RectMinSize = headerContainer.RectMinSize;
        headerContent.RectSize = headerContainer.RectMinSize;
        headerContainer.AddChild(headerContent);

        // Create a close button
        Button closeButton = new Button();
        closeButton.Text = "X";
        closeButton.Align = Button.TextAlign.Center;
        closeButton.AnchorRight = 1.0f;
        closeButton.AnchorLeft = 0.9f;
        StyleBoxFlat styleBox = new StyleBoxFlat();
        styleBox.BgColor = _secondaryColor;
        closeButton.AddStyleboxOverride("normal", styleBox);
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

        header = headerContent;
        content = contentBox;
    }


    // Add to header
    public void AddToHeader(params Control[] components)
    {
        foreach (Control component in components)
        {
            component.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
            component.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
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
    public static Label MenuLabel(string text, int fontSize = 20)
    {
        Label label = new Label();
        label.Align = Label.AlignEnum.Center;
        label.Valign = Label.VAlign.Center;

        DynamicFont font = new DynamicFont();
        font.FontData = (DynamicFontData)GD.Load(_font);
        font.Size = fontSize;
        label.AddFontOverride("font", font);
        label.AddColorOverride("font_color", _textColor);

        label.Text = text;
        return label;
    }

    // Buttons (Text, action - on press)
    public static CustomButton MenuButton(string text, Action onPress, string img = null, int fontSize = 20)
    {
        CustomButton button = new CustomButton(onPress);
        button.Expand = true;
        button.RectMinSize = new Vector2(_size.Value.x, (_size.Value.y - 50.0f) / 8.0f);
        button.AnchorRight = 1;
        button.AnchorBottom = 1;
        button.MarginRight = 0;
        button.MarginBottom = 0;

        // Initialize StyleBoxTexture
        StyleBoxTexture styleBoxTexture = new StyleBoxTexture();
        styleBoxTexture.Texture = (Texture)GD.Load(_buttonImg);
        GD.Print(_buttonImg);

        // Define the 9-slice margins
        /*styleBoxTexture.MarginLeft = 10;
        styleBoxTexture.MarginRight = 10;
        styleBoxTexture.MarginTop = 10;
        styleBoxTexture.MarginBottom = 10;*/

        //button.AddStyleboxOverride("normal", styleBoxTexture);
        button.Set("custom_styles/normal", styleBoxTexture);

        Texture texture = (Texture)GD.Load(_buttonImg);
        GD.Print(button.RectMinSize);

        Vector2 textureSize = new Vector2(texture.GetWidth(), texture.GetHeight());

        if (button.RectMinSize.x < textureSize.x || button.RectMinSize.y < textureSize.y)
        {
            GD.Print("Button is smaller than the texture.");
        }
        else
        {
            GD.Print("Button is larger or equal to the texture.");
        }

        button.SelfModulate = _secondaryColor;

        // Add text
        Label label = new Label();
        label.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
        label.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
        label.AnchorRight = 1; 
        label.AnchorBottom = 1;   
        label.MarginRight = 0;      
        label.MarginBottom = 0;
        label.Align = Label.AlignEnum.Center;
        label.Valign = Label.VAlign.Center;

        DynamicFont font = new DynamicFont();
        font.FontData = (DynamicFontData)GD.Load(_font);
        font.Size = fontSize;
        label.AddFontOverride("font", font);
        label.AddColorOverride("font_color", _textColor);

        label.Text = text;
        button.AddChild(label);
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

    // Do something when closing menu
    public void SetOnMenuClose(Action action)
    {
        OnMenuClose = action;
    }

    // Close menu (don't call directly usually)
    public void CloseMenu()
    {
        OnMenuClose?.Invoke();
        GD.Print("DELETING MENU");
        QueueFree();
    }

    // Why is this public?
    public override void _ExitTree()
    {
        if (_parent != null)
        {
            _parent._disabled = false;
            _parent.overlay.QueueFree();
            if (_parent._parent != null)
            {
                ColorRect tempOverlay = new ColorRect();
                tempOverlay.RectMinSize = OS.WindowSize; // Set to screen size
                tempOverlay.Color = new Color(0, 0, 0, 0.5f); // Set to semi-transparent black
                _parent._parent.AddChild(tempOverlay);
                _parent._parent.overlay = tempOverlay;
                _parent._parent.overlay.Raise();
            }
            //_parent.Visible = true;
        }

        base._ExitTree();
    }
}

public class CustomButton : TextureButton
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
            if(toggleGroup != null && toggleGroup.ValidateChange(selected))
            {
                selected = !selected;
                _action?.Invoke();
            }
            this.Pressed = !selected; // No clue why this needs to be this way
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
            }
            return true;
        }
        else if (selectOnlyOne && initState) // Trying to deselect current option
        {
            return false;
        }
        else if(selectAtLeastOne && initState) // Trying to deselect current option
        {
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