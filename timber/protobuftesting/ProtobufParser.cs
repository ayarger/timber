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
using Newtonsoft.Json.Linq;


//      Else:
//          - GameConfig (game.config)
//          - ActorConfig (Spot.Actor)
//          - ModFileManifest (mod_file_manifest.json)
//          - 
//      STRING:
//          - level1.layout
//          - level1.config


public class ProtobufParser : Node
{

    static ProtobufParser instance;

    public override void _Ready()
    {
        base._Ready();
        instance = this;
    }

    public static T ParseBinary<T>(byte[] body, string type)
    {
        T output;

        if (type == "string")
        {
            var protobufData = Google.Protobuf.Message.SimpleString.Parser.ParseFrom(body);
            output = (T)(object)protobufData.ToString();
        }
        else if (type == "ActorConfig")
        {
            var protoActor = Google.Protobuf.Message.GameActor.Parser.ParseFrom(body);

                List<string> attachedScripts = new List<string>();
                foreach (string item in protoActor.Scripts)
                {
                    attachedScripts.Add(item);
                }

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
        else if (type == "ModFileManifest")
        {
            var protoModFiles = Google.Protobuf.Message.ModFiles.Parser.ParseFrom(body);

            List<string> stored_mod_files = new List<string>();
            foreach (string item in protoModFiles.Files)
            {
                stored_mod_files.Add(item);
            }

            ModFileManifest localModFileManifest = new ModFileManifest()
            {
                mod_files = stored_mod_files
            };
            output = (T)(object)localModFileManifest;
        }
        else if (type == "GameConfig")
        {
            var protoConfig = Google.Protobuf.Message.GameConfig.Parser.ParseFrom(body);

            GameConfig localConfig = new GameConfig
            {
                name = protoConfig.Name,
                title_screen_background_image = protoConfig.TitleBackground,
                title_screen_logo_image = protoConfig.TitleLogo,
                initial_scene_file = protoConfig.InitialScene,
                gameover_image = protoConfig.GameoverImg,
                initial_continue_count = protoConfig.InitialContinueCount,
                cursor_image = protoConfig.CursorImage
            };
            output = (T)(object)localConfig;
        }
        else
        {
            var protoActor = Google.Protobuf.Message.SimpleString.Parser.ParseFrom(body);
            string localString = protoActor.Message;
            output = (T)(object)localString;
        }

        return output;
    }



    // Check if we are grabbing a binary file off AWS
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

    // If we want to rebuild a .proto file
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
    }

    public static void BinaryToFile(byte[] data, string name)
    {
        string filepath = "./protobuftesting/" + name + ".bin";
        System.IO.File.WriteAllBytes(filepath, data);
    }

    // Converting JSON to Binary
    private void ActorJSONtoBinary(string fileName)
    {
        string filepath = "./protobuftesting/" + fileName;
        string jsonContent = System.IO.File.ReadAllText(filepath);

        JObject jsonObj = JObject.Parse(jsonContent);

        GameActor actor = new GameActor
        {
            Guid = jsonObj["guid"]?.ToString(),
            Name = jsonObj["name"]?.ToString(),
            Team = jsonObj["team"]?.ToString(),
            MapCode = jsonObj["map_code"]?.ToString(),
            AestheticScaleFactor = jsonObj["aesthetic_scale_factor"]?.ToObject<float>() ?? 1.0f,
            IdleSpriteFilename = jsonObj["idle_sprite_filename"]?.ToString(),
            LivesIconFilename = jsonObj["lives_icon_filename"]?.ToString(),
        };

        JArray scriptsArray = (JArray)jsonObj["scripts"];
        if (scriptsArray != null)
        {
            foreach (var script in scriptsArray)
            {
                actor.Scripts.Add(script.ToString());
            }
        }

        string binaryName = "./protobuftesting/" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".bin";

        using (FileStream output = System.IO.File.Create(binaryName))
        {
            actor.WriteTo(output); 
        }
    }

    private void ModManifestJSONtoBinary(string fileName)
    {
        string filepath = "./protobuftesting/" + fileName;
        string jsonContent = System.IO.File.ReadAllText(filepath);

        JObject jsonObj = JObject.Parse(jsonContent);

        ModFiles mods = new ModFiles();

        JArray scriptsArray = (JArray)jsonObj["mod_files"];

        if (scriptsArray != null)
        {
            foreach (var script in scriptsArray)
            {
                mods.Files.Add(script.ToString());
            }
        }

        string binaryName = "./protobuftesting/" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".bin";

        using (FileStream output = System.IO.File.Create(binaryName))
        {
            mods.WriteTo(output);
        }
    }


    // Creating Binaries given Protobuf Class

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


    public static void CreateModFileBinary(ModFileManifest gameObject, string name)
    {
        ModFiles localMods = new ModFiles();

        foreach (string mod in gameObject.mod_files)
        {
            localMods.Files.Add(mod);
        }

        BinaryToFile(localMods.ToByteArray(), name);
    }
}
