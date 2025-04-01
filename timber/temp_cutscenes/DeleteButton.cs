using Godot;
using System;

public class DeleteButton : TextureButton
{
    private SlidePreview parentPreview;
    private PreviewRect parentPreviewRect;


    public override void _Ready()
    {
        
        parentPreview = GetParent().GetParent<SlidePreview>();
        parentPreviewRect = GetParent<PreviewRect>();
        
        Visible = false;

        // Connect this button's pressed signal
        Connect("pressed", this, nameof(OnDeletePressed));
    }

    
    private void OnDeletePressed()
    {
        if (parentPreview == null || parentPreview.originalSceneData == null)
        {
            GD.PrintErr("DeleteButton: Missing parent preview or original scene data.");
            return;
        }
        CutsceneEditor.Instance.PromptDeleteSlide(parentPreview);
    }
}