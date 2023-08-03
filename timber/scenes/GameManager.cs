using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Amazon.S3.Model;

public class GameManager : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    Game current_game;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        return;

        /* check if saved game exists */
        string save_file_path = System.IO.Directory.GetCurrentDirectory() + @"\game_state.json";
        if (System.IO.File.Exists(save_file_path))
        {
            string json_rep = System.IO.File.ReadAllText(save_file_path);
            current_game = JsonConvert.DeserializeObject<Game>(json_rep);
        }
        else
        {
            /* Create new game */
            current_game = Game.Create();
            current_game.SpawnActor(new Guid("6ef3ddce-110e-4da6-aa59-1a8628d3f264"), new Vector3(5, 0, 5));
            current_game.SpawnActor(new Guid("d1d53e92-6956-4485-9081-3baa0731a34c"), new Vector3(3, 0, 5));
            for(int i = 0; i < 10000; i++)
            {
                current_game.SpawnActor(new Guid("d1d53e92-6956-4485-9081-3baa0731a34c"), new Vector3(3, 0, 5));
            }
        }
    }

    bool space_down = false;
    public override void _Process(float delta)
    {
        if (current_game == null)
            return;

        current_game.Update(delta);
        if (Input.IsPhysicalKeyPressed((int)KeyList.Space) && !space_down)
        {
            space_down = true;
            string json_rep = JsonConvert.SerializeObject(current_game, Formatting.Indented);
            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + @"\game_state.json", json_rep);
        }
        else
        {
            space_down = false;
        }
    }
}

[Serializable]
public class Game // TODO : Implement MessagePack instead of json for serialization.
{
    public ulong frame = 0;
    public float time = 0.0f;
    public SortedDictionary<Guid, ActorConfig> actor_definitions = new SortedDictionary<Guid, ActorConfig>();
    public SortedDictionary<Guid, ActorInfo> actor_instances = new SortedDictionary<Guid, ActorInfo>();

    private Game() { }
    public static Game Create()
    {
        Game new_game = new Game();
        new_game.LoadActorDefinitions();
        
        return new_game;
    }

    public void LoadActorDefinitions()
    {


        string[] actor_files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\resources\actor_definitions", "*.actor");

        foreach (string actor_file in actor_files)
        {
            string actor_data = System.IO.File.ReadAllText(actor_file);
            GD.Print(actor_file);
            GD.Print(actor_data);
            ActorConfig actor_info = JsonConvert.DeserializeObject<ActorConfig>(actor_data);
            GD.Print("SUCCESS");

            actor_definitions[actor_info.guid] = actor_info;
        }
    }

    public ActorInfo SpawnActor(Guid _actor_definition_ref, Vector3 _position)
    {
        ActorInfo new_actor = ActorInfo.Create(_actor_definition_ref, _position);
        actor_instances[new_actor.guid] = new_actor;
        return new_actor;
    }

    public void Update(float delta_time)
    {
        frame++;
        time += delta_time;
        UpdateActors(delta_time);
    }

    void UpdateActors(float delta_time)
    {
        foreach(ActorInfo actor in actor_instances.Values)
        {
            actor.Update(delta_time);
        }
    }
}

[Serializable]
public class ActorInfo
{
    public Guid guid;
    public Guid definition_guid;

    public float health = 100.0f;
    public float experience = 0.0f;
    public List<string> items = new List<string>();
    public Vector3 position = Vector3.Zero;
    public float attack_cooldown_sec = 0.0f;

    public static ActorInfo Create(Guid _actor_definition_guid, Vector3 _position)
    {
        ActorInfo new_actor = new ActorInfo();
        new_actor.guid = Guid.NewGuid();
        new_actor.definition_guid = _actor_definition_guid;
        new_actor.position = _position;

        return new_actor;
    }

    public void Update(float delta_time)
    {
        position += Vector3.Right * delta_time;
    }
}
