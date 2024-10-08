using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using NAudio.Wave;
using NLayer;
using NLayer.NAudioSupport;
using System.Reflection;
using Amazon.Runtime.Internal.Transform;
using System.Runtime.CompilerServices;
using Amazon.Auth.AccessControlPolicy;

public class ArborResource : Node
{
    static Dictionary<string, string> web_query_parameters = new Dictionary<string, string>();

    static ArborResource instance;

    public override void _Ready()
    {
        base._Ready();

        instance = this;

        if (OS.GetName() == "Web" || OS.GetName() == "HTML5")
        {
            string web_query_string = (string)JavaScript.Eval("window.location.search");
            web_query_string = web_query_string.Substring(1, web_query_string.Length - 1);

            if (web_query_string != null && web_query_string != "")
            {
                GD.Print("Web parameters : ");
                string[] split_params = web_query_string.Split('=', '&');
                for (int i = 0; i < split_params.Length; i += 2)
                {
                    GD.Print(split_params[i] + " = " + split_params[i + 1]);
                    web_query_parameters[split_params[i]] = split_params[i + 1];
                }
            }

            InitializeFileDialogueWeb();
        }
    }

    public void OnCallbackAssociatedNodeExitTree(object destroyed_node)
    {
        GD.Print("NODE DESTROYED");

        /* A node has been destroyed, so let's go and unregister all of its callbacks */
        Node node = (Node)destroyed_node;
        GD.Print("node destroyed : [" + node.Name + "]");

        foreach(CallbackResourcePair callback_pair in node_to_associated_callbacks[node])
        {
            resource_usage_callbacks[callback_pair.resource].Remove(callback_pair.callback);
        }

        node_to_associated_callbacks.Remove(node);
    }

    Dictionary<string, object> loaded_assets = new Dictionary<string, object>();

    HashSet<string> assets_currently_loading = new HashSet<string>();
    public static bool NumberAssetsCurrentlyLoading() { return instance.assets_currently_loading.Count > 0; }

