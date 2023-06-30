using Godot;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

public class UploadButtonExperiment : Button
{
    public void OnPressed()
    {
        ArborCoroutine.StartCoroutine(DoOnPressed(), this);
    }

    IEnumerator DoOnPressed()
    {
        yield return ArborResource.Upload<Texture>("upload");
        Texture t = ArborResource.Get<Texture>("upload");

        var tex_rect = (TextureRect)GetNode("TextureRect");
        tex_rect.Texture = t;
    }

    



    
}
