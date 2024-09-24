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

    // Goal: Trying to make ArborResource have the option to use binary in a backward-compatible manner.
    //       And make calling these things as minimal as possible (not change Load / Get in ArborResource)

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

    // TODO: Not exactly sure where to insert the binary parsing methods

    public static T ParseBinary<T>(byte[] body, string resource, string type)
    {
        T output;    

        if(type == "string")
        {
            // TODO: Rebuild .proto again
            var protobufData = Google.Protobuf.Message.SimpleString.Parser.ParseFrom(body);
            output = protobufData.ToString();
            GD.Print(output);
        }
            
        else
        {
            string file_extension = System.IO.Path.GetExtension(resource).ToLower();
            
            // At some point these will likely change. 
            if(file_extension == ".Actor")
            {
                var protoActor = Google.Protobuf.Message.GameActor.Parser.ParseFrom(body);

                string[] attachedScripts = Array.Empty<string>();
                foreach(string item in protoActor.Scripts)
                {
                    attachedScripts.Append(item);
                }
                GD.Print(attachedScripts);

                // I think you can use Handlers but it's not appearing for v3 proto
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

                output = localActor;
            }
            else if (file_extension == ".Config")
            {
                var Config = Google.Protobuf.Message.GameConfig.Parser.ParseFrom(body);
            }
            else if (file_extension == ".JSON")
            {
                var ModFile = Google.Protobuf.Message.ModFiles.Parser.ParseFrom(body);
            }


            output = "1";
        }

        return output;
    }

    // Handles the web process of grabbing a binary file off AWS
    public void LoadBinary(string filename)
    {
        string format = filename.Substring(filename.Length() - 4);

        if (format != ".bin" || filename.Length > 4)
        {
            GD.Print("Not a binary file");
            return;
        }


    }

    public void JSONToBinary<T>(string fileName)
    {

    }

    public void readBinary()
	{
        // Cuff
        byte[] cuffData = System.IO.File.ReadAllBytes("./protobuftesting/cuff.bin");
        var cuff = Google.Protobuf.Message.GameActor.Parser.ParseFrom(cuffData);
    }

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

    void generateProtocFile(string name)
    {
        string command = "./protoc-25/bin/protoc --csharp_out=:. " + name + ".proto";   // e.x. protofile = Messages.proto
        string arguments = "--csharp_out=:. Messages.proto";

        Process process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally { 
            process.Dispose(); 
        }
    }

    void makeProtoFile()
    {
        // Should players be able to make protobuf files?
        // If they can't, then they can only use the default data structures we make
        // We also have timestamp capabilities now, what can we do with that

        // Next week: Get AWS perms ready so I can try to get WebBuildUploader.cs ready? I haven't tried using it. 
    }


    // Maybe instead of we use a custom Binary return format? this way we don't need to change load
    [Serializable]
    public class BinaryData
    {
        public string type;

        public byte[] Data;
    }
}
