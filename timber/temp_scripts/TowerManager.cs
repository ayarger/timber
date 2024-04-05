using Godot;
using System;
using System.Collections.Generic;

// Press T to toggle placement
// Press X to toggle removal

public class TowerManager : Node
{
	enum TowerManagerStatus
	{
		idle,
		isPlacingTower,
		isRemovingTower
	}

	private TowerManagerStatus status;
	public List<Tower> tower_spawn_positions = new List<Tower>();

	public override void _Ready()
	{
		SetProcessInput(true);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (uint)KeyList.T)
			{
				if (status == TowerManagerStatus.isPlacingTower) status = TowerManagerStatus.idle;
				else status = TowerManagerStatus.isPlacingTower;
				
				if (status == TowerManagerStatus.isPlacingTower)
				{
					// ToastManager.SendToast(this, "Tower placement triggered.", ToastMessage.ToastType.Notice, 1f);
					EventBus.Publish(new EventToggleTowerPlacement());
				}
				else
				{
					// ToastManager.SendToast(this, "Tower placement canceled.", ToastMessage.ToastType.Notice, 1f);
					EventBus.Publish(new EventCancelTowerPlacement());
				}
			} 
			else if (eventKey.Pressed && eventKey.Scancode == (uint)KeyList.X)
			{
				if (status == TowerManagerStatus.isRemovingTower) status = TowerManagerStatus.idle;
				else status = TowerManagerStatus.isRemovingTower;
				if (status == TowerManagerStatus.isRemovingTower)
				{
					// ToastManager.SendToast(this, "Tower removal triggered.", ToastMessage.ToastType.Notice, 1f);
					EventBus.Publish(new EventCancelTowerPlacement());
				}
				else
				{
					// ToastManager.SendToast(this, "Tower removal canceled.", ToastMessage.ToastType.Notice, 1f);
				}
			}
		}
		
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == (int)ButtonList.Left)
		{
			if (status == TowerManagerStatus.isPlacingTower)
			{
				status = TowerManagerStatus.idle;
				SpawnTower(SelectionSystem.GetTilePosition());
			} else if (status == TowerManagerStatus.isRemovingTower)
			{
				status = TowerManagerStatus.idle;
				RemoveTower(SelectionSystem.GetTilePosition());
			}
		}
	}
	
	void SpawnTower(Vector3 cursorPos)
	{
		Vector3 spawn_pos = new Vector3(cursorPos.x , 0, cursorPos.z );
		Coord cur = Grid.ConvertToCoord(spawn_pos);
		EventBus.Publish(new EventCancelTowerPlacement());
		if (Grid.Get(cur).actor != null)
		{
			// please ignore this, will fix
			ToastManager.SendToast(this, "Cannot put a tower on a non-empty grid.", ToastMessage.ToastType.Warning, 2f);
			return;
		}

		if (Grid.Get(cur).value != '.')
		{
			ToastManager.SendToast(this, "Cannot throw a tower into the void.", ToastMessage.ToastType.Warning, 2f);
			return;
		}
		// ToastManager.SendToast(this, "Tower coord: [" + cur.x + "," + cur.z + "]", ToastMessage.ToastType.Notice, 1f);
		ActorConfig config = new ActorConfig();
		config.name = "Test Tower";
		config.map_code = 't';
		config.idle_sprite_filename = "cuff_idle.png";

		config.team = "player";
		config.statConfig = new StatConfig();
		config.statConfig.stats["health"] = 150;
	
		Tower new_tower = SpawnActorOfType(config, spawn_pos);
		new_tower.Configure(config);
		EventBus.Publish(new TileDataLoadedEvent());
		
		// Copied from MoveToNearestTile()
		if (Grid.Get(cur).actor==null || Grid.Get(cur).actor == new_tower)
		{
			new_tower.currentTile.actor = null;
			Grid.Get(cur).actor = new_tower;
			new_tower.currentTile = Grid.Get(cur);
			Grid.Get(cur).value = 't';
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
		tower_spawn_positions.Add(tower_script);

		new_tower.GlobalTranslation = position;
		new_tower.AddChild(tower_script);
		
		return tower_script;
	}

	void RemoveTower(Vector3 cursorPos)
	{
		Vector3 pos = new Vector3(cursorPos.x , 0, cursorPos.z );
		Coord cur = Grid.ConvertToCoord(pos);
		status = TowerManagerStatus.idle;

		for (int i = 0; i < tower_spawn_positions.Count; i++)
		{
			Tower tower = tower_spawn_positions[i];
			if (Grid.Get(cur).actor!= null && Grid.Get(cur).actor.Equals(tower))
			{
				tower_spawn_positions.RemoveAt(i);
				tower.QueueFree();
				Grid.Get(cur).actor = null;
				Grid.Get(cur).value = '.';
				break;
			}
		}
	}

	public class EventToggleTowerPlacement
	{
		private Tower.TowerType towerType;
		
		public EventToggleTowerPlacement(Tower.TowerType _towerType = Tower.TowerType.Normal)
		{
			towerType = _towerType;
		}
	}
	
	public class EventCancelTowerPlacement
	{
		public EventCancelTowerPlacement()
		{
		}
	}

}
