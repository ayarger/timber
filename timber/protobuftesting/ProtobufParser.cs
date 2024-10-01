using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Google.Protobuf;
using Google.Protobuf.Message;
using System.Diagnostics;
using Amazon.Auth.AccessControlPolicy;
using Amazon.S3.Model;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Runtime.InteropServices;

public class ProtobufParser : Node
{

    static ProtobufParser instance;

    public override void _Ready()
    {
        base._Ready();
        instance = this;
    }

    // Goal: Trying to make ArborResource have the option to read binary files in a backward-compatible manner.
    //       And make calling these things as minimal as possible (don't change Load / Get in ArborResource)

    // Check how you load: LoadActorConfigs
    //      - Single function call that loads all actor files 
    //      - TODO: Maybe fix static configs ...

    // Ideas: 
    // OnRequestComplete determines how do we build these constructs
    //      Else:
    //          - GameConfig (game.config)
    //          - ActorConfig (Spot.Actor)
    //          - ModFileManifest (mod_file_manifest.json)
    //          - 
    //      STRING:
    //          - level1.layout
    //          - level1.config


    // Not a new <T>, but can't call OnRequestCompleted and SetResource the same way
    //      - SetResource(key, JsonConvert.DeserializeObject(s, Type.GetType(type), settings))



    // TODO: Add control logic to ArborResource.SetResources(), so they can handle the new <T> Type
    //else 
    //{
    //    string file_extension = System.IO.Path.GetExtension(resource).ToLower();
    //    if(file_extension == ".bin")
    //    {
    //        ParseBinary<>
    //    }
    //    else 
    //    {
    //        string s = Encoding.UTF8.GetString(body);
    //        JsonSerializerSettings settings = new JsonSerializerSettings();
    //        SetResource(key, JsonConvert.DeserializeObject(s, Type.GetType(type), settings));
    //    }
    //}

    public static T ParseBinary<T>(byte[] body, string resource, string type)
    {
        T output;    

        if(type == "string")
        {
            var protobufData = Google.Protobuf.Message.SimpleString.Parser.ParseFrom(body);
            output = (T)(object) protobufData.ToString();  
            GD.Print(output);
        }
        else
        {
            string file_extension = System.IO.Path.GetExtension(resource).ToLower();

            // At some point these will likely change. 
            if (file_extension == ".Actor")
            {
                var protoActor = Google.Protobuf.Message.GameActor.Parser.ParseFrom(body);

                List<string> attachedScripts = new List<string>();
                foreach (string item in protoActor.Scripts)
                {
                    attachedScripts.Append(item);
                }
                GD.Print(attachedScripts);

                ActorConfig localActor = new ActorConfig
                {
                    guid = Guid.Parse(protoActor.Guid),
                    name = protoActor.Name,
                    team = protoActor.Team,
                    map_code = protoActor.MapCode[0],
                    aesthetic_scale_factor = protoActor.AestheticScaleFactor,
                    idle_sprite_filename = protoActor.IdleSpriteFilename,
                    lives_sprite_filename = protoActor.LivesIconFilename,
                    scripts = attachedScripts
                };
                output = (T)(object)localActor;
            }
            else if (file_extension == ".Config")
            {
                var gameConfig = Google.Protobuf.Message.GameConfigData.Parser.ParseFrom(body);

                GameConfig localConfig = new GameConfig
                {
                    name = gameConfig.Name,
                    title_screen_background_image = gameConfig.TitleScreenBackgroundImage,
                    title_screen_logo_image = gameConfig.TitleScreenLogoImage,
                    initial_scene_file = gameConfig.InitialSceneFile,
                    gameover_image = gameConfig.GameoverImage,
                    initial_continue_count = gameConfig.InitialContinueCount,
                    cursor_image = gameConfig.CursorImage
                };
                output = (T)(object)localConfig;
            }
            else if (file_extension == ".JSON")
            {
                var modFile = Google.Protobuf.Message.ModFiles.Parser.ParseFrom(body);
                List<string> list = new List<string>();

                foreach (string item in modFile.Files)
                {
                    list.Append(item);
                }

                ModFileManifest modFilesArray = new ModFileManifest
                {
                    mod_files = list
                };

                output = (T)(object)modFilesArray;
            }
            else if (file_extension == ".CombatConfig")
            {
                var combatConfig = Google.Protobuf.Message.ActorCombatConfig.Parser.ParseFrom(body);

                CombatConfig localConfig = new CombatConfig
                {
                    name = combatConfig.StateName,
                    attackRange = combatConfig.AttackRange,
                    attackDamage = combatConfig.AttackDamage,
                    criticalHitRate = combatConfig.CriticalHitRate,
                    attackCooldown = combatConfig.AttackCooldown,
                    attackRecovery = combatConfig.AttackRecovery,
                    attackWindup = combatConfig.AttackWindup
                };

                output = (T)(object)localConfig;
            }
            else
            {
                output = (T)(object)"";
            }
        }
        return output;
    }

