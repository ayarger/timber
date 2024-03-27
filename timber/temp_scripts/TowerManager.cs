// Assume that tower is a special type of actor that does not move, and has a ranged attack method. 

// Experiment: use temp art to generate and put a tower; make it follow mouse cursor
// Goal: make sure that two towers are not spawned in the same grid
//       cursor thing
//       auto-align
// Extra goal: juice

using Godot;
using System;

public class TowerManager : Node
{

	private bool isPlacingTower = false;
	private Texture customCursor;

	public override void _Ready()
	{
		SetProcessInput(true);
		customCursor = (Texture)ResourceLoader.Load("res://temp_scripts/TempTowerSprite.png");

	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Scancode == (uint)KeyList.X)
		{
			isPlacingTower = !isPlacingTower;
			if (isPlacingTower)
			{
				// Input.SetCustomMouseCursor(customCursor, Input.CursorShape.Arrow, new Vector2(0, 0));
			}
			else
			{
				// Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);
			}
		}
		
		if (isPlacingTower && @event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == (int)ButtonList.Left)
		{
			SpawnTower(SelectionSystem.GetTilePosition());
			isPlacingTower = false;
		}
	}
	
	void SpawnTower(Vector3 cursorPos)
	{
		Vector3 spawn_pos = new Vector3(cursorPos.x , 0, cursorPos.z );
		Coord cur = Grid.ConvertToCoord(spawn_pos);
		if (Grid.Get(cur).actor==null)
		{
			ActorConfig config = new ActorConfig();
			config.name = "Test Tower";
			config.map_code = 't';
			config.idle_sprite_filename = "cuff_idle.png";
		
			Tower new_tower = SpawnActorOfType(config, spawn_pos);
			EventBus.Publish(new TileDataLoadedEvent());
			
			// Copied from MoveToNearestTile()
			if (Grid.Get(cur).actor==null || Grid.Get(cur).actor == new_tower)
			{
				new_tower.currentTile.actor = null;
				Grid.Get(cur).actor = new_tower;
				new_tower.currentTile = Grid.Get(cur);
			}
		}
		else
		{
			ToastManager.SendToast(this, "You cannot put a tower on a non-empty grid.", ToastMessage.ToastType.Warning);
		}
		
	}
	
	Tower SpawnActorOfType(ActorConfig config, Vector3 position)
	{
		/* Spawn actor scene */
		PackedScene tower_scene = (PackedScene)ResourceLoader.Load("res://temp_scenes/tower.tscn");
		Spatial new_tower = (Spatial)tower_scene.Instance();
		new_tower.Name = config.name;
		AddChild(new_tower);

		Tower tower_script = new_tower as Tower;

		new_tower.GlobalTranslation = position;
		new_tower.AddChild(tower_script);
		tower_script.Configure(config);
		
		return tower_script;
	}
}
