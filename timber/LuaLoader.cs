using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Amazon.Auth.AccessControlPolicy;

public class LuaLoader : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public static LoadSceneResult most_recent_load_scene_result = null;

	public static LuaLoader instance { get; private set; }

    static Stopwatch timeline;

    //TEMP fake json example
    // StateConfig enemyMeleeCombatConfig = new StateConfig() { name = "MeleeCombatState", 
    // 	stateStats = { 
    // 		{ "attackRange", 1 }, 
    // 		{ "attackDamage", 20 }, 
    // 		{ "criticalHitRate", 0.5f }, 
    // 		{ "attackWindup", 0.5f }, 
    // 		{ "attackRecovery", 0.125f }, 
    // 		{ "attackCooldown", 1 } } };

    StatConfig enemyStatConfig = new StatConfig();
	StatConfig playerStatConfig = new StatConfig();

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;

        ArborCoroutine.StartCoroutine(Load(), this);

		timeline = new Stopwatch();
		timeline.Start();
	}

	bool loading_scene = false;
	public static bool IsLoadingScene()
	{
		return instance.loading_scene;
	}

	IEnumerator Load()
	{
		loading_scene = true;

        // JUSTIN: Test to work with binries
        //ArborResource.Load<GameConfig>("game.config.bin");
        //yield return ArborResource.WaitFor("game.config.bin");
        //GameConfig game_config = ArborResource.Get<GameConfig>("game.config.bin");

        /* Load game config */
        TimeSpan elapsedSnapshot = timeline.Elapsed;
        GD.Print("Loading scene: " + elapsedSnapshot);

        ArborResource.Load<GameConfig>("game.config");
		yield return ArborResource.WaitFor("game.config");
		GameConfig game_config = ArborResource.Get<GameConfig>("game.config");

		//yield return LoadActorConfigs();
        yield return LoadScene(game_config.initial_scene_file);

        elapsedSnapshot = timeline.Elapsed;
        GD.Print("Scene loaded at " + elapsedSnapshot);

        loading_scene = false;
    }

	Dictionary<char, ActorConfig> map_code_to_actor_config = new Dictionary<char, ActorConfig>();
    IEnumerator LoadActorConfigs()
    {
		// JUSTIN: Test to work with binaries
		ArborResource.Load<ModFileManifest>("binary_mod_file_manifest.bin");
		yield return ArborResource.WaitFor("binary_mod_file_manifest.bin");
		ModFileManifest manifest = ArborResource.Get<ModFileManifest>("binary_mod_file_manifest.bin");

		//ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
		//yield return ArborResource.WaitFor("mod_file_manifest.json");
		//ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");

		List<string> actor_files = manifest.Search("actor_definitions/*").Select((file) =>
        {
            return file.name;
        }).ToList();

        StateProcessor.Initialize();

		foreach (string actor_file in actor_files)
        {
			ArborResource.Load<ActorConfig>(actor_file);
		}

		foreach(string actor_file in actor_files)
		{
			GD.Print("Loading actor file: " + actor_file);
			yield return ArborResource.WaitFor(actor_file);

            ActorConfig actor_info = ArborResource.Get<ActorConfig>(actor_file);

			//temporary
			playerStatConfig.stats["health"] = 100;

			enemyStatConfig.stats["health"] = 50;
			
			if (actor_info.team == "player")
            {
				//actor_info.stateConfigs.Add(playerCombatConfig);
				actor_info.statConfig = playerStatConfig;
            }
			else if (actor_info.team=="construction")
			{
				actor_info.statConfig = playerStatConfig;
			}
            else if(actor_info.name=="Chunk")
            {
				actor_info.statConfig = enemyStatConfig;
				
            }
            else
            {
				actor_info.statConfig = enemyStatConfig;
				ArborResource.Load<Texture>("public/images/cheese.png");
			}
			
            map_code_to_actor_config[actor_info.map_code] = actor_info;
        }
	}

    IEnumerator LoadScene (string scene_filename)
	{
        TimeSpan elapsedSnapshot = timeline.Elapsed;
        GD.Print("! Load Scene: " + scene_filename + " at " + elapsedSnapshot);

        ArborResource.Load<ModFileManifest>("mod_file_manifest.json");
        yield return ArborResource.WaitFor("mod_file_manifest.json");
        ModFileManifest manifest = ArborResource.Get<ModFileManifest>("mod_file_manifest.json");

        List<string> actor_files = manifest.Search("actor_definitions/*").Select((file) =>
        {
            return file.name;
        }).ToList();

        StateProcessor.Initialize();

        foreach (string actor_file in actor_files)
        {
            ArborResource.Load<ActorConfig>(actor_file);
        }

        ViewportTexture fog_of_war_visibility_texture = new ViewportTexture();

        string image_filename = "images/" + scene_filename + ".png";
		ArborResource.Load<Texture>(image_filename);

        string layout_filename = "scenes/" + scene_filename + ".layout";
        ArborResource.Load<string>(layout_filename);

        string config_filename = "scenes/" + scene_filename + ".config";
        ArborResource.Load<string>(config_filename);

		/* Load Audio */
		ArborResource.UseResource(
			"public/sounds/pre_battle.ogg",
			(AudioStream audio) =>
			{
				ArborAudioManager.RequestBGM(audio);
			},
			this
		);

		ArborResource.Load<AudioStream>("public/sounds/bgm_btd_defeat.ogg");

        //yield return ArborResource.WaitFor("sounds/bgm_btd_defeat.ogg");

        //yield return ArborResource.WaitFor(image_filename);
        //yield return ArborResource.WaitFor(layout_filename);
        //yield return ArborResource.WaitFor(config_filename);


        // JUSTIN: Image is the largest file, should limit time by largest constraining download. 
        // JUSTIN: A little unsafe. Works fine under testing.
        yield return ArborResource.WaitFor(image_filename);

        foreach (string actor_file in actor_files)
        {
            //GD.Print("Loading actor file: " + actor_file);

            //TODO: Remove this when actor protobufs are updated

            string[] parts = actor_file.Split('.');
            string extension = (parts.Length > 1) ? "." + parts[parts.Length - 1] : string.Empty;
            if (extension == ".bin")
			{
				continue;
			}

			// JUSTIN: No need with previous yield return.
            yield return ArborResource.WaitFor(actor_file);

            ActorConfig actor_info = ArborResource.Get<ActorConfig>(actor_file);
            //if (actor_info.stateConfigs.Count > 0)
            //    GD.Print(actor_info.stateConfigs.Count);

            //temporary
            playerStatConfig.stats["health"] = 100;

            enemyStatConfig.stats["health"] = 50;

            if (actor_info.team == "player")
            {
                //actor_info.stateConfigs.Add(playerCombatConfig);
                actor_info.statConfig = playerStatConfig;
            }
            else if (actor_info.team == "construction")
            {
                actor_info.statConfig = playerStatConfig;
            }
            else if (actor_info.name == "Chunk")
            {
                actor_info.statConfig = enemyStatConfig;

            }
            else
            {
                actor_info.statConfig = enemyStatConfig;
                ArborResource.Load<Texture>("public/images/cheese.png");
            }

            map_code_to_actor_config[actor_info.map_code] = actor_info;
        }

        elapsedSnapshot = timeline.Elapsed;
        GD.Print("Actors loaded at " + elapsedSnapshot);


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
			}

			x++;
        }
		tileData.RemoveAt(tileData.Count - 1);
		var tempTileData = new List<TileData[]>();
		foreach (var td in tileData) tempTileData.Add(td.ToArray());
		Grid.tiledata = tempTileData.ToArray();

		EventBus.Publish(new TileDataLoadedEvent());
		height = z;

        elapsedSnapshot = timeline.Elapsed;
		GD.Print("Map loaded at " + elapsedSnapshot);

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

		//yield return null;
		//yield return null;

		//Deprecated
		//fog_of_war_visibility_texture = new ViewportTexture();

		//fog_of_war_visibility_texture.ViewportPath = viewport.GetPath();

        //yield return null;
        //yield return null;


        Vector2 new_marker_pos = new Vector2(new_marker_x, new_marker_y);
        GD.Print("NEW MARKER POS " + new_marker_pos.x + " " + new_marker_pos.y);
        
        //GD.Print("fog_of_war_visibility_texture.size() " + fog_of_war_visibility_texture.GetSize());


        Sprite visibility_marker = GetParent().GetNode<Sprite>("FogOfWar/HighVisibility/Sprite");
		visibility_marker.Position = new_marker_pos;

		//yield return null;
		//yield return null;
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

        elapsedSnapshot = timeline.Elapsed;
		GD.Print("Fog of war loaded at " + elapsedSnapshot);
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
        foreach (ScriptConfig script in config.scripts)
        {
			LoadScriptAtLocation(script, new_actor);
		}
		return actor_script;
	}

	void LoadScriptAtLocation(ScriptConfig scriptConfig, Spatial owning_actor)
    {
        string source_path = System.IO.Directory.GetCurrentDirectory() + @"\resources\scripts\" + scriptConfig.name;
        NLuaScriptManager.Instance
                .CreateActor(scriptConfig.name, NLuaScriptManager.GenerateObjectName(), owning_actor);
       
        return;
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
	public string type = "actor";

	public Dictionary<string, string> sprite_filenames = new Dictionary<string, string>();

	public List<ScriptConfig> scripts; //For now, contains at most one script.

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
public class ModFile
{
	public string name;
	public long size;
    [JsonProperty] private string last_modified;
	public string data_type; //actor, dialogue, image, scene, script, sound

	//All times are UTC
	public DateTime GetLastModifiedTime() 
	{
        return DateTime.ParseExact(last_modified, "yyyy-MM-dd HH:mm:ss",
                                   System.Globalization.CultureInfo.InvariantCulture);
    }

	public void WriteLastModifiedTime(DateTime time)
	{
		last_modified = time.ToString("yyyy-MM-dd HH:mm:ss",
								   System.Globalization.CultureInfo.InvariantCulture);
	}
}

[Serializable]
public class ModFileManifest
{
	public List<ModFile> mod_files = new List<ModFile>();

    public List<ModFile> Search (string pattern)
    {

        List<ModFile> matchingFiles = new List<ModFile>();
        Regex regex = new Regex(pattern);

        foreach (ModFile mf in mod_files)
        {
            if (regex.IsMatch(mf.name))
            {
                matchingFiles.Add(mf);
            }
        }

        return matchingFiles;
    }
}

//May move elsewhere
public class TileDataLoadedEvent{ }


//change into different stat in hasStat
public class StateConfig
{
	public string name;
	public Dictionary<string,float> stateStats = new Dictionary<string, float>();
}

//state configs for actors
//Stats should be moved to stat manager
public class CombatConfig : StateConfig
{
	public CombatConfig(string n, int ar = 1, int damage = 10, float critRate = 0.5f, float windup = 0.5f, float recovery = 0.125f, float cooldown = 1f)
	{
		name = n;
		stateStats = new Dictionary<string, float>
		{
			{ "attackRange", ar },
			{ "attackDamage", damage },
			{ "criticalHitRate", critRate },
			{ "attackWindup", windup },
			{ "attackRecovery", recovery },
			{ "attackCooldown", cooldown }
		};
	}
}


public class StatConfig
{
	public Dictionary<string, float> stats = new Dictionary<string, float>();
}


[Serializable]
public class ScriptConfig
{
	public string name;
	public Dictionary<string, object> variables;
}

