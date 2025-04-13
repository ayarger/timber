using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class CreateProjectPopup : PopupDialog
{
    TextEdit projectName;

    ProjectsPopup projectsPopup;

    RichTextLabel error;

    bool busy = false;


    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        projectName = GetNode<TextEdit>("ProjectNameField");

        projectsPopup = GetParent().GetNode<ProjectsPopup>("ProjectsPopup");
        error = GetNode<RichTextLabel>("Error");
    }

    public void Open()
    {
        PopupCentered();
    }

    public void OnCreateProject()
    {
        if(busy) return;
        
        ArborCoroutine.StartCoroutine(CreateProjectCoro());
    }

    IEnumerator CreateProjectCoro()
    {
        busy = true;
        error.Text = "Loading...";
        string pn = projectName.Text;
        string id = ArborResource.CreateProject(pn);
        yield return ArborResource.WaitAuthentication(id);
        bool successful = ArborResource.timber_responses[id].responseCode < 400;
        if (successful)
        {
            TempSecrets.TOKEN = System.Text.Encoding.Default.GetString(ArborResource.timber_responses[id].body);

            id = ArborResource.GetProjects();
            yield return ArborResource.WaitAuthentication(id);
            ProjectMetadata[] data = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectMetadata[]>(ArborResource.timber_responses[id].GetBodyAsString());

            foreach (var datum in data)
            {
                GD.PushWarning(datum.name);
            }

            Hide();
            projectsPopup.Open(data);
        }
        else
        {
            error.Text = "Something went wrong!";
        }
        busy = false;

    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
