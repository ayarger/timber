using Godot;
using System;
using YarnSpinnerGodot;

public class YarnSelect : Button
{
    private PopupMenu projectSelect;

    public override void _Ready()
    {
        projectSelect = GetNode<PopupMenu>("PopupMenu");
        projectSelect.Name = "Yarn Projects";
        projectSelect.Connect("id_pressed", this, nameof(OnProjectSelected));
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
