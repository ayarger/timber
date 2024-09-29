using Godot;
using System.Collections.Generic;
using YarnSpinnerGodot;



static class YarnManager
{
    public static Dictionary<string, YarnProject> projects = new Dictionary<string, YarnProject>();
}

public class YarnUpload : Button
{
    private FileDialog fileDialog;
    [Export] NodePath fileDialogPath;

    public override void _Ready()
    {
        fileDialog = GetNode<FileDialog>(fileDialogPath);
    }

    private void _on_Button_pressed()
    {
        fileDialog.PopupCentered(Vector2.One * 400);
        fileDialog.SetAsToplevel(true);
    }

    private void _on_FileDialog_file_selected(string path)
    {
        ProcessFile(path);
    }

    private void ProcessFile(string path)
    {
        // Create new directory
        /*string dirName = GenerateUniqueDirectoryName();
        string fileName = System.IO.Path.GetFileName(path);
        string dirPath = "res://" + dirName + "/";
        string filePath = dirPath + fileName;

        var dir = new Godot.Directory();
        if (!dir.DirExists(dirPath))
        {
            dir.MakeDirRecursive(dirPath);
        }

        // Copy file to directory
        if (dir.FileExists(path))
        {
            dir.Copy(path, filePath);
        }

        // Create yarn project in directory if it doesn't already exist
        string projectPath = dirPath + "YarnProject.tres";
        var newYarnProject = GD.Load<CSharpScript>("res://addons/YarnSpinner-Godot/Runtime/YarnProject.cs").New() as YarnSpinnerGodot.YarnProject;
        var absPath = ProjectSettings.GlobalizePath(projectPath);
        
        if (new Godot.File().FileExists(projectPath))
        {
            GD.PrintErr("File already exists at:", projectPath);
        } else
        {
            newYarnProject.ResourceName = System.IO.Path.GetFileNameWithoutExtension(absPath);
            newYarnProject.ResourcePath = projectPath;
            newYarnProject.SourceScripts.Add(fileName);
            var saveErr = ResourceSaver.Save(projectPath, newYarnProject);
            if (saveErr != Error.Ok)
            {
                GD.Print($"Failed to save yarn project to {projectPath}; Error: " + saveErr);
            }
            else
            {
                GD.Print($"Saved new yarn project to {projectPath}");
                YarnProjectEditorUtility.AddProjectToList(newYarnProject);
            }
            YarnProjectEditorUtility.CompileAllScripts(newYarnProject);
            YarnProjectEditorUtility.AddLineTagsToFilesInYarnProject(newYarnProject);
            YarnProjectEditorUtility.UpdateLocalizationCSVs(newYarnProject);
            YarnSpinnerGodot.DialogueRunner dialogueRunner = GetTree().CurrentScene.FindNode("DialogueRunner", recursive: true) as YarnSpinnerGodot.DialogueRunner;
            dialogueRunner.SetProject(GD.Load<YarnSpinnerGodot.YarnProject>(projectPath));
        }*/

        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        var newYarnProject = GD.Load<CSharpScript>("res://addons/YarnSpinner-Godot/Runtime/YarnProject.cs").New() as YarnSpinnerGodot.YarnProject;
        newYarnProject.ResourceName = fileName;
        //newYarnProject.ResourcePath = path; // Can probably remove in future
        newYarnProject.SourceScripts.Add(path); // Can remove in the future

        //TODO: THIS DOES NOT WORK ON THE WEB RIGHT NOW

        YarnEditorUtilityDecoupled.CompileAllScripts(newYarnProject);
        YarnEditorUtilityDecoupled.AddLineTagsToFilesInYarnProject(newYarnProject);

        //TODO: This function assumes running on OS, not a priority
        YarnEditorUtilityDecoupled.UpdateLocalizationCSVs(newYarnProject);
        
        YarnManager.projects[fileName] = newYarnProject;
        //YarnSpinnerGodot.DialogueRunner dialogueRunner = GetTree().CurrentScene.FindNode("DialogueRunner", recursive: true) as YarnSpinnerGodot.DialogueRunner;
        //dialogueRunner.SetProject(newYarnProject);
    }

}
