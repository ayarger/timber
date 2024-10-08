/* https://docs.google.com/document/d/143EIk6GPQYmyi_rvcmdRf6JrtR310wKdd2p85ehELUE/edit?usp=sharing */

using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : Node
{
    public static UIManager instance;

    Panel flash_panel;
    Color flash_panel_invisible = new Color(1, 1, 1, 0);
    Color flash_panel_visible = new Color(1, 1, 1, 1);

    private static Stack<string> menuStack;

    public override void _Ready()
    {
        instance = this;
        flash_panel = GetNode<Panel>("flash_panel");
        flash_panel.MouseFilter = Control.MouseFilterEnum.Ignore;
        flash_panel.Modulate = new Color(1, 1, 1, 0.0f);
        ToastManager.Initialize(this);
        menuStack = new Stack<string>();
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

    DynamicMenu currMenu = null;

    public static void ClearAllMenus()
    {
        while(instance.currMenu != null)
        {
            instance.currMenu = instance.currMenu._parent;
            instance.currMenu.CloseMenu();    
        }
        menuStack.Clear();
    }

    public override void _Input(InputEvent @event)
    {
        // Check if the event is an InputEventKey
        if (@event is InputEventKey eventKey)
        {
            // Check if the escape key was just pressed
            if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
            {
                if (menuStack.Count == 0)
                {
                    SettingsMenu();
                }
                else
                {
                    // close the most recent menu window
                    instance.currMenu.CloseMenu();
                    menuStack.Pop();
                }
            }
        }
        ToastManager.HandleInput(@event);
    }

    void SettingsMenu()
    {
        menuStack.Push("settings");
        DynamicMenu.Configure(bgColor: new Color(41.0f / 255.0f, 41.0f / 255.0f, 69.0f / 255.0f, 1.0f),
            secondaryColor: new Color(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 1.0f),
            font: "res://UI/Oxanium-Regular.ttf",
            textColor: new Color(169.0f / 255.0f, 180.0f / 255.0f, 194.0f / 255.0f, 1.0f),
            buttonImg: "res://UI/default_tex.png",
            panelImg: "res://UI/default_tex.png");
        //List<string> options = new List<string>() { "Discord", "Credits2", "Quit", "debug_victory", "debug_lose", "debug_open_webgl_build", "debug_webgl_upload", "debug_webgl_upload_opt", "debug_change_title_screen", "debug_change_title_audio", "debug_test_image_upload", "debug_test_fullscreen" };
        List <System.Action> actions = new List<Action>()
        {
            () => { OS.ShellOpen(@"https://discord.gg/2GZ3SxVA7Q"); },
            () => {  },
            () => { GameOver.PerformGameOver(new GameOverRequest() {fast_mode = true}); },
            () => { VictoryScene.PerformVictory(); },
            () => { GetNode<Actor>("../Main/LuaLoader/Spot").Kill(); },
            () => { WebBuildUploader.OpenWebBuild(); },
            () => { WebBuildUploader.UploadWebBuild(); },
            () => { WebBuildUploader.UploadOptimizedWebBuild(); },
            () => { CanvasBuilder.PerformBuild(); },
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

        Toggle toggle = DynamicMenu.MenuToggle(true);
        toggle.SetOnValueChanged(() => { GD.Print("Toggling: " + toggle.selected); });
        Toggle toggle2 = DynamicMenu.MenuToggle();
        toggle2.SetOnValueChanged(() => { GD.Print("Toggling: " + toggle2.selected); });

        Control[] content = {
            DynamicMenu.MenuRow(
                DynamicMenu.MenuLabel("Volume"),
                DynamicMenu.MenuSlider(0, 100, ArborAudioManager.master_volume * 100, new Action<float>(UIManager.OnVolumeChanged)),
                DynamicMenu.MenuSpacer()
            ),
            //DynamicMenu.MenuButton("Discord", actions[0]),
            DynamicMenu.MenuButton("Discord", () => { OS.ShellOpen(@"https://discord.gg/2GZ3SxVA7Q"); }),
            DynamicMenu.MenuButton("Credits2", actions[1]),
            DynamicMenu.MenuButton("Quit", actions[2]),
            DynamicMenu.MenuButton("debug_victory", actions[3]),
            DynamicMenu.MenuButton("debug_lose", actions[4]),
            DynamicMenu.MenuButton("debug_open_webgl_build", actions[5]),
            DynamicMenu.MenuButton("debug_webgl_upload", actions[6]),
            DynamicMenu.MenuButton("debug_webgl_upload_opt", actions[7]),
            DynamicMenu.MenuButton("debug_change_title_screen", actions[8]),
            DynamicMenu.MenuButton("debug_change_title_audio", actions[9]),
            DynamicMenu.MenuButton("debug_test_image_upload", actions[10]),
            DynamicMenu.MenuButton("debug_test_fullscreen", actions[11]),
            DynamicMenu.MenuRow(
                DynamicMenu.MenuLabel("toggle"), toggle, DynamicMenu.MenuSpacer()
                
            ),
            DynamicMenu.MenuRow(
                DynamicMenu.MenuLabel("toggle2"), toggle2, DynamicMenu.MenuSpacer()

            ),

        };

        DynamicMenu settings;
        settings = new DynamicMenu(currMenu, content);
        currMenu = settings;
        settings.CreateMenu();
        GD.Print("Creating new menu");

        settings.SetOnMenuClose(() => { currMenu = settings._parent;  });
        settings.AddToHeader(
            DynamicMenu.MenuSpacer(),
            DynamicMenu.MenuLabel("Pause", 40),
            DynamicMenu.MenuSpacer()
            );
        settings.AddToggleGroup(true, false, toggle, toggle2);
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

    static void OnVolumeChanged(float volume)
    {
        ArborAudioManager.SetMasterVolume(volume);
    }
}
