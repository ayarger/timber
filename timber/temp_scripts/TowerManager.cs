using Godot;
using System;
using System.Collections.Generic;
using Amazon.CloudFront.Model;


// TODO: 4.11 - 4.18
//		fix button press issue ?
//		ranged projectile
//		ko animation sprite
//		currency
//		make different kind of tower - by disable/enable components. 
// cannot put at somewhere not discovered
// need units to build

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
	CombatConfig TowerRangeConfig = new CombatConfig(4, 10, 0.3f, 0.5f, 0.125f, 0.75f);

	public override void _Ready()
	{
		SetProcessInput(true);
	}

	public override void _Input(InputEvent @event)
	{
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
		Coord cur = Grid.ConvertToCoord(new Vector3(cursorPos.x , 0, cursorPos.z));
		Vector3 spawnPos = Grid.ConvertToWorldPos(cur);
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

		// To attract player and avoid being attacked
		config.team = "construction";
		config.statConfig = new StatConfig();
		config.statConfig.stats["health"] = 150;
		// for Tower only
		config.statConfig.stats["buildCost"] = 10;
		config.stateConfigs.Add(TowerRangeConfig);
		config.type = "tower";
	
		Tower new_tower = SpawnActorOfType(config, spawnPos);
		new_tower.Configure(config);
		new_tower.curr_coord = cur;
		SpawnAnimate(new_tower);
		EventBus.Publish(new TileDataLoadedEvent());
		
		// Copied from MoveToNearestTile()
		if (Grid.Get(cur).actor==null || Grid.Get(cur).actor == new_tower)
		{
			new_tower.currentTile.actor = null;
			Grid.Get(cur).actor = new_tower;
			new_tower.currentTile = Grid.Get(cur);
			Grid.Get(cur).value = 't';
		}
		
		TempCurrencyManager.DecreaseMoney(10);
		EventBus.Publish(new EventTowerStatusChange(Tower.TowerStatus.AwaitConstruction));

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
		public Tower.TowerType towerType;
		
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
	
	public void SpawnAnimate(Tower tower)
	{
		var tween = tower.GetNode<Tween>("Tween");
		var mesh = tower.GetNode<MeshInstance>("view/mesh");
		Vector3 startScale = new Vector3(1, 0, 1);
		Vector3 endScale = new Vector3(1, 1, 1); 

		float deltaY = (endScale.y - startScale.y) / 2.0f;
		Vector3 startPosition = mesh.Translation + new Vector3(0, -deltaY, 0);
		Vector3 endPosition = mesh.Translation; 

		mesh.Scale = startScale;
		mesh.Translation = startPosition;

		tween.InterpolateProperty(
			mesh,
			"scale",
			startScale,
			endScale,
			1.0f,
			Tween.TransitionType.Sine,
			Tween.EaseType.Out
		);

		tween.InterpolateProperty(
			mesh,
			"translation",
			startPosition,
			endPosition,
			1.0f, 
			Tween.TransitionType.Sine,
			Tween.EaseType.Out
		);

		tween.Start();
	}

	
	public void OnTowerPlacementButtonPressed()
	{
		if (status == TowerManagerStatus.isPlacingTower)
		{
			status = TowerManagerStatus.idle;
			EventBus.Publish(new EventCancelTowerPlacement());
			return;
		}
		
		status = TowerManagerStatus.isPlacingTower;
		if (TempCurrencyManager.CheckCurrencyGreaterThan(this, 10, true))
		{
			EventBus.Publish(new EventToggleTowerPlacement());
		}
		else
		{
			status = TowerManagerStatus.idle;
		}
	}
	
	public void OnTowerRemovalButtonPressed()
	{
		if (status == TowerManagerStatus.isRemovingTower)
		{
			status = TowerManagerStatus.idle;
			ToastManager.SendToast(this, "Tower removal canceled.", ToastMessage.ToastType.Notice, 1f);
			return;
		}
		status = TowerManagerStatus.isRemovingTower;
		ToastManager.SendToast(this, "Tower removal triggered.", ToastMessage.ToastType.Notice, 1f);
		EventBus.Publish(new EventCancelTowerPlacement());
	}

}
