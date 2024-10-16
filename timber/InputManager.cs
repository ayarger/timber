using Godot;
using System;

public class InputManager : Node
{
	
	public override void _Ready()
	{
		
	}

	public override void _Input(InputEvent @event)
	{
		// Check if the event is an InputEventKey
		if (@event is InputEventKey eventKey)
		{
			// Check if the escape key was just pressed
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
			{
				// do something.
			}
		}
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}
