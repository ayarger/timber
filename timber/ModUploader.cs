using Godot;
using System;
using System.IO;
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
using Newtonsoft.Json;

public class ModUploader : Button
{
    private const string BucketName = "arborinteractive-website-public";
    private const string DistributionId = "E2Z1C9QZG151BD";

    public override void _Ready()
    {
        base._Ready();

        if (Engine.EditorHint)
            QueueFree();


    }

    public async void OnPressed()
    {
        GD.Print("upload mod!");
        await PerformUpload();
    }

    async Task PerformUpload()
    {
        string LocalDirectory = System.IO.Directory.GetCurrentDirectory() + @"/windows_build/mods/021c18c5-d54b-4338-a441-4f07ff496333/resources/";

        GenerateModFileManifest(LocalDirectory, LocalDirectory + "mod_file_manifest.json");

        GD.Print("upload 1...");

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
            string relative_path = GetFormattedRelativePath(LocalDirectory, filePath);

            GD.Print("considering [" + relative_path + "]");
            var key = "squirrel_rts/mods/021c18c5-d54b-4338-a441-4f07ff496333/resources/" + relative_path;
            uploadedFiles.Add("/" + key);

            await transferUtility.UploadAsync(filePath, BucketName, key);
            GD.Print($"Uploaded {key} to S3 bucket.");
        }

        GD.Print("upload 3...");

        return;


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
        OS.ShellOpen("https://arborinteractive.com/squirrel_rts/lua_experiment.html");
        GD.Print("Done uploading web build!");

    }

    public static void GenerateModFileManifest(string directoryPath, string outputFilePath)
    {
        List<string> modFiles = new List<string>();
        GetAllFilesRecursive(directoryPath, modFiles, directoryPath);
        string json = JsonConvert.SerializeObject(new { mod_files = modFiles }, Formatting.Indented);
        System.IO.File.WriteAllText(outputFilePath, json);
    }

    public static void GetAllFilesRecursive(string directoryPath, List<string> modFiles, string basePath)
    {
        string[] filePaths = System.IO.Directory.GetFiles(directoryPath);
        foreach (string filePath in filePaths)
        {
            modFiles.Add(GetFormattedRelativePath(basePath, filePath));
        }

        string[] subDirectories = System.IO.Directory.GetDirectories(directoryPath);
        foreach (string subDirectory in subDirectories)
        {
            GetAllFilesRecursive(subDirectory, modFiles, basePath);
        }
    }

    public static string GetFormattedRelativePath(string basePath, string fullPath)
    {
        Uri baseUri = new Uri(basePath + System.IO.Path.DirectorySeparatorChar);
        Uri fullUri = new Uri(fullPath);
        Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
        relativePath = relativePath.TrimStart('.', System.IO.Path.DirectorySeparatorChar).Replace(System.IO.Path.DirectorySeparatorChar + "" + System.IO.Path.DirectorySeparatorChar, "" + System.IO.Path.DirectorySeparatorChar);
        relativePath = relativePath.Substring(1, relativePath.Length - 1);
        return relativePath;
    }
}