    // Checks if we are grabbing a binary file off AWS
    public bool isBinaryFile(string filename)
    {
        string format = filename.Substring(filename.Length() - 4);

        if (format != ".bin" || filename.Length > 4)
        {
            GD.Print("Not a binary file");
            return false;
        }
        return true;
    }

    // If we want to rebuild an old proto file
    void buildProtoFile(string filename)
    {
        string filepath = "./protoc-25/bin/protoc --csharp_out=:. " + filename + ".proto";
        // string filepath = "./../protoc-25/bin/protoc --csharp_out=:. " + filename + ".proto";  if in protobuftesting folder


        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe", // Use "bash" for Linux/macOS
                Arguments = ("/C " + filepath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        GD.Print(result);
    }


    // CREATING BINARY FILES GIVEN 
    public static void BinaryToFile(byte[] data, string name)
    {
        string level = "./temp_data/";   // TODO: Which folder should we store uploaded data? Temp data?
        string filepath = name + ".bin";
        System.IO.File.WriteAllBytes(filepath, data);
    }


    public static void CreateActorBinary(ActorConfig gameObject, string name)
    {
        string mapCodeString = gameObject.map_code.ToString();

        GameActor localActor = new GameActor
        {
            Guid = gameObject.guid.ToString(),
            Name = gameObject.name,
            Team = gameObject.team,
            MapCode = mapCodeString,
            AestheticScaleFactor = gameObject.aesthetic_scale_factor,
            IdleSpriteFilename = gameObject.idle_sprite_filename,
            LivesIconFilename = gameObject.lives_sprite_filename
        };

        foreach(string script in gameObject.scripts)
        {
            localActor.Scripts.Add(script);
        }

        BinaryToFile(localActor.ToByteArray(), name);
    }

    public static void CreateGameConfigBinary(GameConfig gameObject, string name)
    {
        GameConfigData localGameConfig = new GameConfigData
        {
            Name = gameObject.name,
            TitleScreenBackgroundImage = gameObject.title_screen_background_image,
            TitleScreenLogoImage = gameObject.title_screen_logo_image,
            InitialSceneFile = gameObject.initial_scene_file,
            GameoverImage = gameObject.gameover_image,
            InitialContinueCount = gameObject.initial_continue_count,
            CursorImage = gameObject.cursor_image
        };

        BinaryToFile(localGameConfig.ToByteArray(), name);
    }

    public static void CreateModFileBinary(ModFileManifest gameObject, string name)
    {
        ModFiles localMods = new ModFiles();

        foreach (string mod in gameObject.mod_files)
        {
            localMods.Files.Append(mod);
        }

        BinaryToFile(localMods.ToByteArray(), name);
    }

    public static void CreateCombatConfig(CombatConfig gameObject, string name)
    {
        string state = gameObject.name;

        ActorCombatConfig localCombatConfig = new ActorCombatConfig
        {
            AttackRange = gameObject.attackRange,
            AttackDamage = gameObject.attackDamage,
            CriticalHitRate = gameObject.criticalHitRate,
            AttackWindup = gameObject.attackWindup,
            AttackRecovery = gameObject.attackRecovery,
            AttackCooldown = gameObject.attackCooldown
        };

        BinaryToFile(localCombatConfig.ToByteArray(), name);
    }


    // TODO: Need to change how we store map data if we want to serialize
    public static void CreateMapBinary(GameConfig gameObject, string name)
    {

    }

    // TODO: Should players be able to make protobuf files?
    void makeProtoFile()
    {

    }


    // TODO: Unfinished Functions
    // TODO: Maybe there is a future we want users to easily write json files, but store them as binaries in the cloud?
    public void SerializeJSONFile(string filename, string filepath)
    {
        // Read in JSON file

        // Make Protobuf Object
        Google.Protobuf.Message.GameActor cuff = new Google.Protobuf.Message.GameActor();

        // Serialize to Binary
        byte[] cuffData = cuff.ToByteArray();

        // Dump Data
        System.IO.File.WriteAllBytes("./protobuftesting/cuff.bin", cuffData);

    }


    // Maybe instead of we use a custom Binary return format? 
    [Serializable]
    public class BinaryData
    {
        public string type;

        public byte[] Data;
    }
}
