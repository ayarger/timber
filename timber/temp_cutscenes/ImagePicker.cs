using Godot;
using System;

public class ImagePicker : Node
{
    public string ImagePath { get; set; }
    private FileDialog _fileDialog;
    private Button _chooseButton;
    private Button _uploadButton;

    private enum DialogPurpose { None, Choose, Upload }
    private DialogPurpose _currentPurpose = DialogPurpose.None;

    public override void _Ready()
    {
        _fileDialog = GetParent<FileDialog>();
        _chooseButton = GetNode<Button>("../../../ScrollContainer/GridContainer/AddSlide/ChooseButton");
        _uploadButton = GetNode<Button>("../../../ScrollContainer/GridContainer/AddSlide/UploadButton");

        _fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _fileDialog.Filters = new string[]
        {
            "*.png ; PNG Images",
            "*.jpg ; JPG Images",
            "*.jpeg ; JPEG Images",
            "*.bmp ; Bitmap Images",
            "*.webp ; WebP Images"
        };

        _fileDialog.Connect("file_selected", this, nameof(OnFileSelected));
        _chooseButton.Connect("pressed", this, nameof(OnChoosePressed));
        _uploadButton.Connect("pressed", this, nameof(OnUploadPressed));
    }

    private void OnChoosePressed()
    {
        _currentPurpose = DialogPurpose.Choose;
        _fileDialog.Mode = FileDialog.ModeEnum.OpenFile;
        _fileDialog.Access = FileDialog.AccessEnum.Resources;
        _fileDialog.WindowTitle = "Choose an Image";
        _fileDialog.PopupCentered();
        _chooseButton.Hide();
        _uploadButton.Hide();
    }

    private void OnUploadPressed()
    {
        _currentPurpose = DialogPurpose.Upload;
        _fileDialog.Mode = FileDialog.ModeEnum.SaveFile;
        _fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _fileDialog.WindowTitle = "Upload an Image";
        _fileDialog.PopupCentered();
        _chooseButton.Hide();
        _uploadButton.Hide();
    }

    private void OnFileSelected(string path)
    {
        GD.Print($"Selected Path: {path}");

        switch (_currentPurpose)
        {
            case DialogPurpose.Choose:
                GD.Print("You chose an image: " + path);
                break;
            case DialogPurpose.Upload:
                GD.Print("Upload will be saved to: " + path);
                break;
        }
        _chooseButton.Hide();
        _uploadButton.Hide();
        _currentPurpose = DialogPurpose.None;
        CutsceneEditor.Instance.AddSlideFromPath(path);
    }
}