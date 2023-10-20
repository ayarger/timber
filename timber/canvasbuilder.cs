using Godot;
using System;
using System.IO;
using BrotliSharpLib;
using System.Threading.Tasks;
using Amazon.Auth;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using System.Collections.Generic;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using System.Net;

public class CanvasBuilder : Node
{
    static CanvasBuilder instance;

    private const string BucketName = "arborinteractive-website-public";
    private const string DistributionId = "E2Z1C9QZG151BD";

    public override void _Ready()
    {
        base._Ready();

        if (Engine.EditorHint)
            QueueFree();

        instance = this;
    }

    public static void PerformBuild(int quality = 1)
    {
        string LocalDirectory = @"C:\Users\ayarg\Documents\dev\umich\canvasminigame\web_build2\";

        /* Replace links in html and js files with brotli references */
        string primary_html_file_contents = System.IO.File.ReadAllText(LocalDirectory + "canvasminigame.html");
        primary_html_file_contents = primary_html_file_contents.Replace(".html\"", ".html.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".html'", ".html.br'");
        primary_html_file_contents = primary_html_file_contents.Replace(".js\"", ".js.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".js'", ".js.br'");
        primary_html_file_contents = primary_html_file_contents.Replace(".pck\"", ".pck.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".pck'", ".pck.br'");
        primary_html_file_contents = primary_html_file_contents.Replace(".wasm\"", ".wasm.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".wasm'", ".wasm.br'");

        System.IO.File.WriteAllText(LocalDirectory + "canvasminigame.html", primary_html_file_contents);

        string primary_js_file_contents = System.IO.File.ReadAllText(LocalDirectory + "canvasminigame.js");
        primary_js_file_contents = primary_js_file_contents.Replace(".html\"", ".html.br\"");
        primary_js_file_contents = primary_js_file_contents.Replace(".html'", ".html.br'");
        primary_js_file_contents = primary_js_file_contents.Replace(".js\"", ".js.br\"");
        primary_js_file_contents = primary_js_file_contents.Replace(".js'", ".js.br'");

        primary_js_file_contents = primary_js_file_contents.Replace(".pck\"", ".pck.br\"");
        primary_js_file_contents = primary_js_file_contents.Replace(".pck'", ".pck.br'");
        primary_js_file_contents = primary_js_file_contents.Replace(".pck`", ".pck.br`");

        primary_js_file_contents = primary_js_file_contents.Replace(".wasm\"", ".wasm.br\"");
        primary_js_file_contents = primary_js_file_contents.Replace(".wasm'", ".wasm.br'");
        primary_js_file_contents = primary_js_file_contents.Replace(".wasm`", ".wasm.br`");




        System.IO.File.WriteAllText(LocalDirectory + "canvasminigame.js", primary_js_file_contents);

        /* Compress files */
        foreach (var filePath in System.IO.Directory.GetFiles(LocalDirectory, "*.*", SearchOption.AllDirectories))
        {
            string extension = System.IO.Path.GetExtension(filePath);
            if (extension == ".js" || extension == ".pck" || extension == ".wasm")
            {
                CompressWithBrotli(filePath, filePath + ".br", quality);
            }
        }
        
        GD.Print("Done making brotli!");
    }

    public static void OpenWebBuild()
    {
        OS.ShellOpen("https://arborinteractive.com/canvas/494/canvasminigame.html");
    }

    static void CompressWithBrotli(string inputPath, string outputPath, int quality)
    {
        byte[] uncompressed_data = System.IO.File.ReadAllBytes(inputPath);
        GD.Print("Compressing [" + inputPath + "] of size [" + uncompressed_data.Length + "]");

        //byte[] compressed_data = Brotli.CompressBuffer(uncompressed_data, 0, uncompressed_data.Length, 11, 22);
        byte[] compressed_data = Brotli.CompressBuffer(uncompressed_data, 0, uncompressed_data.Length, quality);

        System.IO.File.WriteAllBytes(outputPath, compressed_data);
        GD.Print("...done!");
    }
}
