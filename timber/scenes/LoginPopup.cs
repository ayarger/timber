using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class LoginPopup : PopupDialog
{
    TextEdit username;
    TextEdit password;

    ProjectsPopup projectsPopup;
    CreateUserPopup createUserPopup;

    RichTextLabel error;

    bool busy = false;


    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        username = GetNode<TextEdit>("UsernameField");
        password = GetNode<TextEdit>("PasswordField");
        error = GetNode<RichTextLabel>("Error");
        projectsPopup = GetParent().GetNode<ProjectsPopup>("ProjectsPopup");
        createUserPopup = GetParent().GetNode<CreateUserPopup>("CreateUserPopup");
    }

    public void Open()
    {
        PopupCentered();
    }

    public void OnCreateUser()
    {
        GD.PushWarning("WTF");
        createUserPopup.Open();
    }
    public void OnLogin()
    {
        if(busy) return;
        // If there is a game id in the url, go directly to the game. TODO: Later, disable the editor.
        if (OS.GetName() == "Web" || OS.GetName() == "HTML5")
        {
            Dictionary<string, string> web_query_parameters = new Dictionary<string, string>();
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
            if (web_query_parameters.ContainsKey("default_mod"))
            {
                TransitionSystem.RequestTransition(@"res://Main.tscn");
                return;
            }
        }
        ArborCoroutine.StartCoroutine(LoginCoro());
    }

    IEnumerator LoginCoro()
    {
        busy = true;
        error.Text = "Loading...";
        string u = username.Text;
        string p = password.Text;
        string id = ArborResource.GenerateToken(u, p);
        yield return ArborResource.WaitAuthentication(id);
        bool successful = ArborResource.timber_responses[id].responseCode < 400;
        if (successful)
        {
            id = ArborResource.GetProjects();
            yield return ArborResource.WaitAuthentication(id);
            ProjectMetadata[] data = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectMetadata[]>(ArborResource.timber_responses[id].GetBodyAsString());

            foreach(var datum in data)
            {
                GD.PushWarning(datum.name);
            }

            projectsPopup.Open(data);

        }
        else
        {
            error.Text = "Invalid login!";
        }
        busy = false;

    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

//Might move this elsewhere later
[Serializable]
public class ProjectMetadata
{
    public string uuid;
    public string name;
}