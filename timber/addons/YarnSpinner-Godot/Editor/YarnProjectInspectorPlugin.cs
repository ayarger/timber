#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace YarnSpinnerGodot.Editor
{
    [Tool]
    public partial class YarnProjectInspectorPlugin : EditorInspectorPlugin
    {
        public EditorInterface editorInterface;
        private Button _recompileButton;
        private Button _addTagsButton;
        private Button _updateLocalizationsButton;
        private YarnCompileErrorsPropertyEditor _compileErrorsPropertyEditor;
        private ScrollContainer _parseErrorControl;
        private YarnProject _project;
        private readonly PackedScene _fileNameLabelScene = ResourceLoader.Load<PackedScene>("res://addons/YarnSpinner-Godot/Editor/UI/FilenameLabel.tscn");
        private readonly PackedScene _errorTextLabelScene = ResourceLoader.Load<PackedScene>("res://addons/YarnSpinner-Godot/Editor/UI/ErrorTextLabel.tscn");
        private readonly PackedScene _contextLabelScene = ResourceLoader.Load<PackedScene>("res://addons/YarnSpinner-Godot/Editor/UI/ContextLabel.tscn");
        private VBoxContainer _errorContainer;
        private RichTextLabel _sourceScriptsListLabel;
        private YarnSourceScriptsPropertyEditor _sourceScriptsPropertyEditor;

        public override bool CanHandle(Godot.Object obj)
        {
            return obj is YarnProject;
        }

        private KeyValuePair<string, string> _currLocale;
        public override bool ParseProperty(Godot.Object @object, int type, string path,
            int hint, string hintText, int usage)
        {
            try
            {
                _project = (YarnProject) @object;
                // hide some properties that are not editable by the user
                var hideProperties = new List<string>
                {
                    nameof(YarnProject.LastImportHadAnyStrings),
                    nameof(YarnProject.LastImportHadImplicitStringIDs),
                    nameof(YarnProject.IsSuccessfullyParsed),
                    nameof(YarnProject.CompiledYarnProgramBase64),
                    "_baseLocalizationJSON",
                    "_lineMetadataJSON",
                    "_serializedDeclarationsJSON",
                    "_listOfFunctionsJSON",
                };
                if (hideProperties.Contains(path))
                {
                    // hide these properties from inspector
                    return true;
                }

                if (path == nameof(YarnProject.SourceScripts))
                {
                    _sourceScriptsPropertyEditor = new YarnSourceScriptsPropertyEditor();
                    AddPropertyEditor(path, _sourceScriptsPropertyEditor);

                    int scriptAreaHeight = 40;
                    if (_project.SourceScripts != null && _project.SourceScripts.Any())
                    {
                        scriptAreaHeight = 180;
                    }

                    _sourceScriptsListLabel = new RichTextLabel();
                    _sourceScriptsListLabel.RectMinSize = new Vector2(0, scriptAreaHeight);
                    _sourceScriptsListLabel.HintTooltip = "YarnSpinner-Godot will search for all .yarn files" +
                                                          $" in the same directory as this {nameof(YarnProject)} (or a descendent directory)." +
                                                          " A list of .yarn files found this way will be displayed here.";
                    _sourceScriptsListLabel.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
                    _sourceScriptsListLabel.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
                    RenderSourceScriptsList(_project);
                    AddCustomControl(_sourceScriptsListLabel);
                    return true;
                }

                if (path == nameof(YarnProject.ProjectErrors))
                {
                    _compileErrorsPropertyEditor = new YarnCompileErrorsPropertyEditor();
                    AddPropertyEditor(path, _compileErrorsPropertyEditor);
                    _parseErrorControl = new ScrollContainer();
                    int errorAreaHeight = 40;
                    if (_project.ProjectErrors != null && _project.ProjectErrors.Length > 0)
                    {
                        errorAreaHeight = 200;
                    }

                    _parseErrorControl.RectMinSize = new Vector2(0, errorAreaHeight);
                    _parseErrorControl.SizeFlagsVertical = (int)Control.SizeFlags.Expand;
                    _parseErrorControl.SizeFlagsHorizontal = (int)Control.SizeFlags.Expand;

                    _errorContainer = new VBoxContainer();
                    _errorContainer.SizeFlagsVertical = (int)Control.SizeFlags.Expand;
                    _errorContainer.SizeFlagsHorizontal = (int)Control.SizeFlags.Expand;
                    _parseErrorControl.AddChild(_errorContainer);
                    //parseErrorControl.BbcodeEnabled = true;
                    _compileErrorsPropertyEditor.OnErrorsUpdated += RenderCompilationErrors;
                    RenderCompilationErrors(_project);
                    AddCustomControl(_parseErrorControl);
                    return true;
                }

                if (path == nameof(YarnProject.LocaleCodeToCSVPath))
                {
                    var localeGrid = new GridContainer();
                    localeGrid.Columns = 3;

                    var label = new Label();
                    label.Text = "Localization CSVs";

                    localeGrid.AddChild(label);
                    localeGrid.AddChild(new Label());

                    var addButton = new Button();
                    addButton.Text = "Add";
                    addButton.Connect("pressed", this, nameof(AddLocale));
                    localeGrid.AddChild(addButton);
                    localeGrid.SizeFlagsHorizontal |= (int)Control.SizeFlags.ExpandFill;
                    localeGrid.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;

                    foreach (var locale in _project.LocaleCodeToCSVPath)
                    {
                        _currLocale = locale;
                        var localeLabel = new Label();
                        localeLabel.Text = locale.Key;
                        localeGrid.AddChild(localeLabel);
                        var picker = new HBoxContainer();
                        picker.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
                        picker.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
                        var pathLabel = new Label();
                        pathLabel.Text = locale.Value;
                        if (pathLabel.Text == "")
                        {
                            pathLabel.Text = "(none)";
                        }

                        pathLabel.RectMinSize = new Vector2(80, 30);
                        pathLabel.SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;
                        pathLabel.ClipText = true;
                        picker.AddChild(pathLabel);
                        var pickerButton = new Button();
                        pickerButton.Text = "Browse";
                        pickerButton.Connect("pressed", this, nameof(CallSelectLocaleCSVPath));
                        picker.AddChild(pickerButton);
                        localeGrid.AddChild(picker);
                        var deleteButton = new Button();
                        deleteButton.Text = "X";
                        deleteButton.Connect("pressed", this, nameof(CallRemoveLocale));
                        localeGrid.AddChild(deleteButton);
                    }

                    AddCustomControl(localeGrid);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                GD.PushError($"Error in {nameof(YarnProjectInspectorPlugin)}: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private void CallSelectLocaleCSVPath()
        {
            SelectLocaleCSVPath(_currLocale.Key);
        }

        private void CallRemoveLocale()
        {
            RemoveLocale(_currLocale.Key);
        }

        private void RemoveLocale(string localeCode)
        {
            GD.Print($"Removed locale code {localeCode}");
            _project.LocaleCodeToCSVPath.Remove(localeCode);
            //_project.NotifyPropertyListChanged();
        }

        private string _selectedLocaleCode;
        public void SelectLocaleCSVPath(string localeCode)
        {
            var dialog = new FileDialog();
            dialog.AddFilter("*.csv; CSV File");
            dialog.Mode = FileDialog.ModeEnum.SaveFile;
            dialog.Access = FileDialog.AccessEnum.Filesystem;
            dialog.WindowTitle = $"Select CSV Path for Locale {localeCode}";
            _selectedLocaleCode = localeCode;
            dialog.Connect("file_selected", this, nameof(CallCSVFileSelected));
            editorInterface.GetBaseControl().AddChild(dialog);
            dialog.Popup_(new Rect2(50, 50, 700, 500));
        }

        void CallCSVFileSelected(string savePath)
        {
            CSVFileSelected(savePath, _selectedLocaleCode);
        }


        public void CSVFileSelected(string savePath, string localeCode)
        {
            savePath = ProjectSettings.LocalizePath(savePath);
            GD.Print($"CSV file selected for locale {localeCode}: {savePath}");
            _project.LocaleCodeToCSVPath[localeCode] = savePath;
            //_project.NotifyPropertyListChanged();
        }

        private string _newLocale = null;
        private bool _addLocaleConnected;

        private AcceptDialog _acceptDialog;
        private LineEdit _textEntry;

        private void AddLocale()
        {
            _addLocaleConnected = false;
            _acceptDialog = new AcceptDialog();
            _acceptDialog.WindowTitle = "Add New Locale Code";
            SetNewLocaleText("", _acceptDialog.GetOk());
            _textEntry = new LineEdit();
            _acceptDialog.AddChild(_textEntry);
            _textEntry.PlaceholderText = "locale code";
            _textEntry.Connect("text_changed", this, nameof(CallSetNewLocaleText));
            editorInterface.GetBaseControl().AddChild(_acceptDialog);
            _acceptDialog.Popup_(new Rect2(50, 50, 400, 150));
            _textEntry.GrabFocus();
        }

        private void CallSetNewLocaleText(string text)
        {
            SetNewLocaleText(text, _acceptDialog.GetOk());
        }

        private void SetNewLocaleText(string localeCode, Button okButton)
        {
            _newLocale = localeCode;
            okButton.Disabled = string.IsNullOrEmpty(_newLocale);
            if (_addLocaleConnected)
            {
                okButton.Disconnect("pressed", this, nameof(LocaleAdded));
            }

            if (okButton.Disabled)
            {
                return;
            }

            okButton.Connect("pressed", this, nameof(CallLocaleAdded));
            _addLocaleConnected = true;
        }

        private void CallLocaleAdded()
        {
            LocaleAdded(_newLocale);
        }

        public void LocaleAdded(string localeCode)
        {
            _project.LocaleCodeToCSVPath[localeCode] = "";
            //_project.NotifyPropertyListChanged();
        }

        public override void ParseBegin(Godot.Object @object)
        {
            try
            {
                _project = (YarnProject) @object;
                YarnProjectEditorUtility.AddProjectToList(_project);
                if (_recompileButton != null)
                {
                    if (IsInstanceValid(_recompileButton))
                    {
                        _recompileButton.QueueFree();
                    }

                    _recompileButton = null;
                }

                _recompileButton = new Button();
                _recompileButton.Text = "Re-compile Scripts in Project";
                _recompileButton.Connect("pressed", this, nameof(CallOnRecompileClicked));
                AddCustomControl(_recompileButton);

                if (_addTagsButton != null)
                {
                    if (IsInstanceValid(_addTagsButton))
                    {
                        _addTagsButton.QueueFree();
                    }

                    _addTagsButton = null;
                }

                _addTagsButton = new Button();

                _addTagsButton.Text = "Add Line Tags to Scripts";
                _addTagsButton.Connect("pressed", this, nameof(CallOnTagsClicked));
                AddCustomControl(_addTagsButton);

                if (_updateLocalizationsButton != null)
                {
                    if (IsInstanceValid(_updateLocalizationsButton))
                    {
                        _updateLocalizationsButton.QueueFree();
                    }

                    _updateLocalizationsButton = null;
                }

                _updateLocalizationsButton = new Button();

                _updateLocalizationsButton.Text = "Update Localizations";
                _updateLocalizationsButton.HintTooltip = "Update Localization CSV and Godot .translation Files";
                _updateLocalizationsButton.Connect("pressed", this, nameof(OnUpdateLocalizationsClicked));
                AddCustomControl(_updateLocalizationsButton);
            }
            catch (Exception e)
            {
                GD.PushError($"Error in {nameof(YarnProjectInspectorPlugin)}: {e.Message}\n{e.StackTrace}");
            }
        }

        private void CallOnTagsClicked()
        {
            OnAddTagsClicked(_project);
        }

        private void CallOnRecompileClicked()
        {
            OnRecompileClicked(_project);
        }

        private void OnUpdateLocalizationsClicked()
        {
            YarnProjectEditorUtility.UpdateLocalizationCSVs(_project);
        }

        private void OnRecompileClicked(YarnProject project)
        {
            YarnProjectEditorUtility.UpdateYarnProject(project);
            _compileErrorsPropertyEditor.Refresh();
            //project.NotifyPropertyListChanged();
        }

        public void RenderCompilationErrors(Godot.Object yarnProject)
        {
            _project = (YarnProject) yarnProject;
            var errors = _project.ProjectErrors;
            SetErrors(errors);
            //yarnProject.NotifyPropertyListChanged();
        }

        public void RenderSourceScriptsList(Godot.Object yarnProject)
        {
            _project = (YarnProject) yarnProject;
            var scripts = _project.SourceScripts;
            SetSourceScripts(scripts);
            //yarnProject.NotifyPropertyListChanged();
        }

        private void SetErrors(YarnProjectError[] errors)
        {
            for (var i = _errorContainer.GetChildCount() - 1; i >= 0; i--)
            {
                var child = _errorContainer.GetChild(i);

                child.QueueFree();
            }

            var errorGroups = errors.GroupBy(e => e.FileName);
            foreach (var errorGroup in errorGroups)
            {
                var errorsInGroup = errorGroup.ToList();
                var fileNameLabel = _fileNameLabelScene.Instance<Label>();
                var resFileName = ProjectSettings.LocalizePath(errorsInGroup[0].FileName);
                fileNameLabel.Text = $"{resFileName}:";
                _errorContainer.AddChild(fileNameLabel);
                var separator = new HSeparator();
                separator.RectMinSize = new Vector2(0, 4);
                separator.SizeFlagsHorizontal = (int)Control.SizeFlags.Expand;
                _errorContainer.AddChild(separator);
                foreach (var err in errorsInGroup)
                {
                    var errorTextLabel = _errorTextLabelScene.Instance<Label>();
                    errorTextLabel.Text = $"    {err.Message}";
                    _errorContainer.AddChild(errorTextLabel);

                    var contextLabel = _contextLabelScene.Instance<Label>();
                    contextLabel.Text = $"    {err.Context}";
                    _errorContainer.AddChild(contextLabel);
                }
            }
        }

        private void SetSourceScripts(Array<string> sourceScripts)
        {
            _sourceScriptsListLabel.Text = "";
            foreach (var script in sourceScripts)
            {
                var resFileName = ProjectSettings.LocalizePath(script.Replace("\\", "/"));
                _sourceScriptsListLabel.Text += resFileName + "\n";
            }
        }

        private void OnAddTagsClicked(YarnProject project)
        {
            YarnProjectEditorUtility.AddLineTagsToFilesInYarnProject(project);
            _compileErrorsPropertyEditor.Refresh();
            //project.NotifyPropertyListChanged();
        }
    }
}
#endif