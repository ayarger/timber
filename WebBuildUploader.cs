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

public class WebBuildUploader : Node
{
    static WebBuildUploader instance;

    private const string BucketName = "arborinteractive-website-public";
    private const string DistributionId = "E2Z1C9QZG151BD";

    public override void _Ready()
    {
        base._Ready();

        if (Engine.EditorHint)
            QueueFree();

        instance = this;
    }

    public static void GotoWebBuild()
    {
        OpenWebBuild();
    }

    public static async void UploadWebBuild()
    {
        await instance.PerformUpload(quality: 1);
    }

    public static async void UploadOptimizedWebBuild()
    {
        await instance.PerformUpload(quality: 11);
    }

    async Task PerformUpload(int quality = 1)
    {
        string LocalDirectory = System.IO.Directory.GetCurrentDirectory() + @"/web_build/";

        /* Replace links in html and js files with brotli references */
        string primary_html_file_contents = System.IO.File.ReadAllText(LocalDirectory + "lua_experiment.html");
        primary_html_file_contents = primary_html_file_contents.Replace(".html\"", ".html.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".html'", ".html.br'");
        primary_html_file_contents = primary_html_file_contents.Replace(".js\"", ".js.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".js'", ".js.br'");
        primary_html_file_contents = primary_html_file_contents.Replace(".pck\"", ".pck.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".pck'", ".pck.br'");
        primary_html_file_contents = primary_html_file_contents.Replace(".wasm\"", ".wasm.br\"");
        primary_html_file_contents = primary_html_file_contents.Replace(".wasm'", ".wasm.br'");

        System.IO.File.WriteAllText(LocalDirectory + "lua_experiment.html", primary_html_file_contents);

        string primary_js_file_contents = System.IO.File.ReadAllText(LocalDirectory + "lua_experiment.js");
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

        


        System.IO.File.WriteAllText(LocalDirectory + "lua_experiment.js", primary_js_file_contents);

        /* Compress files */
        foreach (var filePath in System.IO.Directory.GetFiles(LocalDirectory, "*.*", SearchOption.AllDirectories))
        {
            string extension = System.IO.Path.GetExtension(filePath);
            if(extension == ".js" || extension == ".pck" || extension == ".wasm")
            {
                CompressWithBrotli(filePath, filePath + ".br", quality);
            }
        }

        var chain = new CredentialProfileStoreChain();
        AWSCredentials awsCredentials;
        if (chain.TryGetAWSCredentials("squirrel_engine_web_uploader", out awsCredentials))
        {
            GD.Print("Found aws credentials...");
        }
        else
        {
            GD.Print("Failed to locate aws credentials...");
        }

        GD.Print("upload 2...");

        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

        var s3Client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.USEast2);

        GD.Print("upload 3...");




        var cloudFrontClient = new AmazonCloudFrontClient(awsCredentials, Amazon.RegionEndpoint.USEast1);

        var transferUtility = new TransferUtility(s3Client);

        var uploadedFiles = new List<string>();

        foreach (var filePath in System.IO.Directory.GetFiles(LocalDirectory, "*.*", SearchOption.AllDirectories))
        {
            var key = "squirrel_rts/" + System.IO.Path.GetFileName(filePath);

            var request = new TransferUtilityUploadRequest
            {
                FilePath = filePath,
                BucketName = BucketName,
                Key = key
            };

            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            if (extension == ".import" || extension == ".js" || extension == ".pck" || extension == ".wasm")
                continue;

            uploadedFiles.Add("/" + key);

            GD.Print(extension);
            if(extension == ".br")
            {
                request.Headers.ContentEncoding = "br";
            }

            transferUtility.Upload(request);
            GD.Print($"Uploaded {key} to S3 bucket.");
        }

        GD.Print("upload 3...");

        var invalidationRequest = new CreateInvalidationRequest
        {
            
            DistributionId = DistributionId,
            InvalidationBatch = new InvalidationBatch
            {
                CallerReference = DateTime.Now.Ticks.ToString(),
                Paths = new Paths
                {
                    Quantity = uploadedFiles.Count,
                    Items = uploadedFiles
                }
            } 
             
        };

        GD.Print("Invalidation created for uploaded files in CloudFront distribution.");

        await cloudFrontClient.CreateInvalidationAsync(invalidationRequest);

        // Dispose of the clients
        s3Client.Dispose();
        cloudFrontClient.Dispose();

        GD.Print("Waiting for cloudfront distribution to invalidate...");

        await Task.Delay(30 * 1000);
        OpenWebBuild();
        GD.Print("Done uploading web build!");

    }

    static void OpenWebBuild()
    {
        OS.ShellOpen("https://arborinteractive.com/squirrel_rts/lua_experiment.html?default_mod=021c18c5-d54b-4338-a441-4f07ff496333");
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
