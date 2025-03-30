using Godot;
using System;

public class DeleteButton : TextureButton
{
    private SlidePreview parentPreview;
    private PreviewRect parentPreviewRect;
    private bool isHovering = false;

    public override void _Ready()
    {
        parentPreview = GetParent().GetParent<SlidePreview>();
        parentPreviewRect = GetParent<PreviewRect>();
        
        Visible = false;

        // Connect this button's pressed signal
        Connect("pressed", this, nameof(OnDeletePressed));

        // Listen to parent's hover signals to show/hide
        if (parentPreviewRect != null)
        {
            parentPreviewRect.Connect("mouse_entered", this, nameof(OnParentMouseEntered));
            parentPreviewRect.Connect("mouse_exited", this, nameof(OnParentMouseExited));
        }
        this.Connect("mouse_entered", this, nameof(OnMouseEntered));
    }

    private void OnParentMouseEntered()
    {
        Visible = true;
        isHovering = true;
    }

    private void OnParentMouseExited()
    {
        Visible = false;
        isHovering = false;
    }

    private void OnMouseEntered()
    {
        //TODO
    }

    private void OnMouseExited()
    {
        //TODO
    }

    private void OnDeletePressed()
    {
        if (parentPreview == null || parentPreview.originalSceneData == null)
        {
            GD.PrintErr("DeleteButton: Missing parent preview or original scene data.");
            return;
        }
        
        //Hide the UI
        parentPreview.Hide();
        
        //Delete currSlide Preview
        GD.Print("resource index: " + parentPreview.cutsceneImageResource.Index);
        GD.Print("slide index: " + parentPreview.orderLabel.Text);
        int removedCount = CutsceneManager.Instance.cutsceneImages.RemoveAll(c => c.ImagePath == parentPreview.cutsceneImageResource.ImagePath);
        if (removedCount == 0)
        {
            GD.PrintErr("DeleteButton: No matching resource removed!");
            return;
        }
        //Update order
        int index = 0;
        foreach(CutsceneImageResource cutsceneImage in CutsceneManager.Instance.cutsceneImages)
        {
            cutsceneImage.Index = index;
            index++;
        }
        
        CutsceneEditor.Instance.UpdateList();
        GD.Print("SlidePreview deleted and JSON updated.");
        
        // Save updated cutscene data
        string filePath = "res://temp_cutscenes/intro_cutscene_config_test.json";
        filePath = ProjectSettings.GlobalizePath(filePath);
        CutsceneManager.Instance.ConvertCutsceneToJson(filePath);
        
        // Remove SlidePreviewNode
        parentPreview.QueueFree();
        // Update order display
        
    }
}