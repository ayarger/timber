using Godot;

public class Tower : Actor
{
	public override void _Ready()
	{
		base._Ready();
		SetProcessInput(true);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Scancode == (uint)KeyList.Z)
		{
			GD.Print("Test Tower reaction.");
		}
		
	}
}