    void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body, string resource, string type, HTTPRequest request_used, string force_key_for_asset = null)
    {
        string key = resource;
        if (force_key_for_asset != null)
            key = force_key_for_asset;

        if (responseCode != 200)
        {
            GD.PrintErr("Failed to download resource [" + key + "] got error code [" + responseCode + "]");
            if (force_key_for_asset != null)
                loaded_assets[key] = null;
            return;
        }

        GD.Print("retrieved [" + key + "]");

        if (type == "Texture")
        {
            string extension = System.IO.Path.GetExtension(resource).ToLower().Trim();

            Image image = new Image();
            if (extension == ".png")
                image.LoadPngFromBuffer(body);
            else if (extension == ".jpg" || extension == ".jpeg")
                image.LoadJpgFromBuffer(body);
            else if (extension == ".bmp")
                image.LoadBmpFromBuffer(body);
            else if (extension == ".tga")
                image.LoadTgaFromBuffer(body);
            else if (extension == ".webp")
                image.LoadWebpFromBuffer(body);

            ImageTexture result_tex = new ImageTexture();
            result_tex.CreateFromImage(image, (uint)Godot.Texture.FlagsEnum.Filter);
            SetResource(key, result_tex);
        }
        else if (type == "String" || type == "string")
        {
            string s = Encoding.UTF8.GetString(body);
            SetResource(key, s);
        }
        else if (type == "AudioStream")
        {
            string file_extension = System.IO.Path.GetExtension(resource).ToLower();
            if (file_extension == ".wav")
            {
                // Parse the WAV file header
                using (var stream = new MemoryStream(body))
                using (var reader = new BinaryReader(stream))
                {
                    // Check for valid WAV header "RIFF"
                    string riff = new string(reader.ReadChars(4));
                    if (riff != "RIFF") return;

                    reader.ReadInt32(); // Chunk size
                    reader.ReadChars(4); // Format

                    // "fmt " chunk
                    reader.ReadChars(4); // Sub-chunk1 ID
                    reader.ReadInt32(); // Sub-chunk1 size
                    reader.ReadInt16(); // Audio format
                    short numChannels = reader.ReadInt16(); // Num channels
                    int sampleRate = reader.ReadInt32(); // Sample rate
                    reader.ReadInt32(); // Byte rate
                    reader.ReadInt16(); // Block align
                    short bitsPerSample = reader.ReadInt16(); // Bits per sample

                    // "data" chunk
                    reader.ReadChars(4); // Sub-chunk2 ID
                    int dataSize = reader.ReadInt32(); // Sub-chunk2 size

                    // Load the WAV data into an AudioStreamSample
                    var audioStreamSample = new AudioStreamSample();
                    audioStreamSample.Data = reader.ReadBytes(dataSize);
                    audioStreamSample.Format = bitsPerSample == 16 ? AudioStreamSample.FormatEnum.Format16Bits : AudioStreamSample.FormatEnum.Format8Bits;
                    audioStreamSample.Stereo = numChannels == 2;
                    audioStreamSample.MixRate = sampleRate;

                    SetResource(key, audioStreamSample);
                }
            }
            else if (file_extension == ".mp3")
            {
                GD.PrintErr("Processing MP3 [" + key + "] performance will be awful. Please use OGGs instead (ideally, all mp3s are converted to oggs on server).");
                GD.Print("processing mp3 [" + key + "]");
                // Decode the MP3 data into raw PCM data using NLayer
                var mp3Stream = new MemoryStream(body);
                var mp3Reader = new MpegFile(mp3Stream);

                var floatBuffer = new float[mp3Reader.Length];
                int samplesRead = mp3Reader.ReadSamples(floatBuffer, 0, floatBuffer.Length);

                // Convert the float samples to 16-bit integer samples
                var intBuffer = new byte[samplesRead * sizeof(short)];
                for (int i = 0; i < samplesRead; i++)
                {
                    short sample = (short)(floatBuffer[i] * short.MaxValue);
                    intBuffer[i * 2] = (byte)(sample & 0xff);
                    intBuffer[i * 2 + 1] = (byte)(sample >> 8);
                }

                // Create an AudioStreamSample and set its properties
                var audioStreamSample = new AudioStreamSample
                {
                    Data = intBuffer,
                    Format = AudioStreamSample.FormatEnum.Format16Bits,
                    MixRate = (int)mp3Reader.SampleRate,
                    Stereo = mp3Reader.Channels == 2
                };

                GD.Print("done processing mp3 [" + resource + "]");

                SetResource(key, audioStreamSample);
            }
            else if (file_extension == ".ogg")
            {
                var oggStream = new AudioStreamOGGVorbis();
                oggStream.Data = body;
                SetResource(key, oggStream);
            }
        }
        else
        {
            // Get the last part of the string
            string[] parts = resource.Split('.');
            string extension = (parts.Length > 1) ? "." + parts[parts.Length - 1] : string.Empty;
            GD.Print(extension);

            if(extension == ".bin" && type == "ActorConfig")
            {

                ActorConfig parsedData = ProtobufParser.ParseBinary<ActorConfig>(body, resource, type);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                SetResource(key, parsedData);

                GD.Print("We did it! We read: ===========================");
                GD.Print(parsedData.ToString());
            }
            else
            {
                string s = Encoding.UTF8.GetString(body);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                SetResource(key, JsonConvert.DeserializeObject(s, Type.GetType(type), settings));
            }
        }

        if (assets_currently_loading.Contains(key))
        {
            assets_currently_loading.Remove(key);
        }

        if (request_used != null)
            request_used.QueueFree();
    }

    static void SetResource<T>(string key, T asset)
    {
        instance.loaded_assets[key] = asset;

        NotifyUsageCallbacks(key, asset);
    }

    static void NotifyUsageCallbacks<T>(string resource, T resource_value)
    {
        if (!resource_usage_callbacks.ContainsKey(resource))
            return;

        List<Action<T>> callbacks_for_resource_changed = resource_usage_callbacks[resource]
            .Cast<Action<T>>()
            .ToList();

        foreach (Action<T> callback in callbacks_for_resource_changed)
        {
            callback(resource_value);
        }
    }

    public static void Load<T>(string resource)
    {
        if (instance.loaded_assets.ContainsKey(resource))
        {
            GD.PrintErr("We already have [" + resource + "] in cache.");
            return;
        }

        HTTPRequest new_request = new HTTPRequest();
        instance.AddChild(new_request);

        Godot.Collections.Array extra_params = new Godot.Collections.Array
        {
            resource,
            typeof(T).Name,              new_request,
            null
        };

        GD.Print("OBJECT============================");
        GD.Print(extra_params[0] + " " + extra_params[1]);

        new_request.Connect("request_completed", instance, nameof(OnRequestCompleted), extra_params);
        string web_url = @"https://arborinteractive.com/squirrel_rts/mods/" + GetCurrentModID() + @"/resources/" + resource;

        if (instance.assets_currently_loading.Contains(resource))
            return;
        instance.assets_currently_loading.Add(resource);

        var headers = new string[]
        {
            "Cache-Control: max-age=999999", // Cache data for 1 hour (3600 seconds)
        };

        new_request.Request(web_url, headers);
        GD.Print("web retrieving [" + resource + "]");
    }

    public static T Get<T>(string asset_path_relative_to_external_resources_folder) where T : class
    {
        if (!instance.loaded_assets.ContainsKey(asset_path_relative_to_external_resources_folder))
        {
            GD.PrintErr("[ArborResource.Get] Failed to find asset [" + asset_path_relative_to_external_resources_folder + "]. It may not be loaded yet. Be sure to launch the ArborResource.Load() coroutine.");
            return null;
        }

        return (T)instance.loaded_assets[asset_path_relative_to_external_resources_folder];
    }

    public static IEnumerator WaitFor(string asset_path_relative_to_external_resources_folder)
    {
        while (!instance.loaded_assets.ContainsKey(asset_path_relative_to_external_resources_folder))
            yield return null;
    }

    public static IEnumerator Upload<T>(string resource)
    {
        if (OS.GetName() == "Web" || OS.GetName() == "HTML5")
            yield return UploadWeb<T>(resource);
        else
            yield return UploadDesktop<T>(resource);
    }

    /* A developer may associate an unlimited number of callbacks with any particular resource. */
    /* When the resource becomes available, or when it changes, all associated callbacks will run */
    /* When the callback's associated node becomes invalid / destroyed, we can clean up the callbacks automatically. */
    static Dictionary<string, List<object>> resource_usage_callbacks = new Dictionary<string, List<object>>();
    static Dictionary<Node, List<CallbackResourcePair>> node_to_associated_callbacks = new Dictionary<Node, List<CallbackResourcePair>>();

    public static void UseResource<T>(string resource, Action<T> callback, Node associated_node) {

        /* Establish list of callbacks for this resource if one doesn't yet exist */
        if (!resource_usage_callbacks.ContainsKey(resource))
            resource_usage_callbacks.Add(resource, new List<object>());

        /* Add this callback to the list (dupes are possible, but cause warning message). */
        if (resource_usage_callbacks[resource].Contains(callback))
            GD.PrintErr("duplicate resource callback added on [" + resource + "]");
        resource_usage_callbacks[resource].Add(callback);

        if (!node_to_associated_callbacks.ContainsKey(associated_node))
            node_to_associated_callbacks[associated_node] = new List<CallbackResourcePair>();
        node_to_associated_callbacks[associated_node].Add(new CallbackResourcePair() { callback = callback, resource = resource });

        /* Establish a callbackk on the associated_node so we know when it is destroyed (then we can unregister its callbacks) */
        if (!associated_node.IsConnected("tree_exiting", instance, nameof(OnCallbackAssociatedNodeExitTree)))
        {
            Godot.Collections.Array extra_params = new Godot.Collections.Array
            {
                associated_node
            };

            associated_node.Connect("tree_exiting", instance, nameof(OnCallbackAssociatedNodeExitTree), extra_params);
        }

        /* If this asset has already been loaded, execute the callback immediately */
        if (instance.loaded_assets.ContainsKey(resource) && !instance.assets_currently_loading.Contains(resource))
        {
            callback((T)instance.loaded_assets[resource]);
        }
        /* Otherwise, request a load of the resource */
        else
        {
            ArborResource.Load<T>(resource);
        }

        PrintUsageCallbackCount();
    }

    class CallbackResourcePair
    {
        public object callback;
        public string resource;
    }

    static void PrintUsageCallbackCount()
    {
        int result = 0;
        foreach(List<object> callback_list in resource_usage_callbacks.Values)
        {
            result += callback_list.Count;
        }

        GD.Print("TOTAL USAGE CALLBACKS : " + result.ToString());
    }

    static IEnumerator UploadWeb<T>(string resource)
    {
        string filter_string = "image/*";
        if(typeof(T) == typeof(AudioStream))
            filter_string = "audio/*";

        OpenFileDialogueWeb(filter_string);

        /* Wait for upload dialogue */
        bool upload_finished = false;
        object data = null;
        object filename = null;
        while (!upload_finished)
        {
            data = JavaScript.Eval("window.upload_input_data");
            filename = JavaScript.Eval("window.upload_input_filename");

            if (data != null && filename != null)
                upload_finished = true;

            yield return null;
        }

        /* Process data */
        JavaScript.Eval("window.upload_input_data = null;");
        JavaScript.Eval("window.upload_input_filename = null;");

        instance.OnRequestCompleted(200, 200, null, (byte[])data, (string)filename, typeof(T).Name, null, force_key_for_asset: resource);
    }

    string file_dialogue_path = "";
    private void OnFileSelected(string path)
    {
        GD.Print("File dialog was selected.");
        file_dialogue_path = path;
    }

    static IEnumerator UploadDesktop<T>(string resource)
    {
        CanvasLayer new_canvas_layer = new CanvasLayer { Layer = 1 };
        instance.AddChild(new_canvas_layer);

        FileDialog file_dialogue = new FileDialog();
        file_dialogue.Mode = FileDialog.ModeEnum.OpenFile;
        file_dialogue.PopupExclusive = true;
        file_dialogue.Access = FileDialog.AccessEnum.Filesystem;
        file_dialogue.RectMinSize = new Vector2(OS.WindowSize.x * 0.75f, OS.WindowSize.y * 0.75f); // Set to the desired dimensions

        instance.file_dialogue_path = "";
        file_dialogue.Connect("file_selected", instance, nameof(OnFileSelected));

        new_canvas_layer.AddChild(file_dialogue);

        file_dialogue.PopupCentered();
        GD.Print("dialogue opening!");

        yield return null;
        yield return null;

        while (file_dialogue.Visible)
            yield return null;

        GD.Print("dialogue closed!"); 
        GD.Print("closed via cancel? [" + instance.file_dialogue_path + "]");


        if (file_dialogue.CurrentPath == null || file_dialogue.CurrentPath == "" || instance.file_dialogue_path == null || instance.file_dialogue_path == "")
            yield break;

        string filepath = file_dialogue.CurrentPath;
        string filename = System.IO.Path.GetFileName(filepath);
        byte[] data = System.IO.File.ReadAllBytes(filepath);

        instance.OnRequestCompleted(200, 200, null, data, filename, typeof(T).Name, null, force_key_for_asset: resource);
    }

    static bool file_dialogue_web_initialized = false;
    static void InitializeFileDialogueWeb()
    {
        if (file_dialogue_web_initialized)
            return;

        string jsScript = @"window.timber_file_picker = document.createElement('input');
                            window.timber_file_picker.type = 'file';
                            window.timber_file_picker.onchange = function(event) {
                                var file = event.target.files[0];
                                var reader = new FileReader();

                                reader.onload = function(event) {
                                    window.upload_input_data = reader.result;
                                    window.upload_input_filename = file.name
                                };

                                reader.readAsArrayBuffer(file);
                            };";
        JavaScript.Eval(jsScript);

        file_dialogue_web_initialized = true;
        GD.Print("performed web file dialogue initialize.");
    }

    static void OpenFileDialogueWeb(string accept_filter_string)
    {
        string jsScript = @"window.timber_file_picker.accept = '" + accept_filter_string + "'; window.timber_file_picker.click();";
        JavaScript.Eval(jsScript);
        GD.Print("performed upload dialogue");
    }

    public static string GetResourcesDirectory()
    {
        string location = System.IO.Directory.GetCurrentDirectory();
        if (OS.HasFeature("editor") || OS.HasFeature("wasm"))
            location += @"\windows_build";

        location += @"\mods\" + GetCurrentModID() + @"\resources\";


        return location;
    }

    static string GetCurrentModID()
    {
        if (web_query_parameters.ContainsKey("default_mod"))
            return web_query_parameters["default_mod"];

        return "021c18c5-d54b-4338-a441-4f07ff496333";
    }

    public static void PerformEditType(Type t)
    {
        List<string> buttonLabels = new List<string>();
        List<Action> buttonActions = new List<Action>();

        FieldInfo[] fields = typeof(TitleScreenDefinition).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (FieldInfo field in fields)
        {
            buttonLabels.Add("Edit " + field.Name);
            buttonActions.Add(() => { ArborCoroutine.StartCoroutine(DoEdit()); });
        }

        Vector2 location = new Vector2(300, 200);
        //UIManager.SimpleMenu("Title Screen", buttonLabels, buttonActions);
    }

    static IEnumerator DoEdit()
    {
        yield return ArborResource.Upload<Texture>("upload");
    }
}

public static class ArborResourceExtensions
{
    public static void UseResource<T>(this Node node, string resource, Action<T> callback)
    {
        ArborResource.UseResource(resource, callback, node);
    }
}