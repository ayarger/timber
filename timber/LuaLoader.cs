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

public class LuaLoader : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public static LoadSceneResult most_recent_load_scene_result = null;

	static LuaLoader instance;

	//temporary fake json
	CombatConfig enemyMeleeCombatConfig = new CombatConfig(1, 10, 0.5f, 0.5f, 0.125f, 1);
	CombatConfig enemyRangeCombatConfig = new CombatConfig(3, 5, 0.3f, 0.75f, 0.125f, 1.5f);
	CombatConfig playerCombatConfig = new CombatConfig(2, 20, 0.3f, 0.5f, 0.125f, 0.75f);
	StatConfig enemyStatConfig = new StatConfig();
	StatConfig playerStatConfig = new StatConfig();


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
        yield return LoadScene(game_config.initial_scene_file);

        loading_scene = false;
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

		foreach(string actor_file in actor_files)
		{
			yield return ArborResource.WaitFor(actor_file);
            ActorConfig actor_info = ArborResource.Get<ActorConfig>(actor_file);

			//temporary
			playerStatConfig.stats["health"] = 100;

			enemyStatConfig.stats["health"] = 50;
			
			if (actor_info.team == "player")
            {
				actor_info.stateConfigs.Add(playerCombatConfig);
				actor_info.statConfig = playerStatConfig;
            }
            else if(actor_info.name=="Chunk")
            {
				actor_info.stateConfigs.Add(enemyMeleeCombatConfig);
				actor_info.statConfig = enemyStatConfig;
            }
            else
            {
				actor_info.stateConfigs.Add(enemyRangeCombatConfig);
				actor_info.statConfig = enemyStatConfig;
			}
			

            map_code_to_actor_config[actor_info.map_code] = actor_info;
        }
	}

    IEnumerator LoadScene (string scene_filename)
	{
		ViewportTexture fog_of_war_visibility_texture = new ViewportTexture();

        string image_filename = "images/" + scene_filename + ".png";
		ArborResource.Load<Texture>(image_filename);

        string layout_filename = "scenes/" + scene_filename + ".layout";
        ArborResource.Load<string>(layout_filename);

        string config_filename = "scenes/" + scene_filename + ".config";
        ArborResource.Load<string>(config_filename);

		/* Load Audio */
		ArborResource.UseResource(
			"sounds/pre_battle.ogg",
			(AudioStream audio) =>
			{
				ArborAudioManager.RequestBGM(audio);
			},
			this
		);

		ArborResource.Load<AudioStream>("sounds/bgm_btd_defeat.ogg");
		yield return ArborResource.WaitFor("sounds/bgm_btd_defeat.ogg");

        yield return ArborResource.WaitFor(image_filename);
        yield return ArborResource.WaitFor(layout_filename);
        yield return ArborResource.WaitFor(config_filename);

        Texture scene_tile_texture = ArborResource.Get<Texture>(image_filename);
        string layout_file_contents = ArborResource.Get<string>(layout_filename);
        string config_file_contents = ArborResource.Get<string>(config_filename);

        List<Vector3> player_actor_spawn_positions = new List<Vector3>();

		List<ShaderMaterial> tile_mats = new List<ShaderMaterial>();

		int width = 0;
		int height = 0;
		int x = 0;
		int z = 0;
		//May move tile data elsewhere
		//Possibly load tile width
		var tileData = new List<List<TileData>>();
		tileData.Add(new List<TileData>());
		for(int i = 0; i < layout_file_contents.Length; i++)
        {
			char current_char = layout_file_contents[i];
			char next_char = ' ';
			if (i < layout_file_contents.Length - 1)
				next_char = layout_file_contents[i + 1];

			/* special cases */
			if(current_char == '\n')
            {
				tileData.Add(new List<TileData>());
				width = x;
                x = 0;
				z++;
				continue;
            }

			if(current_char == '\r')
            {
				continue;
            }

			if (current_char == 'e')
            {
				tileData[z].Add(new TileData('e',new Coord(x,z)));
            }
			else
            {
                tileData[z].Add(new TileData('.', new Coord(x, z)));
                PackedScene tile_scene = (PackedScene)ResourceLoader.Load("res://scenes/Tile.tscn");
				CSGBox new_tile = (CSGBox)tile_scene.Instance();
                ShaderMaterial mat = (ShaderMaterial)new_tile.Material;
				tile_mats.Add(mat);

                mat.SetShaderParam("albedo_texture", scene_tile_texture);
				mat.SetShaderParam("visibility_texture", fog_of_war_visibility_texture);
				AddChild(new_tile);
				new_tile.GlobalTranslation = new Vector3(x * Grid.tileWidth, -1, z * Grid.tileWidth);
			}

			if(map_code_to_actor_config.ContainsKey(current_char))
            {
				ActorConfig config = map_code_to_actor_config[current_char];
				Vector3 spawn_pos = new Vector3(x * Grid.tileWidth, 0, z * Grid.tileWidth);
				Actor new_actor = SpawnActorOfType(config, spawn_pos);

				if (config.team.ToLower().Trim() == "player")
					player_actor_spawn_positions.Add(spawn_pos);

				//Test Code:
				if(current_char == 'm' || current_char == 'q')
				{
                    NLuaScriptManager.Instance
						.CreateActor(NLuaScriptManager.testClassName, NLuaScriptManager.GenerateObjectName(), new_actor);
                }
			}

			x++;
        }
		tileData.RemoveAt(tileData.Count - 1);
		var tempTileData = new List<TileData[]>();
		foreach (var td in tileData) tempTileData.Add(td.ToArray());
		Grid.tiledata = tempTileData.ToArray();

		EventBus.Publish(new TileDataLoadedEvent());
		height = z;

		/* Configure fog of war */
		Viewport viewport = GetParent().GetNode<Viewport>("FogOfWar/HighVisibility");
		

		//viewport.RenderTargetClearMode = Godot.Viewport.ClearMode.Never;
        float pixels_per_tile = 10;
		//viewport.Size = new Vector2(width, height);
		//viewport.Size = new Vector2(1000, 1000);
		//GD.Print("setting viewport to " + width + " " + height);
		Spatial player_node = GetNode<Spatial>("Spot");
        GD.Print("PLAYER AT " + player_node.GlobalTranslation.x + " " + player_node.GlobalTranslation.z);

        float new_marker_x = viewport.Size.x * (player_node.GlobalTranslation.x * 0.5f / width);
		float new_marker_y = viewport.Size.y * (player_node.GlobalTranslation.z * 0.5f / height);

		yield return null;
		yield return null;

		//Deprecated
		//fog_of_war_visibility_texture = new ViewportTexture();

		//fog_of_war_visibility_texture.ViewportPath = viewport.GetPath();

        yield return null;
        yield return null;


        Vector2 new_marker_pos = new Vector2(new_marker_x, new_marker_y);
        GD.Print("NEW MARKER POS " + new_marker_pos.x + " " + new_marker_pos.y);
        
        //GD.Print("fog_of_war_visibility_texture.size() " + fog_of_war_visibility_texture.GetSize());


        Sprite visibility_marker = GetParent().GetNode<Sprite>("FogOfWar/HighVisibility/Sprite");
		visibility_marker.Position = new_marker_pos;

		yield return null;
		yield return null;
		//I think you need to set the param every frame? Done in FogOfWar.cs
		//foreach(ShaderMaterial mat in tile_mats)
		//{
		//	mat.SetShaderParam("visibility_texture", fog_of_war_visibility_texture);
		//}

        /* Finish up */
        Vector3 avg_pos = Vector3.Zero;
		foreach (Vector3 pos in player_actor_spawn_positions)
			avg_pos += pos;
		avg_pos /= player_actor_spawn_positions.Count;
        most_recent_load_scene_result = new LoadSceneResult() { average_position_of_player_actors = avg_pos };
	}


	public Actor SpawnActorOfType(ActorConfig config, Vector3 position)
    {
		/* Spawn actor scene */
		PackedScene actor_scene = (PackedScene)ResourceLoader.Load("res://scenes/actor.tscn");
		Spatial new_actor = (Spatial)actor_scene.Instance();
		new_actor.Name = config.name;
		AddChild(new_actor);

		Actor actor_script = new_actor as Actor;

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

[Serializable]
public class ActorConfig
{
	public Guid guid;
	public string name = "???";
	public string team = "player";
	public char map_code = '?';
	public float aesthetic_scale_factor = 1.0f;
	public string idle_sprite_filename;
	public string lives_sprite_filename;
	public string pre_ko_sprite_filename = "";
	public string ko_sprite_filename = "";

	public List<string> scripts;

	public List<StateConfig> stateConfigs = new List<StateConfig>();
	public StatConfig statConfig;
}

[Serializable]
public class ScriptInfo
{
	public string name;
}

public class LoadSceneResult
{
	public Vector3 average_position_of_player_actors = Vector3.Zero;
}

[Serializable]
public class SceneConfig
{
	public string name = null;
	public List<string> basic_background_tracks = new List<string>();
    public List<string> combat_background_tracks = new List<string>();
	public string intro_text_scroll = null;
	public string success_image = null;
	public string failure_image = null;
	public string subsequent_scene_file = null;
}

[Serializable]
public class GameConfig
{
	public string name = null;
	public string title_screen_background_image = null;
	public string title_screen_logo_image = null;
	public string initial_scene_file = null;
	public string gameover_image = null;
	public int initial_continue_count = 3;
	public string cursor_image = null;
}

[Serializable]
public class ModFileManifest
{
	public List<string> mod_files = new List<string>();

    public List<string> Search (string pattern)
    {
        List<string> matchingStrings = new List<string>();
        Regex regex = new Regex(pattern);

        foreach (string str in mod_files)
        {
            if (regex.IsMatch(str))
            {
                matchingStrings.Add(str);
            }
        }

        return matchingStrings;
    }
}

//May move elsewhere
public class TileDataLoadedEvent{ }


//change into different stat in hasStat
public class StateConfig
{
	public string name;
}

//state configs for actors
public class CombatConfig : StateConfig
{ 
	public int attackRange = 2;//number of grids
	public int attackDamage = 10;
	public float criticalHitRate = 0.3f;

	public float attackWindup = 0.5f;//animation before attack
	public float attackRecovery = 0.125f;//anim after attack
	public float attackCooldown = 1f;

	public CombatConfig(int ar, int damage, float critRate, float windup, float recovery, float cooldown)//temp constructor
    {
		name = "CombatState";
		attackRange = ar;
		attackDamage = damage;
		criticalHitRate = critRate;
		attackWindup = windup;
		attackRecovery = recovery;
		attackCooldown = cooldown;
    }
}

public class StatConfig
{
	public Dictionary<string, float> stats = new Dictionary<string, float>();
}

