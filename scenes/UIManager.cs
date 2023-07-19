/* https://docs.google.com/document/d/143EIk6GPQYmyi_rvcmdRf6JrtR310wKdd2p85ehELUE/edit?usp=sharing */

using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : Node
{
    static UIManager instance;

    Panel flash_panel;
    Color flash_panel_invisible = new Color(1, 1, 1, 0);
    Color flash_panel_visible = new Color(1, 1, 1, 1);

    public override void _Ready()
    {
        instance = this;
        flash_panel = GetNode<Panel>("flash_panel");
        flash_panel.MouseFilter = Control.MouseFilterEnum.Ignore;
        flash_panel.Modulate = new Color(1, 1, 1, 0.0f);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if(flash_panel.Modulate.a > 0.01f)
        {
            flash_panel.Modulate = flash_panel.Modulate.LinearInterpolate(flash_panel_invisible, flash_degrade_rate);
        }
    }

    float flash_degrade_rate = 0.01f;
    public static void RequestFlash(float _flash_degrade_rate = 0.01f)
    {
        instance.flash_degrade_rate = _flash_degrade_rate;
        instance.flash_panel.Modulate = instance.flash_panel_visible;
    }

    public static void SimpleMenu (string header, List<string> buttonLabels, List<Action> buttonActions, Vector2? location = null, Vector2? size = null)
    {
        instance._SimpleMenu(header, buttonLabels, buttonActions, location, size);
    }

    void _SimpleMenu(string header, List<string> buttonLabels, List<Action> buttonActions, Vector2? location=null, Vector2? size=null)
    {
        const float height_per_option = 40.0f;
        float final_vertical_size = Mathf.Min(60.0f + height_per_option * buttonLabels.Count, 800);

        size = size ?? new Vector2(400.0f, final_vertical_size);
        location = location ?? OS.WindowSize * 0.5f - size * 0.5f;

        if (buttonLabels.Count != buttonActions.Count)
        {
            GD.Print("Error: The number of button labels must match the number of actions.");
            return;
        }

        // Create a MarginContainer to position the Panel
        MarginContainer marginContainer = new MarginContainer();
        marginContainer.RectPosition = (Vector2)location;
        marginContainer.RectSize = (Vector2)size;

        AddChild(marginContainer);

        // Create a Panel to serve as a background
        Panel panel = new Panel();
        panel.RectSize = (Vector2)size;
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
        panel.RectSize = (Vector2)size;
        panel.AddChild(vbox);

        // Create a container for the header and close button
        MenuHeader headerContainer = new MenuHeader();
        headerContainer.Configure(_top_window_control:marginContainer);
        headerContainer.RectSize = new Vector2(size.Value.x, 50.0f);
        headerContainer.RectMinSize = new Vector2(size.Value.x, 50.0f);

        vbox.AddChild(headerContainer);

        // Create a Label as the header
        Label headerLabel = new Label();
        headerLabel.RectSize = new Vector2(size.Value.x, 50.0f);
        headerLabel.RectMinSize = new Vector2(size.Value.x, 50.0f);
        headerLabel.Text = header;
        headerContainer.AddChild(headerLabel);

        // Create a close button
        Button closeButton = new Button();
        closeButton.Text = "X";
        closeButton.Align = Button.TextAlign.Center;
        closeButton.AnchorRight = 1.0f;
        closeButton.AnchorLeft = 0.9f;
        closeButton.Connect("pressed", this, nameof(CloseButtonPressed), new Godot.Collections.Array { marginContainer });
        headerContainer.AddChild(closeButton);

        // Create a ScrollContainer
        ScrollContainer scrollContainer = new ScrollContainer();
        scrollContainer.RectMinSize = (Vector2)size;
        scrollContainer.RectSize = (Vector2)size;
        vbox.AddChild(scrollContainer);

        // Create another VBoxContainer inside the ScrollContainer for the buttons
        VBoxContainer buttonsBox = new VBoxContainer();
        buttonsBox.RectMinSize = new Vector2(size.Value.x, buttonsBox.RectSize.y);
        buttonsBox.RectSize = new Vector2(size.Value.x, buttonsBox.RectSize.y);
        scrollContainer.AddChild(buttonsBox);

        // Create buttons and add them to the VBoxContainer
        for (int i = 0; i < buttonLabels.Count; i++)
        {
            CustomButton button = new CustomButton(buttonActions[i]);
            button.Text = buttonLabels[i];
            button.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
            button.RectSize = new Vector2(size.Value.x, height_per_option);
            button.RectMinSize = new Vector2(size.Value.x, height_per_option);
            buttonsBox.AddChild(button);
        }

        menus.Add(marginContainer);
    }

    List<Control> menus = new List<Control>();

    public static void ClearAllMenus()
    {
        foreach(Control c in instance.menus)
        {
            if(IsInstanceValid(c))
                c.QueueFree();
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Check if the event is an InputEventKey
        if (@event is InputEventKey eventKey)
        {
            // Check if the escape key was just pressed
            if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
            {
                SettingsMenu();
            }
        }
    }

    void SettingsMenu()
    {
        List<string> options = new List<string>() { "Discord", "Credits2", "Quit", "debug_victory", "debug_lose", "debug_open_webgl_build", "debug_webgl_upload", "debug_webgl_upload_opt", "debug_change_title_screen", "debug_change_title_audio", "debug_test_image_upload", "debug_test_fullscreen" };
        List<System.Action> actions = new List<Action>()
        {
            () => { OS.ShellOpen(@"https://discord.gg/2GZ3SxVA7Q"); },
            () => {  },
            () => { GameOver.PerformGameOver(new GameOverRequest() {fast_mode = true}); },
            () => { VictoryScene.PerformVictory(); },
            () => { GameOver.PerformGameOver(new GameOverRequest()); },
            () => { WebBuildUploader.OpenWebBuild(); },
            () => { WebBuildUploader.UploadWebBuild(); },
            () => { WebBuildUploader.UploadOptimizedWebBuild(); },
            () => {
                ArborCoroutine.StartCoroutine(ArborResource.Upload<Texture>("images/title_screen_background.png"));
            },
            () => {
                ArborCoroutine.StartCoroutine(ArborResource.Upload<AudioStream>("sounds/bgm_title.ogg"));
            },
            () => {
                ArborCoroutine.StartCoroutine(ArborResource.Upload<Texture>("images/spot_idle.png"));
                //ArborCoroutine.StartCoroutine(changeCharacterExperiment());
            },
            () => {
                string jsCode = @"
                    document.getElementById('canvas').requestFullscreen ? document.getElementById('canvas').requestFullscreen() :
                    document.getElementById('canvas').mozRequestFullScreen ? document.getElementById('canvas').mozRequestFullScreen() :
                    document.getElementById('canvas').webkitRequestFullscreen ? document.getElementById('canvas').webkitRequestFullscreen() : false;
                ";

                JavaScript.Eval(jsCode);
            },
        };

        UIManager.SimpleMenu("pause", options, actions);
    }

    IEnumerator changeCharacterExperiment()
    {
        yield return ArborResource.Upload<Texture>("images/spot_idle.png");

        Texture uploaded_tex = ArborResource.Get<Texture>("images/spot_idle.png");
        if (uploaded_tex == null)
            yield break;

        /* Scale */
        MeshInstance mi = GetNode<MeshInstance>("../Main/LuaLoader/Spot/view/mesh");
        Spatial view = GetNode<Spatial>("../Main/LuaLoader/Spot/view");
        view.Scale = new Vector3(uploaded_tex.GetWidth(), uploaded_tex.GetHeight(), 1.0f) * 0.01f;

        //shadow_view.Scale = new Vector3(Mathf.Min(2.0f, view.Scale.x), shadow_view.Scale.y, shadow_view.Scale.z);

        ShaderMaterial mat = (ShaderMaterial)mi.GetSurfaceMaterial(0);
        mat.SetShaderParam("texture_albedo", uploaded_tex);
        mi.SetSurfaceMaterial(0, mat);
    }

    private void CloseButtonPressed(MarginContainer marginContainer)
    {
        marginContainer.QueueFree();
    }

    private class CustomButton : Button
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
            }
        }
    }
}
