using Godot;
using System;
using System.IO;
using System.Collections;
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

public class delayed_menu_open_experiment : Button
{
    public async void OnPressed()
    {
        GD.Print("upload mod!");
        ArborCoroutine.StartCoroutine(DelayedMenuOpen());
    }

    IEnumerator DelayedMenuOpen()
    {
        yield return ArborCoroutine.WaitForSeconds(2);

        TitleScreenDefinition.UserEdit();
    }
}
