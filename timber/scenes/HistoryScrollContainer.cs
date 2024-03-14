using Godot;

public class HistoryScrollContainer : ScrollContainer
{
	public override void _Ready()
	{
		foreach (Node child in GetChildren())
		{
			if (child is ScrollBar scrollbar)
			{
				scrollbar.MouseFilter = Control.MouseFilterEnum.Pass;
			}
		}
	}
}
