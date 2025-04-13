using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class CreateUserPopup : PopupDialog
{
    TextEdit username;
    TextEdit password;

    LoginPopup loginPopup;

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
        loginPopup = GetParent().GetNode<LoginPopup>("LoginPopup");
    }

    public void Open()
    {
        PopupCentered();
    }

    public void OnCreateUser()
    {
        if(busy) return;
        
        ArborCoroutine.StartCoroutine(CreateUserCoro());
    }

    IEnumerator CreateUserCoro()
    {
        busy = true;
        error.Text = "Loading...";
        string u = username.Text;
        string p = password.Text;
        string id = ArborResource.CreateUser(u, p);
        yield return ArborResource.WaitAuthentication(id);
        bool successful = ArborResource.timber_responses[id].responseCode < 400;
        if (successful)
        {
            loginPopup.Hide();
            Hide();
        }
        else
        {
            error.Text = "Username already exists!";
        }
        busy = false;

    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
