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
    // TODO: Not exactly sure where to insert the binary parsing methods

    public void ParseBinary<T>(long result, long responseCode, string[] headers, byte[] body, string resource, string type, HTTPRequest request_used, string force_key_for_asset = null)
    {

        if(type == "string")
        {
            byte[] message = body;
            int lastDotIndex = resource.LastIndexOf('.');
            string fileType = resource.Substring(lastDotIndex + 1); 

            if(resource == "M")

            if (fileType == "Actor")
            {

            }

            var protobufData = Google.Protobuf.Message.GameActor.Parser.ParseFrom(body);
            string stringOutput = protobufData.ToString();
            GD.Print(stringOutput);
        }
            
        else
        {
            byte[] message = System.IO.File.ReadAllBytes("./protobuftesting/cuff.bin");
            var cuff = Google.Protobuf.Message.GameActor.Parser.ParseFrom(message);
        }
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
