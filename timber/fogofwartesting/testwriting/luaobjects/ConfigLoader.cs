using Godot;
using System;

using Godot;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Security.Permissions;
using System.Collections;
using System.Text.RegularExpressions;

//LuaLoader, but just actor configs.
public class ConfigLoader : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public static LoadSceneResult most_recent_load_scene_result = null;

    static ConfigLoader instance;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        instance = this;

        ArborCoroutine.StartCoroutine(Load(), this);
    }

    bool loading_scene = false;
    public static bool IsLoadingScene()
    {
        return instance.loading_scene;
    }
    IEnumerator Load()
    {
        loading_scene = true;

        /* Load game config */
        ArborResource.Load<GameConfig>("game.config");
        yield return ArborResource.WaitFor("game.config");
        GameConfig game_config = ArborResource.Get<GameConfig>("game.config");
        yield return LoadActorConfigs();
        loading_scene = false;

        //Test code:
        Spatial a = InstantiateActor('s', 0, 0);
        NLuaScriptManager nlsm = NLuaScriptManager.Instance;
        nlsm.CreateActor(NLuaScriptManager.testClassName, NLuaScriptManager.GenerateObjectName(), a);
    }

    Dictionary<char, ActorConfig> map_code_to_actor_config = new Dictionary<char, ActorConfig>();
    IEnumerator LoadActorConfigs()
    {
        ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        yield return ArborResource.WaitFor("mod_file_manifest.json");
        ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");

        List<string> actor_files = manifest.Search("actor_definitions/*");

        foreach (string actor_file in actor_files)
        {
            ArborResource.Load<ActorConfig>(actor_file);
        }

        foreach (string actor_file in actor_files)
        {
            yield return ArborResource.WaitFor(actor_file);
            ActorConfig actor_info = ArborResource.Get<ActorConfig>(actor_file);
            map_code_to_actor_config[actor_info.map_code] = actor_info;
            GD.Print($"Map Code: {actor_info.map_code}");
        }
    }

    public Spatial InstantiateActor(char current_char, int x, int z)
    {
        ActorConfig config = map_code_to_actor_config[current_char];
        Vector3 spawn_pos = new Vector3(x * Grid.tileWidth, 0, z * Grid.tileWidth);
        return SpawnActorOfType(config, spawn_pos);
    }

    Spatial SpawnActorOfType(ActorConfig config, Vector3 position)
    {
        /* Spawn actor scene */
        PackedScene actor_scene = (PackedScene)ResourceLoader.Load("res://fogofwartesting/testwriting/luaobjects/luaActor.tscn");
        Spatial new_actor = (Spatial)actor_scene.Instance();
        new_actor.Name = config.name;
        AddChild(new_actor);
        /* customize actor aesthetics */

        /* Load scripts of an actor */
        //Make new lua object

        LuaActor actor_script = new_actor as LuaActor;

        new_actor.GlobalTranslation = position;

        actor_script.Configure(config);

        /* customize actor aesthetics */

        /* Load scripts of an actor */
        foreach (string script_name in config.scripts)
        {
            string source_path = System.IO.Directory.GetCurrentDirectory() + @"\resources\scripts\" + script_name + ".gd";
            LoadScriptAtLocation(source_path, new_actor);
        }
        return actor_script;

    }

    void LoadScriptAtLocation(string location, Node owning_actor)
    {
        return;
        GD.Print("Attempting to load external file [" + location + "]");
        Script loaded_gdscript = (Script)GD.Load(location);

        Node new_script_node = new Node();
        new_script_node.SetScript(loaded_gdscript);

        //new_script_node.Call("_Ready");
        owning_actor.AddChild(new_script_node);
        new_script_node._Ready();
        new_script_node.SetProcess(true);
        GD.Print("Done attaching external script [" + location + "] to actor [" + owning_actor.Name + "]");
    }
}