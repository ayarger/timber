using Godot;
using System;
using YarnSpinnerGodot;

public class YarnSelect : Button
{
    private PopupMenu projectSelect;
    [Export] NodePath popupPath;

    public override void _Ready()
    {
        projectSelect = GetNode<PopupMenu>(popupPath);
        projectSelect.Name = "Yarn Projects";
    }

    private void OnProjectSelected(int id)
    {
        string selected = projectSelect.GetItemText(id);
        YarnSpinnerGodot.DialogueRunner dialogueRunner = GetTree().CurrentScene.FindNode("DialogueRunner", recursive: true) as YarnSpinnerGodot.DialogueRunner;
        dialogueRunner.SetProject(YarnManager.projects[selected]);
    }

    public void ShowPopupMenu()
    {
        if (YarnManager.projects.Keys.Count > 0)
        {
            projectSelect.Clear();
            foreach (string key in YarnManager.projects.Keys)
            {
                int id = projectSelect.GetItemCount();
                projectSelect.AddItem(key, id);
            }
            projectSelect.PopupCentered(Vector2.One * 300);
            projectSelect.Show();
            projectSelect.SetAsToplevel(true);
        }
    }
}
